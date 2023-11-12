using Daedalin.Core.MVVM.ViewModel;
using SymptomTracker.Utils.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymptomTracker.ViewModel
{
    internal class CreateEventViewModel : ViewModelBase
    {
        private eEventType m_EventType;
        private List<string> m_Titles;

        public CreateEventViewModel(eEventType eventType)
        {
            Date = DateTime.Now;
            Title = String.Empty;
            m_EventType = eventType;
            Description = String.Empty;
            EndTime = DateTime.Now.TimeOfDay;
            ViewTitle = "Ereignis erstellen";
            StartTime = DateTime.Now.TimeOfDay;
            TitleSearchResults = new List<string>();
            SaveClick = new RelayCommand(OnSaveClick);
            PerformSearch = new RelayCommand(OnPerformSearch);

            Task.Run(async () =>
            {
                var TitleResult = await FirebaseBll.GetLastTitles(eventType);
                Validate(TitleResult);
                m_Titles = TitleResult.Result == null ? new List<string>() : TitleResult.Result;
                OnPerformSearch();
            }).GetAwaiter().GetResult();
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
            set => SetProperty(value, () => SetProperty(value, nameof(Title)));
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

        public RelayCommand PerformSearch { get; set; }
        public RelayCommand SaveClick { get; set; }

        private void OnPerformSearch()
        {
            if (m_Titles != null)
            {
                TitleSearchResults = new List<string>(m_Titles.FindAll(t => t != null && t.Contains(Title)));
                OnPropertyChanged(nameof(TitleSearchResults));
            }
        }
        private async void OnSaveClick()
        {
            var dayResult = await FirebaseBll.GetDay(Date);
            Validate(dayResult);

            if (dayResult.Success)
            {
                Day day;
                if (dayResult.Result == null)
                {
                    day = new Day();
                    day.Date = Date;
                    day.IsHoliday = Date.DayOfWeek == DayOfWeek.Saturday || Date.DayOfWeek == DayOfWeek.Saturday;
                }
                else
                    day = dayResult.Result;

                var newEvent = new Event()
                {
                    Name = Title,
                    EndTime = EndTime,
                    StartTime = StartTime,
                    EventType = m_EventType,
                    Description = Description,
                };

                day.Events.Add(newEvent);

                var Result = await FirebaseBll.UpdateDay(day);
                if (Validate(Result))
                {
                    if (!m_Titles.Contains(Title) && !string.IsNullOrEmpty(Title))
                    {
                        var AddTitlesResult = await FirebaseBll.AddLastTitles(m_EventType, Title);
                        Validate(AddTitlesResult);
                    }

                    await Shell.Current.Navigation.PopAsync();
                }

            }
        }
    }
}
