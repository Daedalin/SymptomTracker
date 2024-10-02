using Android.App;
using Android.Runtime;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using SymptomTracker.Utils.Entities;

namespace SymptomTracker
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override MauiApp CreateMauiApp()
        {
            var builder = MauiProgram.CreateMauiApp();
            builder.UseLocalNotification( conf =>
            {
                conf.AddAndroid(android =>
                {
                    android.AddReminderChannel();
                });
            });
            return builder.Build();
        }
    }
}