using System;
using Xamarin.Forms;  // Make sure to include this namespace for Xamarin.Forms

namespace ExpenseTrakingSystem
{
    internal class DashboardPage : ContentPage  // Inherit from ContentPage
    {
        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (!Preferences.ContainsKey("AuthToken"))
            {
                Shell.Current.GoToAsync("//LoginPage");
            }
        }
    }
}
