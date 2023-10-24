using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymptomTracker.Utils.Entities
{
    public class Event
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public eEventType EventType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
