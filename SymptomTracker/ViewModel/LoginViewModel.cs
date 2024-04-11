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
        public LoginViewModel() : this(false)
        {
            Shell.Current.Navigating += __Navigating;
        }

        private void __Navigating(object sender, ShellNavigatingEventArgs e)
        {
            if ((e.Source == ShellNavigationSource.Pop || e.Source == ShellNavigationSource.PopToRoot)
                && e.Current.Location.OriginalString.Contains(nameof(LoginPage)))
            {
                if (!LoginBll.HasLogin)
                {
                    e.Cancel();
                }
            }
        }

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
        public bool IsBusy
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
            var Result = await LoginBll.Login();
            if (Validate(Result, false) && Result.Result)
                await Shell.Current.Navigation.PopAsync();
        }

        #region OnLoginClick
        private async void OnLoginClick()
        {
            IsBusy = true;
            OperatingResult<bool> Result;
            if (IsSignUp)
                Result = await LoginBll.CreateUser(Email, Password, DisplayName);
            else
                Result = await LoginBll.Login(Email, Password);
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
            IsBusy = false;
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
