﻿using SymptomTracker.ViewModel;

namespace SymptomTracker
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new MainPage()
            {
                BindingContext = new MainViewModel()
            });
        }
    }
}