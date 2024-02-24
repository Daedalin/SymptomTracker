using SymptomTracker.Page;
using SymptomTracker.Utils.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SymptomTracker.ViewModel
{
    internal class DayViewModel : ViewModelBase
    {
        private const string Base_Title = "Ereignisse von Tag - ";
        public DayViewModel()
        {
            Date = DateTime.Today;
            EditClick = new Command<int>(__OnEditClick);
        }

        public DateTime Date
        {
            get => GetProperty<DateTime>();
            set => SetProperty(value);
        }
        public List<Event> Events
        {
            get
            {
                var events = GetProperty<List<Event>>();

                return events == null ? new List<Event>() : events;
            }
            set => SetProperty(value);
        }
        public bool IsHolyday
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public ICommand EditClick { get; set; }


        public override void OnAppearing()
        {
            __OnDateChange();
            base.OnAppearing();
        }

        private async void __OnEditClick(int Event)
        {
            var clickedEvent = Events.FirstOrDefault(t => t.ID == Event);

            if (clickedEvent == null)
                return;

            await Shell.Current.Navigation.PushAsync(new CreateEventPage()
            {
                BindingContext = new CreateEventViewModel(Date, clickedEvent)
            });
        }

        private async void __OnDateChange()
        {
            ViewTitle = Base_Title + Date.ToString("dd. MMM yyyy");
            var GetDayReult = await FirebaseBll.GetDay(Date);
            if (Validate(GetDayReult) && GetDayReult?.Result?.Events != null)
            {
                Events = GetDayReult.Result.Events;
                IsHolyday = GetDayReult.Result.IsHoliday;
            }
            else
                Events = new List<Event>();

        }
    }
}
