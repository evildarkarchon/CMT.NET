using CMT.NET.Models;
using CMT.NET.Services;
using ReactiveUI.Fody.Helpers;

namespace CMT.NET.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;

    public MainWindowViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        _navigationService.TabChanged += OnTabChanged;
        CurrentTab = Tab.Overview;
    }

    [Reactive] public Tab CurrentTab { get; set; }
    [Reactive] public ViewModelBase? CurrentViewModel { get; set; }

    private void OnTabChanged(object? sender, Tab tab)
    {
        CurrentTab = tab;
        CurrentViewModel = _navigationService.GetViewModel(tab);
    }

    public void NavigateToTab(Tab tab)
    {
        _navigationService.NavigateTo(tab);
    }
}