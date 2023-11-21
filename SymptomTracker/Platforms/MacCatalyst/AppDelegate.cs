using Foundation;

namespace SymptomTracker
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp()
        {
            var builder = MauiProgram.CreateDefaultMauiAppBuilder();

            return builder.Build();
        }
    }
}