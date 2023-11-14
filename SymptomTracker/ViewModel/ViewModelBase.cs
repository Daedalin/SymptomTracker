using Daedalin.Core.MVVM.ViewModel;
using Daedalin.Core.OperationResult;
using SymptomTracker.BLL;
using SymptomTracker.ViewModel;
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
        public string m_ViewModel;

        public ViewModelBase() : base() 
        {
            m_ViewModel = this.GetType().Name;

            if(m_ViewModel == nameof(MainViewModel))
            {
                FirebaseBll.PlsLogin += PlsLogin;
            }

            HasLogin();
        }
               

        public static FirebaseBll FirebaseBll
        {
            get
            {
                if (m_FirebaseBll == null)
                    m_FirebaseBll = new FirebaseBll();
                return m_FirebaseBll;
            }
        }

        #region Validate
        public bool Validate<T>(OperatingResult<T> operatingResult)
        {
            return Validate(OperatingResult.StatusTransfer(operatingResult));
        }
        public bool Validate(OperatingResult operatingResult)
        {
            if (!operatingResult.Success)
            {
                if (string.IsNullOrEmpty(operatingResult.Message))
                    Shell.Current.DisplayAlert("Fehler: " + operatingResult.Division, operatingResult.ex.Message, "Ok");
                else
                    Shell.Current.DisplayAlert("Fehler: " + operatingResult.Division, operatingResult.Message, "Ok");
                return false;
            }

            if (!string.IsNullOrEmpty(operatingResult.Message))
            {
                Shell.Current.DisplayAlert(operatingResult.Division, operatingResult.Message, "Ok");
                return false;
            }
            return true;
        }
        #endregion

        private async void HasLogin()
        {
            var Result = await FirebaseBll.Login();
            Validate(Result);
            if (!Result.Result && m_ViewModel != nameof(LoginViewModel))
                PlsLogin();
        }
        private void PlsLogin()     
        {
            Shell.Current.Navigation.PushAsync(new LoginPage()
            {
                BindingContext = new LoginViewModel()
            });
        }
    }
}
