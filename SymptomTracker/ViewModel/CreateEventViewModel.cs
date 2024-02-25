using Daedalin.Core.MVVM.ViewModel;
using SymptomTracker.Utils.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SymptomTracker.ViewModel
{
    internal class CreateEventViewModel : ViewModelBase
    {
        private int m_Id;
        private eEventType m_EventType;
        private List<string> m_Titles;

        public CreateEventViewModel(DateTime date, Event existingEvent)
        {
            Date = date;
            m_Id = existingEvent.ID;
            Title = existingEvent.Name;
            FullTime = existingEvent.FullTime;
            m_EventType = existingEvent.EventType;
            Description = existingEvent.Description;
            EndTime = existingEvent.EndTime ?? TimeSpan.Zero;
            StartTime = existingEvent.StartTime ?? TimeSpan.Zero;
            if (IsWorkRelated)
                WorkRelated = (existingEvent as WorkRelatedEvent)?.WorkRelated ?? false;

            __Ini();
        }

        public CreateEventViewModel(eEventType eventType)
        {
            m_Id = -1;
            Date = DateTime.Now;
            Title = String.Empty;
            m_EventType = eventType;
            Description = String.Empty;
            EndTime = DateTime.Now.TimeOfDay;
            StartTime = DateTime.Now.TimeOfDay;

            __Ini();
        }


        public DateTime Date
        {
            get => GetProperty<DateTime>();
            set => SetProperty(value);
        }

        public TimeSpan StartTime
        {
            get => GetProperty<TimeSpan>();
            set => SetProperty(value);
        }

        public TimeSpan EndTime
        {
            get => GetProperty<TimeSpan>();
            set => SetProperty(value);
        }

        public List<KeyValuePair<eEventType, string>> Typs { get => Enums.EventType; }

        public KeyValuePair<eEventType, string> SelectedType
        {
            get
            {
                return Typs.FirstOrDefault(t => t.Key == m_EventType);
            }
            set
            {
                m_EventType = value.Key;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsWorkRelated));
            }
        }
        public string Title
        {
            get => GetProperty<string>();
            set => SetProperty(value, OnPerformSearch);
        }
        public string SelectedTitle
        {
            get => GetProperty<string>();
            set
            {
                SetProperty(value);
                if (value != null)
                    SetProperty(value, nameof(Title));
            }
        }
        public string Description
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }
        public List<string> TitleSearchResults
        {
            get => GetProperty<List<string>>();
            set => SetProperty(value);
        }
        public bool WorkRelated
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public bool FullTime
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public string ImagePath
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public ImageSource Image
        {
            get => GetProperty<ImageSource>();
            set => SetProperty(value);
        }

        public bool IsWorkRelated => m_EventType == eEventType.Stress || m_EventType == eEventType.Mood;

        public RelayCommand PerformSearch { get; set; }
        public RelayCommand SaveClick { get; set; }
        public RelayCommand TakePhotoClick { get; set; }
        public RelayCommand PickImageClick { get; set; }

        #region __Ini
        private async void __Ini()
        {
            ViewTitle = "Ereignis erstellen";
            TitleSearchResults = new List<string>();

            SaveClick = new RelayCommand(OnSaveClick);
            TakePhotoClick = new RelayCommand(TakePhoto);
            PerformSearch = new RelayCommand(OnPerformSearch);
            PickImageClick = new RelayCommand(PickImage);

            var TitleResult = await RealtimeDatabaseBll.GetLastTitles(m_EventType);
            Validate(TitleResult);
            m_Titles = TitleResult.Result == null ? new List<string>() : TitleResult.Result;
            OnPerformSearch();
        }
        #endregion

        #region OnPerformSearch
        private void OnPerformSearch()
        {
            if (m_Titles != null)
            {
                TitleSearchResults = m_Titles.FindAll(t => t != null && t.Contains(Title ?? String.Empty)).ToList();
                OnPropertyChanged(nameof(TitleSearchResults));
            }
        }
        #endregion

        #region OnSaveClick
        private async void OnSaveClick()
        {
            var dayResult = await RealtimeDatabaseBll.GetDay(Date);
            Validate(dayResult);

            if (!dayResult.Success)
                return;

            Day day;
            if (dayResult.Result == null)
            {
                day = new Day();
                day.Date = Date;
                day.IsHoliday = Date.DayOfWeek == DayOfWeek.Saturday || Date.DayOfWeek == DayOfWeek.Saturday;
            }
            else
                day = dayResult.Result;

            Event currentEvent;
            if (m_Id != -1)
            {
                currentEvent = day.Events.FirstOrDefault(t => t.ID == m_Id);
                if (IsWorkRelated && currentEvent is not WorkRelatedEvent)
                {
                    day.Events.Remove(currentEvent);
                    currentEvent = new WorkRelatedEvent()
                    {
                        ID = m_Id
                    };
                    day.Events.Add(currentEvent);
                }
                if (IsWorkRelated)
                    (currentEvent as WorkRelatedEvent).WorkRelated = WorkRelated;
            }
            else
            {
                if (IsWorkRelated)
                {
                    currentEvent = new WorkRelatedEvent()
                    {
                        WorkRelated = WorkRelated
                    };
                }
                else
                    currentEvent = new Event();

                day.Events.Add(currentEvent);
                currentEvent.ID = day.Events.Count() + 1;
            }

            currentEvent.Name = Title;
            currentEvent.FullTime = FullTime;
            currentEvent.EventType = m_EventType;
            currentEvent.Description = Description;
            currentEvent.EndTime = !FullTime ? EndTime : null;
            currentEvent.StartTime = !FullTime ? StartTime : null;

            var Result = await RealtimeDatabaseBll.UpdateDay(day);
            if (Validate(Result))
            {
                if (!m_Titles.Contains(Title) && !string.IsNullOrEmpty(Title))
                {
                    var AddTitlesResult = await RealtimeDatabaseBll.AddLastTitles(m_EventType, Title);
                    Validate(AddTitlesResult);
                }

                await Shell.Current.Navigation.PopAsync();
            }
        }
        #endregion

        #region TakePhoto
        public async void TakePhoto()
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                FileResult photo = await MediaPicker.Default.CapturePhotoAsync();

                if (photo != null)
                {
                    // save the file into local storage
                    string localFilePath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);

                    using Stream sourceStream = await photo.OpenReadAsync();
                    using FileStream localFileStream = File.OpenWrite(localFilePath);

                    await sourceStream.CopyToAsync(localFileStream);
                    ImagePath = localFilePath;
                    Image = ImageSource.FromStream(() => sourceStream);
                }
            }
        }
        #endregion

        #region PickImage
        public async void PickImage()
        {
            try
            {
                var result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images,
                    PickerTitle = "Image auswählen"
                });
                if (result != null)
                {
                    if (result.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                        result.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase))
                    {
                        using var stream = await result.OpenReadAsync();
                        Image = ImageSource.FromStream(() => stream);
                        ImagePath = result.FullPath;
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Fehler", ex.Message, "ok");
            }
        }
        #endregion
    }
}
