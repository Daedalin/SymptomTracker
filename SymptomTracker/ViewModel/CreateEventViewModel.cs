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
            Typs = new List<KeyValuePair<int, string>>
            {
                new KeyValuePair<int, string>(0, "Nichts"),
                new KeyValuePair<int, string>(1, "Essen"),
                new KeyValuePair<int, string>(2, "Symtom"),
                new KeyValuePair<int, string>(3, "Stress")
            };

            TitleSearchResults = new List<string>();
            PerformSearch = new RelayCommand(OnPerformSearch);
            SaveClick = new RelayCommand(OnSaveClick);
            Title = String.Empty;
            Date = DateTime.Now;
            StartTime = DateTime.Now.TimeOfDay;
            EndTime = DateTime.Now.TimeOfDay;

            Task.Run(async () =>
            {
                var TitleResult = await FirebaseBll.GetLastTitles(eventType);
                Validate(TitleResult);
                m_Titles = TitleResult.Result;
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

        public List<KeyValuePair<int, string>> Typs { get; set; }

        public KeyValuePair<int, string> SelectedType
        {
            get
            {
                return Typs.FirstOrDefault(t => t.Key == (int)m_EventType);
            }
            set
            {
                m_EventType = (eEventType)value.Key;
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
                TitleSearchResults = new List<string>(m_Titles.FindAll(t => t.Contains(Title)));
                OnPropertyChanged(nameof(TitleSearchResults));
            }
        }
        private void OnSaveClick()
        {
            new Event()
            {
                Name = Title,
                EndTime = EndTime,
                StartTime = StartTime,
                EventType = m_EventType,
                Description = Description,
            };
            
        }
    }
}
