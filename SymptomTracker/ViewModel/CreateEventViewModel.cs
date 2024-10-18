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
        private bool m_SkipClosingCheck;
        private eEventType m_EventType;
        private List<string> m_Titles;

        #region Constructor
        public CreateEventViewModel(DateTime date, Event existingEvent)
        {
            Date = date;
            m_Id = existingEvent.ID;
            Title = existingEvent.Name;
            Where = existingEvent.Where;
            FullTime = existingEvent.FullTime;
            Quantity = existingEvent.Quantity;
            m_EventType = existingEvent.EventType;
            Description = existingEvent.Description;
            EndTime = existingEvent.EndTime ?? TimeSpan.Zero;
            PreparationMethod = existingEvent.PreparationMethod;
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
                OnPropertyChanged(nameof(IsFood));
                OnPropertyChanged(nameof(IsWorkRelated));
                GetTitle();
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
        public string Quantity
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }
        public string PreparationMethod
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }
        public string Where
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

        public bool IsFood => m_EventType == eEventType.Food;

        public bool Exists => m_Id != -1;

        #region Command
        public RelayCommand PerformSearch { get; set; }
        public RelayCommand SaveClick { get; set; }
        public RelayCommand DeleteClick { get; set; }
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
            DeleteClick = new RelayCommand(__OnDeleteClick);
            TakePhotoClick = new RelayCommand(__TakePhoto);
            PerformSearch = new RelayCommand(__OnPerformSearch);
            PickImageClick = new RelayCommand(__PickImage);
            DeleteImageClick = new RelayCommand(() => ImagePath = null);

            await GetTitle();

            OnPropertyChanged(nameof(HasImage));
            OnPropertyChanged(nameof(IsWindows));
        }

        private async Task GetTitle()
        {
            var TitleResult = await RealtimeDatabaseBll.GetLastTitles(m_EventType);
            Validate(TitleResult);
            m_Titles = TitleResult.Result == null ? new List<string>() : TitleResult.Result.OrderBy(t => t).ToList();
            __OnPerformSearch();
        }
        #endregion

        #region __Navigating
        private async void __Navigating(object sender, ShellNavigatingEventArgs e)
        {
            if ((e.Source == ShellNavigationSource.Pop || e.Source == ShellNavigationSource.PopToRoot)
                && e.Current.Location.OriginalString.Contains(nameof(CreateEventPage)))
            {
                if ((string.IsNullOrEmpty(Title) && string.IsNullOrEmpty(Description)) || m_SkipClosingCheck)
                    return;

                e.Cancel();

                if (await Shell.Current.DisplayAlert("Achtung", "Nicht gespeicherte Änderungen.\nMöchten sie wirklich schließen?", "Ja", "Nein"))
                {
                    m_SkipClosingCheck = true;
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

        #region __OnDeleteClick
        private async void __OnDeleteClick()
        {

            if (m_Id == -1)
                return;

            var dayResult = await RealtimeDatabaseBll.GetDay(Date);
            Validate(dayResult);

            if (!dayResult.Success || dayResult.Result == null)
                return;

            dayResult.Result.Events.RemoveAll(t => t.ID == m_Id);

            var Result = await RealtimeDatabaseBll.UpdateDay(dayResult.Result);
            if (Validate(Result))
            {               
                m_SkipClosingCheck = true;                
                await Shell.Current.Navigation.PopAsync();
            }
        }
        #endregion

        #region __OnSaveClick
        private async void __OnSaveClick()
        {
            var dayResult = await RealtimeDatabaseBll.GetDay(Date);
            Validate(dayResult);

            if (!dayResult.Success )
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
            currentEvent.Where = Where;
            currentEvent.Quantity = Quantity;
            currentEvent.PreparationMethod = PreparationMethod;

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
                m_SkipClosingCheck = true;
                if (m_EventType == eEventType.Symptom)
                    await Shell.Current.DisplayAlert("Du hast ein Symtom gespeichert", "Denk drann ob du das letzte essen genauer bescheiben musst.", "Ok");
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
