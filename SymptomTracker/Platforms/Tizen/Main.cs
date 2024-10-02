using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using System;

namespace SymptomTracker
{
    internal class Program : MauiApplication
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp().Build();

        static void Main(string[] args)
        {
            var app = new Program();
            app.Run(args);
        }
    }
}