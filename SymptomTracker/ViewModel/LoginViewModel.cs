using Daedalin.Core.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymptomTracker
{
    internal class LoginViewModel : ViewModelBase
    {
        public LoginViewModel()
        {
            LoginClick = new RelayCommand(OnLoginClick);
            SignUpClick = new RelayCommand(OnSignUpClick);
        }

        #region Propertys
        public string Email
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }
        public string Password
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }
        #endregion

        #region Commands
        public RelayCommand LoginClick { get; set; }
        public RelayCommand SignUpClick { get; set; }
        #endregion

        private void OnLoginClick()
        {

        }

        private void OnSignUpClick()
        {

        }
    }
}
