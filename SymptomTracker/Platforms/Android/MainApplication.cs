using Android.App;
using Android.Runtime;
using Microsoft.Maui.LifecycleEvents;

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
            var builder = MauiProgram.CreateDefaultMauiAppBuilder();
                                     //.RegisterFirebaseServices();


            return builder.Build();
        }

       

    }
}