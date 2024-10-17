using Daedalin.Core.MVVM.ViewModel;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using SymptomTracker.Page;
using SymptomTracker.Utils.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymptomTracker.ViewModel
{
    internal class SettingsViewModel : ViewModelBase
    {
        private eEventType m_EventType;

        public SettingsViewModel()
        {
            ViewTitle = "Einstellungen";
            SaveClick = new RelayCommand(SetKey);
            UpdateDBClick = new RelayCommand(OnUpdateDB);
            LogOutClick = new RelayCommand(OnLogOutClick);
#if ANDROID
            ReminderClick = new RelayCommand(OnReminderClick);
            ReminderClearClick = new RelayCommandPara(OnReminderClearClick);
            ReminderClearAllClick = new RelayCommand(OnReminderClearAllClick);
#endif
            GetKey();
        }
#if ANDROID
        public ObservableCollection<NotificationRequest> Notifications
        {
            get => GetProperty<ObservableCollection<NotificationRequest>>();
            set => SetProperty(value);
        }
#endif
        public string Key
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }
        public TimeSpan StartTime
        {
            get => GetProperty<TimeSpan>();
            set => SetProperty(value);
        }
        public List<KeyValuePair<eEventType, string>> Typs { get => Enums.EventType; }
        public KeyValuePair<eEventType, string> SelectedType
        {
            get
            {
                return Typs.FirstOrDefault(t => t.Key == m_EventType);
            }
            set
            {
                m_EventType = value.Key;
                OnPropertyChanged();
            }
        }
        public RelayCommand SaveClick { get; set; }
        public RelayCommand LogOutClick { get; set; }
        public RelayCommand UpdateDBClick { get; set; }
        public RelayCommand ReminderClick { get; set; }
        public RelayCommandPara ReminderClearClick { get; set; }
        public RelayCommand ReminderClearAllClick { get; set; }

        public override async void OnAppearing()
        {
#if ANDROID
            var result = await LocalNotificationCenter.Current.GetPendingNotificationList();
            Notifications = new ObservableCollection<NotificationRequest>(result);
#endif
            base.OnAppearing();
        }
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

#if ANDROID
        private void OnReminderClearClick(object parra)
        {
            if (!int.TryParse(parra?.ToString(), out var ID))
                return;

            LocalNotificationCenter.Current.Cancel(ID);
            OnAppearing();
        }

        private void OnReminderClearAllClick()
        {
            LocalNotificationCenter.Current.CancelAll();
            Notifications.Clear();
        }

        private async void OnReminderClick()
        {
            DateTime time = DateTime.Today;
            time = time.AddMilliseconds(StartTime.TotalMilliseconds);

            if (time <= DateTime.Now)
                time = time.AddDays(1);

            if (await LocalNotificationCenter.Current.AreNotificationsEnabled() == false)
            {
                await LocalNotificationCenter.Current.RequestNotificationPermission();
            }

            var type = Enums.EventType.First(t => t.Key == m_EventType).Value;

            var notification = new NotificationRequest
            {
                NotificationId = (int)m_EventType,
                Title = $"Hast du heute schon {type} eingetragen",
                Schedule = new NotificationRequestSchedule
                {
                    RepeatType = NotificationRepeat.Daily,
                    NotifyTime = time.AddSeconds(5),
                },
                Android = new AndroidOptions
                {
                    ChannelId = $"Reminder_{m_EventType}",
                }
            };

            await LocalNotificationCenter.Current.Show(notification);
            await Shell.Current.DisplayAlert("Neue Erinnerung", $"Für {type} wurde auf {time} gesetzt", "Ok");
            Notifications.Add(notification);
        }
#endif
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
