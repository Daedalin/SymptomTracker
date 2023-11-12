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
            ShowSettingsClick = new RelayCommand(OnShowSettingsClick);


            Shell.Current.Navigation.PushAsync(new LoginPage()
            {
                BindingContext = new LoginViewModel()
            });
        }



        #region Command
        public RelayCommandPara CreateEventClick { get; set; }
        public RelayCommand ShowDayClick { get; set; }
        public RelayCommand ShowSettingsClick { get; set; }
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

        #region OnShowDayClick
        public async void OnShowSettingsClick()
        {
            await Shell.Current.Navigation.PushAsync(new SettingPage()
            {
                BindingContext = new SettingsViewModel()
            });
        }
        #endregion

        
    }
}
