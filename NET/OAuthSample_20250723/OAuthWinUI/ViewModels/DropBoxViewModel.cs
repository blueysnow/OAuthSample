using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml.Controls;

using OAuthWinUI.Services.CloudProviders;

namespace OAuthWinUI.ViewModels;

public partial class DropBoxViewModel : ObservableRecipient
{
    private readonly DropBoxProvider dropBoxProvider;

    public DropBoxViewModel() => dropBoxProvider = new DropBoxProvider();

    [RelayCommand]
    private async void Login()
    {
        var contentDialog = new ContentDialog();
        contentDialog.XamlRoot = App.MainWindow.Content.XamlRoot;
        contentDialog.PrimaryButtonText = "Ok";
        var isSignedIn = await dropBoxProvider.SignInAsync();
        if (isSignedIn)
        {
            contentDialog.Title = "Success";
            contentDialog.Content = "You have successfully signed in";
            _ = await contentDialog.ShowAsync();
            var user = await dropBoxProvider.GetUserDetailsAsync();
            if (string.IsNullOrEmpty(user.DisplayName))
            {
                contentDialog.Title = "Error";
                contentDialog.Content = "An error occurred while fetching user info";
                _ = await contentDialog.ShowAsync();
            }
            else
            {
                contentDialog.Title = "User Info";
                contentDialog.Content = $"Name: {user.DisplayName}\nEmail: {user.Email}\nUsername: {user.Username}\n";
                _ = await contentDialog.ShowAsync();
            }
        }
        else
        {
            contentDialog.Title = "Error";
            contentDialog.Content = "An error occurred while signing in";
            _ = await contentDialog.ShowAsync();
        }
    }

    [RelayCommand]
    private async void LogOut()
    {
        var contentDialog = new ContentDialog();
        contentDialog.XamlRoot = App.MainWindow.Content.XamlRoot;
        var isSignedOut = await dropBoxProvider.SignOutAsync();
        if (isSignedOut)
        {
            contentDialog.Title = "Success";
            contentDialog.Content = "You have successfully signed out";
            contentDialog.PrimaryButtonText = "Ok";
            _ = await contentDialog.ShowAsync();
        }
        else
        {
            contentDialog.Title = "Error";
            contentDialog.Content = "An error occurred while signing out";
            _ = await contentDialog.ShowAsync();
        }
    }
}
