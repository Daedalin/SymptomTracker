using Daedalin.Core.MVVM.ViewModel;
using SymptomTracker.Page;
using SymptomTracker.Utils.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

        #region Constructor
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

            if (existingEvent.HasImage)
                __DownloadImage();

            __Ini();
        }

        public CreateEventViewModel(eEventType eventType)
        {
            m_Id = -1;
            Date = DateTime.Now;
            Title = String.Empty;
            m_EventType = eventType;
            Description = " ";
            EndTime = DateTime.Now.TimeOfDay;
            StartTime = DateTime.Now.TimeOfDay;

            __Ini();
        }
        #endregion

        #region Propertys
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
            set => SetProperty(value, __OnPerformSearch);
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
            set => SetProperty(value, () => OnPropertyChanged(nameof(HasImage)));
        }

        public bool HasImage => !string.IsNullOrEmpty(ImagePath);

        public bool IsWindows => DeviceInfo.Current.Platform == DevicePlatform.WinUI;
        public bool ShImage => Settings?.Rights.HasFlag(eRights.Foto) ?? false;

        public bool IsWorkRelated => m_EventType == eEventType.Stress || m_EventType == eEventType.Mood;

        #region Command
        public RelayCommand PerformSearch { get; set; }
        public RelayCommand SaveClick { get; set; }
        public RelayCommand TakePhotoClick { get; set; }
        public RelayCommand PickImageClick { get; set; }
        public RelayCommand DeleteImageClick { get; set; }
        #endregion
        #endregion

        #region private methods
        #region __Ini
        private async void __Ini()
        {
            ViewTitle = "Ereignis erstellen";
            TitleSearchResults = new List<string>();
            SettingsUpdate += __OnUpdateSettings;
            Shell.Current.Navigating += __Navigating;

            SaveClick = new RelayCommand(__OnSaveClick);
            TakePhotoClick = new RelayCommand(__TakePhoto);
            PerformSearch = new RelayCommand(__OnPerformSearch);
            PickImageClick = new RelayCommand(__PickImage);
            DeleteImageClick = new RelayCommand(() => ImagePath = null);

            var TitleResult = await RealtimeDatabaseBll.GetLastTitles(m_EventType);
            Validate(TitleResult);
            m_Titles = TitleResult.Result == null ? new List<string>() : TitleResult.Result.OrderBy(t => t).ToList();
            __OnPerformSearch();

            OnPropertyChanged(nameof(HasImage));
            OnPropertyChanged(nameof(IsWindows));
        }
        #endregion

        #region __Navigating
        private async void __Navigating(object sender, ShellNavigatingEventArgs e)
        {
            if ((e.Source == ShellNavigationSource.Pop || e.Source == ShellNavigationSource.PopToRoot)
                && e.Current.Location.OriginalString.Contains(nameof(CreateEventPage)))
            {
                if (string.IsNullOrEmpty(Title) && string.IsNullOrEmpty(Description))
                    return;

                e.Cancel();

                if (await Shell.Current.DisplayAlert("Achtung", "Nicht gespeicherte Änderungen.\nMöchten sie wirklich schließen?", "Ja", "Nein"))
                {
                    Title = null; Description = null;
                    await Shell.Current.Navigation.PopAsync();
                }
            }
        }
        #endregion

        #region __OnPerformSearch
        private void __OnPerformSearch()
        {
            if (m_Titles != null)
            {
                TitleSearchResults = m_Titles.FindAll(t => t != null && t.Contains(Title ?? String.Empty)).ToList();
                OnPropertyChanged(nameof(TitleSearchResults));
            }
        }
        #endregion

        #region __OnSaveClick
        private async void __OnSaveClick()
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

            Event currentEvent = __CreateOrGetEvent(day);

            currentEvent.Name = Title;
            currentEvent.FullTime = FullTime;
            currentEvent.EventType = m_EventType;
            currentEvent.Description = Description;
            currentEvent.EndTime = !FullTime ? EndTime : null;
            currentEvent.StartTime = !FullTime ? StartTime : null;
            currentEvent.HasImage = !string.IsNullOrEmpty(ImagePath);

            if (!string.IsNullOrEmpty(ImagePath))
            {
                var uploadeResult = await StorageBll.UploadImage(ImagePath, Date, currentEvent.ID);
                if (!Validate(uploadeResult))
                    return;
            }

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

        #region __CreateOrGetEvent
        private Event __CreateOrGetEvent(Day day)
        {
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

            return currentEvent;
        }
        #endregion

        #region __TakePhoto
        private async void __TakePhoto()
        {
            try
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
                    }
                }
            }
            catch (PermissionException pe)
            {
                await Shell.Current.DisplayAlert("Warning", pe.Message, "Ok");
            }
        }
        #endregion

        #region __PickImage
        private async void __PickImage()
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

        #region __DownloadImage
        private async void __DownloadImage()
        {
            var ImageResult = await StorageBll.DownloadImage(Date, m_Id);
            if (Validate(ImageResult))
                ImagePath = ImageResult.Result;
        }
        #endregion

        #region __OnUpdateSettings
        private void __OnUpdateSettings()
        {
            OnPropertyChanged(nameof(ShImage));
        }
        #endregion
        #endregion
    }
}
