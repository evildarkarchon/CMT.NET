//
// Collective Modding Toolkit
// Copyright (C) 2024, 2025  wxMichael
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Reactive;
using System.Threading.Tasks;
using CMT.NET.Models;
using CMT.NET.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace CMT.NET.ViewModels;

public class OverviewViewModel : ViewModelBase
{
    private readonly IGameDetectionService _gameDetectionService;

    public OverviewViewModel(IGameDetectionService gameDetectionService)
    {
        _gameDetectionService = gameDetectionService;
        RefreshCommand = ReactiveCommand.CreateFromTask(RefreshAsync);
    }

    [Reactive] public GameInfo? GameInfo { get; set; }
    [Reactive] public bool IsLoading { get; set; }

    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

    private async Task RefreshAsync()
    {
        IsLoading = true;
        try
        {
            GameInfo = await _gameDetectionService.DetectGameAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }
}

public class F4SeViewModel : ViewModelBase
{
    private readonly ICmCheckerService _cmCheckerService;

    public F4SeViewModel(ICmCheckerService cmCheckerService)
    {
        _cmCheckerService = cmCheckerService;
        RefreshCommand = ReactiveCommand.CreateFromTask(RefreshAsync);
    }

    [Reactive] public F4SeInfo[] Plugins { get; set; } = Array.Empty<F4SeInfo>();
    [Reactive] public bool IsLoading { get; set; }

    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

    private async Task RefreshAsync()
    {
        IsLoading = true;
        try
        {
            // TODO: Implement F4SE plugin scanning
            Plugins = Array.Empty<F4SeInfo>();
        }
        finally
        {
            IsLoading = false;
        }
    }
}

public class ScannerViewModel : ViewModelBase
{
    private readonly ICmCheckerService _cmCheckerService;

    public ScannerViewModel(ICmCheckerService cmCheckerService)
    {
        _cmCheckerService = cmCheckerService;
        ScanCommand = ReactiveCommand.CreateFromTask(ScanAsync);
    }

    [Reactive] public Problem[] Problems { get; set; } = Array.Empty<Problem>();
    [Reactive] public bool IsScanning { get; set; }

    public ReactiveCommand<Unit, Unit> ScanCommand { get; }

    private async Task ScanAsync()
    {
        IsScanning = true;
        try
        {
            var problems = await _cmCheckerService.ScanForProblemsAsync();
            Problems = problems.ToArray();
        }
        finally
        {
            IsScanning = false;
        }
    }
}

public class ToolsViewModel : ViewModelBase
{
    // TODO: Implement tools functionality
}

public class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;

    public SettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
        ResetCommand = ReactiveCommand.CreateFromTask(ResetAsync);
    }

    [Reactive] public AppSettings Settings { get; set; } = new();

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> ResetCommand { get; }

    private async Task SaveAsync()
    {
        await _settingsService.SaveSettingsAsync();
    }

    private async Task ResetAsync()
    {
        await _settingsService.ResetToDefaultsAsync();
        Settings = _settingsService.Settings;
    }
}

public class AboutViewModel : ViewModelBase
{
    public string Version => "1.0.0";
    public string Copyright => "Copyright (C) 2024, 2025 wxMichael";
    public string Description => "Collective Modding Toolkit for Fallout 4";
}
