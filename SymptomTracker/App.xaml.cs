﻿using SymptomTracker.Page;
using SymptomTracker.ViewModel;

namespace SymptomTracker
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new MainMenu();

        }
    }
}