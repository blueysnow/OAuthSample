// Paula Aliu
// OAuthMAUI
// OneDrivePage.xaml.cs
// 2024

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OAuthMAUI.Services.CloudProviders;

namespace OAuthMAUI;

public partial class OneDrivePage : ContentPage
{
    private readonly OneDriveProvider oneDriveProvider;

    public OneDrivePage()
    {
        InitializeComponent();
        oneDriveProvider = new OneDriveProvider();
    }

    private async void Login_OnClicked(object? sender, EventArgs e)
    {
        var isSignedIn = await oneDriveProvider.SignInAsync();
        if (isSignedIn)
        {
            await DisplayAlert("Success", "You have successfully signed in", "Ok");
            var user = await oneDriveProvider.GetUserDetailsAsync();
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
        var isSignedOut = await oneDriveProvider.SignOutAsync();
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

