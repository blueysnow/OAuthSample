using Microsoft.UI.Xaml.Controls;

using OAuthWinUI.ViewModels;

namespace OAuthWinUI.Views;

public sealed partial class OneDrivePage : Page
{
    public OneDriveViewModel ViewModel
    {
        get;
    }

    public OneDrivePage()
    {
        ViewModel = App.GetService<OneDriveViewModel>();
        InitializeComponent();
    }
}
