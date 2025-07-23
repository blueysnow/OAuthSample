// Paula Aliu
// OAuthMAUI
// GoogleDrivePage.xaml.cs
// 2024

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OAuthMAUI.Services.CloudProviders;

namespace OAuthMAUI;

public partial class GoogleDrivePage : ContentPage
{
    public GoogleDrivePage()
    {
        InitializeComponent();
        googleDriveProvider = new GoogleDriveProvider();
    }

    private readonly GoogleDriveProvider googleDriveProvider;

    private async void Login_OnClicked(object? sender, EventArgs e)
    {
        var isSignedIn = await googleDriveProvider.SignInAsync();
        if (isSignedIn)
        {
            await DisplayAlert("Success", "You have successfully signed in", "Ok");
            var user = await googleDriveProvider.GetUserDetailsAsync();
            if (string.IsNullOrEmpty(user.DisplayName))
            {
                await DisplayAlert("Error", "An error occurred while fetching user info", "Ok");
            }
            else
            {
                await DisplayAlert("User Info", $"Name: {user.DisplayName}\nEmail: {user.Email}\nUsername: {user.Username}\n", "Ok");
            }
        }
        else
        {
            await DisplayAlert("Error", "An error occurred while signing in", "Ok");
        }
    }

    private async void LogOut_OnClicked(object? sender, EventArgs e)
    {
        var isSignedOut = await googleDriveProvider.SignOutAsync();
        if (isSignedOut)
        {
            await DisplayAlert("Success", "You have successfully signed out", "Ok");
        }
        else
        {
            await DisplayAlert("Error", "An error occurred while signing out", "Ok");
        }
    }
}

