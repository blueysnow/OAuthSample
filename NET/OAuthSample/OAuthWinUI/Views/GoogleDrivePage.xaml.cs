using Microsoft.UI.Xaml.Controls;

using OAuthWinUI.ViewModels;

namespace OAuthWinUI.Views;

public sealed partial class GoogleDrivePage : Page
{
    public GoogleDriveViewModel ViewModel
    {
        get;
    }

    public GoogleDrivePage()
    {
        ViewModel = App.GetService<GoogleDriveViewModel>();
        InitializeComponent();
    }
}
