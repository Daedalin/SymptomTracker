using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymptomTracker.Utils.Entities
{
    public class Day
    {
        public Day()
        {
            Events = new List<Event>();
        }

        public DateOnly Date { get; set; }
        public bool IsHoliday { get; set; }
        public List<Event> Events { get; set; }
    }
}
