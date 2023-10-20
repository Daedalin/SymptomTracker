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

        public FirebaseBll m_FirebaseBll { get; set; }
        public FirebaseBll FirebaseBll
        {
            get => m_FirebaseBll == null ? new FirebaseBll() : m_FirebaseBll;
        }

        public void Validate<T>(OperatingResult<T> operatingResult)
        {
            Validate(OperatingResult.StatusTransfer(operatingResult));
        }
        public void Validate(OperatingResult operatingResult)
        {
            if (!operatingResult.Success)
            {
                Shell.Current.DisplayAlert("Fehler: "+operatingResult.Division, operatingResult.Message,"Ok");
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
