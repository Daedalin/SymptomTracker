using Daedalin.Core.MVVM.ViewModel;
using SymptomTracker.BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SymptomTracker
{
    internal class ViewModelBase : DaedalinBaseViewModel
    {

        public FirebaseBll m_FirebaseBll { get; set; }
        public FirebaseBll FirebaseBll
        {
            get => m_FirebaseBll == null ? new FirebaseBll() : m_FirebaseBll;
        }
    }
}
