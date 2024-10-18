using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using SymptomTracker.Utils.Entities;
using CommunityToolkit.Maui;

namespace SymptomTracker
{
    public static class MauiProgram
    {
        public static MauiAppBuilder CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder;
        }
        public static void AddReminderChannel(this IAndroidLocalNotificationBuilder android)
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