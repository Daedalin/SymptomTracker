using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using SymptomTracker.Utils.Entities;

namespace SymptomTracker
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseLocalNotification(conf =>
                {
                    conf.AddAndroid(android =>
                    {
                        android.AddReminderChannel();
                    });
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        private static void AddReminderChannel(this IAndroidLocalNotificationBuilder android)
        {
            foreach (var Event in Enums.EventType)
            {
                if (Event.Key == eEventType.NotSet)
                    continue;
                
                android.AddChannel(new NotificationChannelRequest
                {
                    Id = $"Reminder_{Event.Key}",
                    Name = $"{Event.Value} Erinnerung",
                    Description = $"Alle Benachrigutgungne die dich an {Event.Value} erinnern sollen",
                });
            }
        }
    }
}