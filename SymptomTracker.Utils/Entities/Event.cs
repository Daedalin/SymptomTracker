using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SymptomTracker.Utils.Entities
{
    [JsonDerivedType(typeof(Event), typeDiscriminator: nameof(Event))]
    [JsonDerivedType(typeof(WorkRelatedEvent), typeDiscriminator: nameof(WorkRelatedEvent))]
    public class Event
    {
        public int ID { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public eEventType EventType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool FullTime { get; set; }
    }
}
