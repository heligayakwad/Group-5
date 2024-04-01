﻿using WeatherApp.Views;

namespace WeatherApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(LoginView), typeof(LoginView));
            Routing.RegisterRoute(nameof(SignupView), typeof(SignupView));
            Routing.RegisterRoute(nameof(HomeView), typeof(HomeView));
        }
    }
}
