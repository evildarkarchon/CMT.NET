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

        // Initialize with empty game info
        GameInfo = new GameInfo();
    }

    [Reactive] public GameInfo GameInfo { get; set; }
    [Reactive] public bool IsLoading { get; set; }
    [Reactive] public string[] Problems { get; set; } = Array.Empty<string>();

    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

    // UI Helper Properties
    public bool HasProblems => Problems.Length > 0;
    public int FullModuleCount => GameInfo.ModuleCountFull;
    public int LightModuleCount => GameInfo.ModuleCountLight;
    public int TotalModuleCount => GameInfo.ModuleCountFull + GameInfo.ModuleCountLight;
    public int GeneralArchiveCount => GameInfo.Ba2CountGnrl;
    public int TextureArchiveCount => GameInfo.Ba2CountDx10;
    public int TotalArchiveCount => GameInfo.Ba2CountGnrl + GameInfo.Ba2CountDx10;

    // Color properties for UI styling
    public string InstallTypeColor => GameInfo.InstallType switch
    {
        InstallType.OG => "Good",
        InstallType.DG => "Good",
        InstallType.NG => "Good",
        InstallType.Unknown => "Warning",
        InstallType.NotFound => "Bad",
        _ => "Warning"
    };

    public string FullModuleCountColor => GameInfo.ModuleCountFull switch
    {
        < 200 => "Good",
        < 240 => "Warning",
        _ => "Bad"
    };

    public string LightModuleCountColor => GameInfo.ModuleCountLight switch
    {
        < 3000 => "Good",
        < 3800 => "Warning",
        _ => "Bad"
    };

    public string GeneralArchiveCountColor => GameInfo.Ba2CountGnrl switch
    {
        < 200 => "Good",
        < 240 => "Warning",
        _ => "Bad"
    };

    public string TextureArchiveCountColor => GameInfo.Ba2CountDx10 switch
    {
        < 200 => "Good",
        < 240 => "Warning",
        _ => "Bad"
    };

    private async Task RefreshAsync()
    {
        IsLoading = true;
        try
        {
            var detectedGame = await _gameDetectionService.DetectGameAsync();
            if (detectedGame != null)
            {
                GameInfo = detectedGame;
                // TODO: Get problems from scanner service
                Problems = Array.Empty<string>();
            }
        }
        catch (Exception ex)
        {
            Problems = new[] { $"Error detecting game: {ex.Message}" };
        }
        finally
        {
            IsLoading = false;
            // Trigger property change notifications for computed properties
            this.RaisePropertyChanged(nameof(HasProblems));
            this.RaisePropertyChanged(nameof(FullModuleCount));
            this.RaisePropertyChanged(nameof(LightModuleCount));
            this.RaisePropertyChanged(nameof(TotalModuleCount));
            this.RaisePropertyChanged(nameof(GeneralArchiveCount));
            this.RaisePropertyChanged(nameof(TextureArchiveCount));
            this.RaisePropertyChanged(nameof(TotalArchiveCount));
            this.RaisePropertyChanged(nameof(InstallTypeColor));
            this.RaisePropertyChanged(nameof(FullModuleCountColor));
            this.RaisePropertyChanged(nameof(LightModuleCountColor));
            this.RaisePropertyChanged(nameof(GeneralArchiveCountColor));
            this.RaisePropertyChanged(nameof(TextureArchiveCountColor));
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

    // UI Helper Properties
    public bool HasPlugins => Plugins.Length > 0;

    private async Task RefreshAsync()
    {
        IsLoading = true;
        try
        {
            // TODO: Implement F4SE plugin scanning
            await Task.Delay(1000); // Simulate async operation
            Plugins = Array.Empty<F4SeInfo>();
        }
        finally
        {
            IsLoading = false;
            this.RaisePropertyChanged(nameof(HasPlugins));
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
    [Reactive] public Problem? SelectedProblem { get; set; }

    public ReactiveCommand<Unit, Unit> ScanCommand { get; }

    // UI Helper Properties
    public bool HasProblems => Problems.Length > 0;
    public bool HasSelectedProblem => SelectedProblem != null;

    private async Task ScanAsync()
    {
        IsScanning = true;
        try
        {
            var problems = await _cmCheckerService.ScanForProblemsAsync();
            Problems = problems.ToArray();
        }
        catch (Exception ex)
        {
            Problems = new[]
            {
                new Problem
                {
                    Type = ProblemType.Configuration,
                    Severity = ProblemSeverity.Error,
                    Description = $"Error during scan: {ex.Message}"
                }
            };
        }
        finally
        {
            IsScanning = false;
            this.RaisePropertyChanged(nameof(HasProblems));
            this.RaisePropertyChanged(nameof(HasSelectedProblem));
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
