using CMT.NET.Models;
using CMT.NET.Services;
using ReactiveUI.Fody.Helpers;

namespace CMT.NET.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;

    public MainWindowViewModel(
        INavigationService navigationService,
        OverviewViewModel overviewViewModel,
        F4SeViewModel f4SeViewModel,
        ScannerViewModel scannerViewModel,
        ToolsViewModel toolsViewModel,
        SettingsViewModel settingsViewModel,
        AboutViewModel aboutViewModel)
    {
        _navigationService = navigationService;

        // Initialize all tab ViewModels
        OverviewViewModel = overviewViewModel;
        F4SeViewModel = f4SeViewModel;
        ScannerViewModel = scannerViewModel;
        ToolsViewModel = toolsViewModel;
        SettingsViewModel = settingsViewModel;
        AboutViewModel = aboutViewModel;

        // Set up navigation
        _navigationService.TabChanged += OnTabChanged;
        CurrentTab = Tab.Overview;
        CurrentTabIndex = 0;
    }

    [Reactive] public Tab CurrentTab { get; set; }
    [Reactive] public int CurrentTabIndex { get; set; }
    [Reactive] public ViewModelBase? CurrentViewModel { get; set; }

    // Tab ViewModels
    public OverviewViewModel OverviewViewModel { get; }
    public F4SeViewModel F4SeViewModel { get; }
    public ScannerViewModel ScannerViewModel { get; }
    public ToolsViewModel ToolsViewModel { get; }
    public SettingsViewModel SettingsViewModel { get; }
    public AboutViewModel AboutViewModel { get; }

    private void OnTabChanged(object? sender, Tab tab)
    {
        CurrentTab = tab;
        CurrentTabIndex = TabToIndex(tab);
        CurrentViewModel = _navigationService.GetViewModel(tab);
    }

    private static int TabToIndex(Tab tab) => tab switch
    {
        Tab.Overview => 0,
        Tab.F4SE => 1,
        Tab.Scanner => 2,
        Tab.Tools => 3,
        Tab.Settings => 4,
        Tab.About => 5,
        _ => 0
    };

    public void NavigateToTab(Tab tab)
    {
        _navigationService.NavigateTo(tab);
    }
}