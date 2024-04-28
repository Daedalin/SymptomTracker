using Daedalin.Core.MVVM.ViewModel;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using SymptomTracker.Page;
using SymptomTracker.Utils.Entities;
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
            UpdateDBClick = new RelayCommand(OnUpdateDB);
            LogOutClick = new RelayCommand(OnLogOutClick);
            ReminderClick = new RelayCommand(OnReminderClick);
            GetKey();
        }

        public string Key
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }
        public RelayCommand SaveClick { get; set; }
        public RelayCommand LogOutClick { get; set; }
        public RelayCommand UpdateDBClick { get; set; }
        public RelayCommand ReminderClick { get; set; }

        private void OnLogOutClick()
        {
            Shell.Current.Navigation.PopAsync(false);
            LoginBll.Logout();
        }

        private async void OnUpdateDB()
        {
            UpdateDBClick.IsEnabled = false;
            var result = await RealtimeDatabaseBll.UpdateDB();
            Validate(result);
            UpdateDBClick.IsEnabled = true;
        }

        private async void OnReminderClick()
        {
            eEventType eEvent = eEventType.Symptom;
            DateTime time = DateTime.Now;

            if (await LocalNotificationCenter.Current.AreNotificationsEnabled() == false)
            {
                await LocalNotificationCenter.Current.RequestNotificationPermission();
            }

            var notification = new NotificationRequest
            {
                NotificationId = (int)eEvent,
                Title = $"Hast du heute schon {Enums.EventType.First(t => t.Key == eEvent).Value} eingetragen",
                Schedule = new NotificationRequestSchedule
                {
                    RepeatType = NotificationRepeat.Daily,   
                    NotifyTime = time.AddSeconds(5),
                },
                Android = new AndroidOptions
                {
                    ChannelId = $"Reminder_{eEvent}",
                }
            };

            await LocalNotificationCenter.Current.Show(notification);
        }

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
