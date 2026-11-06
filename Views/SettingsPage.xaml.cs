using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using PdfReader.ViewModels;

namespace PdfReader.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; } = App.Services.GetRequiredService<SettingsViewModel>();

    public SettingsPage()
    {
        InitializeComponent();
    }
}
