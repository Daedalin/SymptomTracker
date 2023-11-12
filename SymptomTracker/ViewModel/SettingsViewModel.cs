using Daedalin.Core.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymptomTracker.ViewModel
{
    internal class SettingsViewModel : ViewModelBase
    {
        public SettingsViewModel()
        {
            ViewTitle = "Einstellungen";
            SaveClick = new RelayCommand(SetKey);
            LogOutClick = new RelayCommand(() => FirebaseBll.Logout());
            GetKey();
        }

        public string Key
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }
        public RelayCommand SaveClick { get; set; }
        public RelayCommand LogOutClick { get; set; }

        #region Get Key
        private void GetKey()
        {
            Task.Run(async () =>
            {
                Key = await SecureStorage.Default.GetAsync("EncryptPassword");
            });
        }
        #endregion

        #region SetKey
        private async void SetKey()
        {
            await SecureStorage.Default.SetAsync("EncryptPassword", Key);
            var newKey = await SecureStorage.Default.GetAsync("EncryptPassword");
            await Shell.Current.DisplayAlert("Neuer Key", newKey, "OK");
        }
        #endregion
    }
}
