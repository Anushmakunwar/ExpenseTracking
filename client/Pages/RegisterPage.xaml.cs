using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage; // For Preferences
using System.Net.Http;
using System.Net.Http.Json;
using System;

namespace ExpenseTrackingSystem
{
    public partial class RegisterPage : ContentPage
    {
        private readonly HttpClient _httpClient;

        public RegisterPage()
        {
            InitializeComponent();
            _httpClient = new HttpClient(); // Ideally inject this via Dependency Injection
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
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
                var response = await _httpClient.PostAsJsonAsync("http://localhost:5050/api/auth/register", new { email, password });

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Success", "Registration Successful!", "OK");
                    await Shell.Current.GoToAsync("//LoginPage");
                }
                else
                {
                    await DisplayAlert("Error", "Registration Failed. Try again!", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }
    }
}
