using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using PaperFlow.ViewModels;

namespace PaperFlow.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; } = App.Services.GetRequiredService<SettingsViewModel>();

    public SettingsPage()
    {
        InitializeComponent();
    }
}
