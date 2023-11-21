using Microsoft.Maui.LifecycleEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymptomTracker.Platforms.Android
{
    internal static class FirebaseServiceHelper
    {
        //private static MauiAppBuilder RegisterFirebaseServices(this MauiAppBuilder builder)
        //{
        //    builder.ConfigureLifecycleEvents(events => {
        //        events.AddAndroid(android => android.OnCreate((activity, _) =>
        //            CrossFirebase.Initialize(activity, CreateCrossFirebaseSettings())));
        //    });

        //    builder.Services.AddSingleton(_ => CrossFirebaseAuth.Current);
        //    return builder;
        //}
        //private static CrossFirebaseSettings CreateCrossFirebaseSettings()
        //{
        //    return new CrossFirebaseSettings(isAuthEnabled: true);
        //}
    }
}
