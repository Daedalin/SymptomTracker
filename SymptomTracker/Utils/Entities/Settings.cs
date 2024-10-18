using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymptomTracker.Utils.Entities
{
    public class Settings
    {
        public int DB_Version {  get; set; }

        public eRights Rights { get; set;}
    }
}
