using Foundation;
using Microsoft.Maui;

namespace SymptomTracker
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp().Build();
    }
}