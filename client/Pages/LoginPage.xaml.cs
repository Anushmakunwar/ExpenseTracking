using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage; // For Preferences
using System.Net.Http;
using System.Net.Http.Json;
using System;

namespace ExpenseTrackingSystem
{
    public partial class LoginPage : ContentPage
    {
        private readonly HttpClient _httpClient;

        public LoginPage()
        {
            InitializeComponent();
            _httpClient = new HttpClient(); // Ideally, inject this via Dependency Injection
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            string email = EmailEntry.Text;
            string password = PasswordEntry.Text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                await DisplayAlert("Error", "Email and Password cannot be empty!", "OK");
                return;
            }

            try
            {
                // Send data to backend API
                var response = await _httpClient.PostAsJsonAsync("http://localhost:5050/api/auth/login", new { email, password });

                if (response.IsSuccessStatusCode)
                {
                    // Save token (if any)
                    var token = await response.Content.ReadAsStringAsync();
                    Preferences.Set("AuthToken", token);

                    await DisplayAlert("Success", "Login Successful!", "OK");
                    await Shell.Current.GoToAsync("//DashboardPage");
                }
                else
                {
                    await DisplayAlert("Error", "Login Failed! Please check your credentials.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }
    }
}
