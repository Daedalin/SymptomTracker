using SymptomTracker.Utils.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymptomTracker.ViewModel
{
    internal class DayViewModel : ViewModelBase
    {
        private const string Base_Title = "Ereignisse von Tag - ";
        public DayViewModel()
        {
            Date = DateTime.Today;
        }

        public DateTime Date
        {
            get => GetProperty<DateTime>();
            set => SetProperty(value, OnDateChange);
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

        private async void OnDateChange()
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
