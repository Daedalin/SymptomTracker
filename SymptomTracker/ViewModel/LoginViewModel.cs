using Daedalin.Core.MVVM.ViewModel;
using Daedalin.Core.OperationResult;
using SymptomTracker.Page;
using SymptomTracker.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymptomTracker
{
    internal class LoginViewModel : ViewModelBase
    {
        public LoginViewModel() : this(false) { }

        public LoginViewModel(bool _IsSignUp)
        {
            IsSignUp = _IsSignUp;
            if (IsSignUp)
            {
                ViewTitle = "Regestrieren";
            }
            else
            {
                HasLogin();
                ViewTitle = "Login";
            }
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
        public string DisplayName
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }
        public bool IsSignUp
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }
        #endregion

        #region Commands
        public RelayCommand LoginClick { get; set; }
        public RelayCommand SignUpClick { get; set; }
        #endregion

        #region Methods
        private async void HasLogin()
        {
            var Result = await RealtimeDatabaseBll.Login();
            if (Validate(Result, false) && Result.Result)
                await Shell.Current.Navigation.PopAsync();
        }

        #region OnLoginClick
        private async void OnLoginClick()
        {
            OperatingResult<bool> Result;
            if (IsSignUp)
                Result = await RealtimeDatabaseBll.CreateUser(Email, Password, DisplayName);
            else
                Result = await RealtimeDatabaseBll.Login(Email, Password);
            if (Validate(Result) && Result.Result)
            {
                var CheckKey = await RealtimeDatabaseBll.IsKeyOK();
                if (Validate(CheckKey, false) && !CheckKey.Result)
                {
                    await Shell.Current.Navigation.PushAsync(new SettingPage()
                    {
                        BindingContext = new SettingsViewModel()
                    });
                    ((AppShell)Shell.Current).RemovePage<LoginPage>();
                }
                else
                    await Shell.Current.Navigation.PopAsync();
            }
        }
        #endregion

        #region OnSignUpClick
        private async void OnSignUpClick()
        {
            await Shell.Current.Navigation.PushAsync(new LoginPage()
            {
                BindingContext = new LoginViewModel(true)
            });
            ((AppShell)Shell.Current).RemovePage<LoginPage>();
        }
        #endregion
        #endregion
    }
}
