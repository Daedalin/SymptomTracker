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
        private static RealtimeDatabaseBll m_RealtimeDatabaseBll;
        public string m_ViewModel;

        public ViewModelBase() : base()
        {
            m_ViewModel = this.GetType().Name;

            if (m_ViewModel == nameof(MainViewModel))
            {
                RealtimeDatabaseBll.PlsLogin += PlsLogin;
            }

            HasLogin();
        }


        public static RealtimeDatabaseBll RealtimeDatabaseBll
        {
            get
            {
                if (m_RealtimeDatabaseBll == null)
                    m_RealtimeDatabaseBll = new RealtimeDatabaseBll();
                return m_RealtimeDatabaseBll;
            }
        }

        public virtual void OnAppearing() { }

        #region Validate
        public bool Validate<T>(OperatingResult<T> operatingResult) => Validate<T>(operatingResult, true);

        public bool Validate<T>(OperatingResult<T> operatingResult, bool ShMessage)
        {
            var result = OperatingResult.StatusTransfer(operatingResult);
            //Bug fix
            result.ex = operatingResult.ex;
            return Validate(result, ShMessage);
        }

        public bool Validate(OperatingResult operatingResult) => Validate(operatingResult, true);
        public bool Validate(OperatingResult operatingResult, bool ShMessage)
        {
            if (!operatingResult.Success)
            {
                if (ShMessage)
                {
                    if (string.IsNullOrEmpty(operatingResult.Message))
                        Shell.Current.DisplayAlert("Fehler: " + operatingResult.Division, operatingResult.ex.Message, "Ok");
                    else
                        Shell.Current.DisplayAlert("Fehler: " + operatingResult.Division, operatingResult.Message, "Ok");
                }
                return false;
            }

            if (!string.IsNullOrEmpty(operatingResult.Message))
            {
                if (ShMessage)
                    Shell.Current.DisplayAlert(operatingResult.Division, operatingResult.Message, "Ok");
                return false;
            }
            return true;
        }
        #endregion

        private async void HasLogin()
        {
            var Result = await RealtimeDatabaseBll.Login();
            Validate(Result, false);
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
