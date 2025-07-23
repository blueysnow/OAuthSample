using Microsoft.UI.Xaml.Controls;

using OAuthWinUI.ViewModels;

namespace OAuthWinUI.Views;

public sealed partial class DropBoxPage : Page
{
    public DropBoxViewModel ViewModel
    {
        get;
    }

    public DropBoxPage()
    {
        ViewModel = App.GetService<DropBoxViewModel>();
        InitializeComponent();
    }
}
