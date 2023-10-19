using SymptomTracker.BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SymptomTracker
{
    internal class ViewModelBase
    {

        public FirebaseBll m_Bll { get; set; }
        public FirebaseBll Bll { get => m_Bll == null ? new FirebaseBll() : m_Bll; }
    }
}
