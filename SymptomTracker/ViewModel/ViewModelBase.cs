using Daedalin.Core.MVVM.ViewModel;
using Daedalin.Core.OperationResult;
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
        private static FirebaseBll m_FirebaseBll;

        public ViewModelBase() : base() { }

        public static FirebaseBll FirebaseBll
        {
            get
            {
                if (m_FirebaseBll == null)
                    m_FirebaseBll = new FirebaseBll();
                return m_FirebaseBll;
            }
        }

        public void Validate<T>(OperatingResult<T> operatingResult)
        {
            Validate(OperatingResult.StatusTransfer(operatingResult));
        }
        public void Validate(OperatingResult operatingResult)
        {
            if (!operatingResult.Success)
            {
                Shell.Current.DisplayAlert("Fehler: " + operatingResult.Division, operatingResult.Message, "Ok");
                return;
            }

            if (!string.IsNullOrEmpty(operatingResult.Message))
            {
                Shell.Current.DisplayAlert(operatingResult.Division, operatingResult.Message, "Ok");
                return;
            }
        }
    }
}
