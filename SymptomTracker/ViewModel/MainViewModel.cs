using Daedalin.Core.MVVM.ViewModel;
using SymptomTracker.Page;
using SymptomTracker.Utils.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymptomTracker.ViewModel
{
    class MainViewModel : ViewModelBase
    {

        public MainViewModel()
        {
            ShowDayClick = new RelayCommand(OnShowDayClick);
            CreateEventClick = new RelayCommandPara(OnCreateEventClick);
            Save = new RelayCommand(SetKey);
            GetKey();

            Shell.Current.Navigation.PushAsync(new LoginPage()
            {
                BindingContext = new LoginViewModel()
            });
        }

        public string Key
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        #region Command
        public RelayCommandPara CreateEventClick { get; set; }
        public RelayCommand ShowDayClick { get; set; }
        public RelayCommand Save { get; set; }
        #endregion

        #region OnCreateEventClick
        public async void OnCreateEventClick(object para)
        {
            if (para != null && para is string strPara && Int32.TryParse(strPara, out int intPara))
            {
                await Shell.Current.Navigation.PushAsync(new CreateEventPage()
                {
                    BindingContext = new CreateEventViewModel((eEventType)intPara)
                });
            }
        }
        #endregion

        #region OnShowDayClick
        public async void OnShowDayClick()
        {
            await Shell.Current.Navigation.PushAsync(new DayPage()
            {
                BindingContext = new DayViewModel()
            });
        }
        #endregion

        private void GetKey()
        {
            Task.Run(async () =>
            {
                Key = await SecureStorage.Default.GetAsync("EncryptPassword");
            });
        }

        private void SetKey()
        {
            Task.Run(() =>
            {
                SecureStorage.Default.SetAsync("EncryptPassword", Key).GetAwaiter().GetResult();
                Key = String.Empty;
                Task.Delay(1000).GetAwaiter().GetResult();
                GetKey();
            });
        }
    }
}
