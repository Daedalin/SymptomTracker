using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymptomTracker.Utils.Entities
{
    public static class Enums
    {
        public static List<KeyValuePair<eEventType, string>> EventType = new List<KeyValuePair<eEventType, string>>
        {
            new KeyValuePair<eEventType, string>(eEventType.NotSet, "Nichts"),
            new KeyValuePair<eEventType, string>(eEventType.Food, "Essen"),
            new KeyValuePair<eEventType, string>(eEventType.Symptom, "Symtom"),
            new KeyValuePair<eEventType, string>(eEventType.Stress, "Stress")
        };
    }

    public enum eEventType
    {
        NotSet = 0,
        Food = 1,
        Symptom = 2,
        Stress = 3,
    }
}
