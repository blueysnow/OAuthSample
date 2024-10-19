// Paula Aliu
// OAuthMAUI
// DropBoxPage.xaml.cs
// 2024

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.DependencyInjection;

using OAuthMAUI.Services.CloudProviders;

namespace OAuthMAUI;

public partial class DropBoxPage : ContentPage
{
    private DropBoxProvider dropBoxProvider;
    public DropBoxPage()
    {
        dropBoxProvider =  Ioc.Default.GetService<DropBoxProvider>() ??  new DropBoxProvider();
        InitializeComponent();
    }

    private async  void Login_OnClicked(object? sender, EventArgs e)
    {
        bool isSignedIn = await dropBoxProvider.SignInAsync();
        if(isSignedIn)
        {
            await DisplayAlert("Success", "You have successfully signed in", "Ok");
            var user = dropBoxProvider.CurrentUser;
            if(string.IsNullOrEmpty(user.DisplayName))
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
        _ = await dropBoxProvider.SignOutAsync();
    }
}

