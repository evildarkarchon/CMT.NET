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
using System.Collections.Generic;
using System.Linq;
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
    private readonly ICmCheckerService _cmCheckerService;

    public OverviewViewModel(IGameDetectionService gameDetectionService, ICmCheckerService cmCheckerService)
    {
        _gameDetectionService = gameDetectionService;
        _cmCheckerService = cmCheckerService;
        RefreshCommand = ReactiveCommand.CreateFromTask(RefreshAsync);

        // Initialize with empty game info
        GameInfo = new GameInfo();
    }

    [Reactive] public GameInfo GameInfo { get; set; }
    [Reactive] public bool IsLoading { get; set; }
    [Reactive] public string[] Problems { get; set; } = Array.Empty<string>();
    [Reactive] public string ModManager { get; set; } = "Unknown";
    [Reactive] public string SystemInfo { get; set; } = "";

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

                // Get system info
                var systemInfo = await _cmCheckerService.GetSystemInfoAsync();
                SystemInfo = $"OS: {systemInfo.OperatingSystem}\n" +
                             $"Processor: {systemInfo.ProcessorName} ({systemInfo.ProcessorCount} cores)\n" +
                             $"Memory: {systemInfo.AvailableMemory / (1024 * 1024 * 1024)} GB\n" +
                             $".NET: {systemInfo.DotNetVersion}";

                // Detect mod manager
                ModManager = DetectModManager();

                // Get problems from analysis
                var analysisResult = await _cmCheckerService.AnalyzeGameInstallationAsync();
                Problems = analysisResult.Problems.Select(p => p.Title).ToArray();
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

    private string DetectModManager()
    {
        // TODO: Implement mod manager detection logic
        // Check for MO2, Vortex, etc.
        return "Unknown";
    }
}

public class F4SeViewModel : ViewModelBase
{
    private readonly ICmCheckerService _cmCheckerService;
    private readonly IGameDetectionService _gameDetectionService;

    // Whitelisted DLLs that are known to work with both OG and NG
    private readonly string[] _ogNgWhitelist =
    {
        "AchievementsModsEnablerLoader.dll",
        "BetterConsole.dll",
        "Buffout4.dll",
        "ClockWidget.dll",
        "FloatingDamage.dll",
        "GCBugFix.dll",
        "HUDPlusPlus.dll",
        "IndirectFire.dll",
        "MinimalMinimap.dll",
        "MoonRotationFix.dll",
        "mute_on_focus_loss.dll",
        "SprintStutteringFix.dll",
        "UnlimitedFastTravel.dll",
        "WeaponDebrisCrashFix.dll",
        "x-cell-fo4.dll"
    };

    public F4SeViewModel(ICmCheckerService cmCheckerService, IGameDetectionService gameDetectionService)
    {
        _cmCheckerService = cmCheckerService;
        _gameDetectionService = gameDetectionService;
        RefreshCommand = ReactiveCommand.CreateFromTask(RefreshAsync);

        // Initialize with empty data
        Plugins = Array.Empty<F4SeInfo>();
    }

    [Reactive] public F4SeInfo[] Plugins { get; set; }
    [Reactive] public bool IsLoading { get; set; }
    [Reactive] public F4SeInfo? SelectedPlugin { get; set; }

    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

    // UI Helper Properties
    public bool HasPlugins => Plugins.Length > 0;
    public bool HasSelectedPlugin => SelectedPlugin != null;
    public int CompatiblePluginCount => Plugins.Count(p => p.IsCompatible);
    public int IncompatiblePluginCount => Plugins.Count(p => !p.IsCompatible);
    public int WhitelistedPluginCount => Plugins.Count(p => p.IsWhitelisted);

    private async Task RefreshAsync()
    {
        IsLoading = true;
        try
        {
            var gameInfo = await _gameDetectionService.DetectGameAsync();
            if (gameInfo?.F4SePath == null)
            {
                Plugins = Array.Empty<F4SeInfo>();
                return;
            }

            var analysisResult = await _cmCheckerService.AnalyzeGameInstallationAsync();
            Plugins = analysisResult.F4SePlugins.ToArray();

            // Apply whitelist and compatibility logic
            foreach (var plugin in Plugins)
            {
                plugin.IsWhitelisted = _ogNgWhitelist.Contains(plugin.FileName);
                plugin.IsCompatible = DetermineCompatibility(plugin, gameInfo);
                plugin.Status = plugin.IsCompatible ? "Compatible" : "Incompatible";
            }
        }
        catch (Exception ex)
        {
            // Create error entry
            Plugins = new[]
            {
                new F4SeInfo
                {
                    FileName = "Error",
                    Name = "Scan Error",
                    Version = "N/A",
                    Status = ex.Message,
                    IsCompatible = false,
                    IsWhitelisted = false
                }
            };
        }
        finally
        {
            IsLoading = false;
            this.RaisePropertyChanged(nameof(HasPlugins));
            this.RaisePropertyChanged(nameof(HasSelectedPlugin));
            this.RaisePropertyChanged(nameof(CompatiblePluginCount));
            this.RaisePropertyChanged(nameof(IncompatiblePluginCount));
            this.RaisePropertyChanged(nameof(WhitelistedPluginCount));
        }
    }

    private bool DetermineCompatibility(F4SeInfo plugin, GameInfo gameInfo)
    {
        // If whitelisted, it's compatible
        if (plugin.IsWhitelisted)
            return true;

        // Basic compatibility logic - this would need to be expanded
        // based on actual DLL analysis and game version
        return gameInfo.InstallType switch
        {
            InstallType.OG => true, // Most DLLs work with OG
            InstallType.DG => true, // DG is compatible with OG DLLs
            InstallType.NG => plugin.IsWhitelisted, // NG only works with whitelisted DLLs
            _ => false
        };
    }
}

public class ScannerViewModel : ViewModelBase
{
    private readonly ICmCheckerService _cmCheckerService;
    private readonly IGameDetectionService _gameDetectionService;

    public ScannerViewModel(ICmCheckerService cmCheckerService, IGameDetectionService gameDetectionService)
    {
        _cmCheckerService = cmCheckerService;
        _gameDetectionService = gameDetectionService;
        ScanCommand = ReactiveCommand.CreateFromTask(ScanAsync);

        // Initialize with empty data
        Problems = Array.Empty<Problem>();
        GroupedProblems = Array.Empty<ProblemGroup>();
    }

    [Reactive] public Problem[] Problems { get; set; }
    [Reactive] public ProblemGroup[] GroupedProblems { get; set; }
    [Reactive] public bool IsScanning { get; set; }
    [Reactive] public Problem? SelectedProblem { get; set; }
    [Reactive] public string ScanProgress { get; set; } = "";
    [Reactive] public int ScanPercentage { get; set; }

    public ReactiveCommand<Unit, Unit> ScanCommand { get; }

    // UI Helper Properties
    public bool HasProblems => Problems.Length > 0;
    public bool HasSelectedProblem => SelectedProblem != null;
    public int ErrorCount => Problems.Count(p => p.Severity == ProblemSeverity.Error);
    public int WarningCount => Problems.Count(p => p.Severity == ProblemSeverity.Warning);
    public int InfoCount => Problems.Count(p => p.Severity == ProblemSeverity.Info);

    private async Task ScanAsync()
    {
        IsScanning = true;
        ScanPercentage = 0;
        ScanProgress = "Initializing scan...";

        try
        {
            var gameInfo = await _gameDetectionService.DetectGameAsync();
            if (gameInfo == null)
            {
                Problems = new[]
                {
                    new Problem
                    {
                        Type = ProblemType.Configuration,
                        Severity = ProblemSeverity.Error,
                        Title = "Game Not Found",
                        Description = "Unable to detect Fallout 4 installation",
                        Solution = "Check that Fallout 4 is properly installed"
                    }
                };
                return;
            }

            ScanProgress = "Analyzing game installation...";
            ScanPercentage = 25;

            var analysisResult = await _cmCheckerService.AnalyzeGameInstallationAsync();

            ScanProgress = "Scanning for problems...";
            ScanPercentage = 50;

            var problems = await _cmCheckerService.ScanForProblemsAsync();

            ScanProgress = "Processing results...";
            ScanPercentage = 75;

            // Add analysis-specific problems
            var allProblems = problems.ToList();
            allProblems.AddRange(AnalyzeGameData(analysisResult));

            Problems = allProblems.ToArray();
            GroupedProblems = GroupProblemsByType(Problems);

            ScanProgress = "Scan complete";
            ScanPercentage = 100;
        }
        catch (Exception ex)
        {
            Problems = new[]
            {
                new Problem
                {
                    Type = ProblemType.Configuration,
                    Severity = ProblemSeverity.Error,
                    Title = "Scan Error",
                    Description = $"Error during scan: {ex.Message}",
                    Solution = "Check the logs for more details"
                }
            };
            GroupedProblems = Array.Empty<ProblemGroup>();
        }
        finally
        {
            IsScanning = false;
            this.RaisePropertyChanged(nameof(HasProblems));
            this.RaisePropertyChanged(nameof(HasSelectedProblem));
            this.RaisePropertyChanged(nameof(ErrorCount));
            this.RaisePropertyChanged(nameof(WarningCount));
            this.RaisePropertyChanged(nameof(InfoCount));
        }
    }

    private List<Problem> AnalyzeGameData(GameAnalysisResult analysisResult)
    {
        var problems = new List<Problem>();

        if (analysisResult.GameInfo == null)
            return problems;

        var gameInfo = analysisResult.GameInfo;

        // Check module limits
        if (gameInfo.ModuleCountFull >= 255)
        {
            problems.Add(new Problem
            {
                Type = ProblemType.ModuleLimit,
                Severity = ProblemSeverity.Error,
                Title = "Full Module Limit Exceeded",
                Description = $"You have {gameInfo.ModuleCountFull} full modules (limit: 255)",
                Solution = "Disable some ESP files or convert them to light modules"
            });
        }
        else if (gameInfo.ModuleCountFull >= 240)
        {
            problems.Add(new Problem
            {
                Type = ProblemType.ModuleLimit,
                Severity = ProblemSeverity.Warning,
                Title = "Approaching Full Module Limit",
                Description = $"You have {gameInfo.ModuleCountFull} full modules (limit: 255)",
                Solution = "Consider disabling some ESP files or converting them to light modules"
            });
        }

        // Check light module limits
        if (gameInfo.ModuleCountLight >= 4096)
        {
            problems.Add(new Problem
            {
                Type = ProblemType.ModuleLimit,
                Severity = ProblemSeverity.Error,
                Title = "Light Module Limit Exceeded",
                Description = $"You have {gameInfo.ModuleCountLight} light modules (limit: 4096)",
                Solution = "Disable some ESL files"
            });
        }
        else if (gameInfo.ModuleCountLight >= 3800)
        {
            problems.Add(new Problem
            {
                Type = ProblemType.ModuleLimit,
                Severity = ProblemSeverity.Warning,
                Title = "Approaching Light Module Limit",
                Description = $"You have {gameInfo.ModuleCountLight} light modules (limit: 4096)",
                Solution = "Consider disabling some ESL files"
            });
        }

        // Check archive limits
        if (gameInfo.Ba2CountGnrl >= 255)
        {
            problems.Add(new Problem
            {
                Type = ProblemType.ArchiveLimit,
                Severity = ProblemSeverity.Error,
                Title = "General Archive Limit Exceeded",
                Description = $"You have {gameInfo.Ba2CountGnrl} general archives (limit: 255)",
                Solution = "Disable some BA2 archives"
            });
        }

        if (gameInfo.Ba2CountDx10 >= 255)
        {
            problems.Add(new Problem
            {
                Type = ProblemType.ArchiveLimit,
                Severity = ProblemSeverity.Error,
                Title = "Texture Archive Limit Exceeded",
                Description = $"You have {gameInfo.Ba2CountDx10} texture archives (limit: 255)",
                Solution = "Disable some texture BA2 archives"
            });
        }

        return problems;
    }

    private ProblemGroup[] GroupProblemsByType(Problem[] problems)
    {
        return problems
            .GroupBy(p => p.Type)
            .Select(g => new ProblemGroup
            {
                Type = g.Key,
                Problems = g.ToArray(),
                Count = g.Count(),
                ErrorCount = g.Count(p => p.Severity == ProblemSeverity.Error),
                WarningCount = g.Count(p => p.Severity == ProblemSeverity.Warning),
                InfoCount = g.Count(p => p.Severity == ProblemSeverity.Info)
            })
            .OrderBy(g => g.Type)
            .ToArray();
    }
}

public class ToolsViewModel : ViewModelBase
{
    private readonly IToolLauncherService _toolLauncherService;

    public ToolsViewModel(IToolLauncherService toolLauncherService)
    {
        _toolLauncherService = toolLauncherService;

        LaunchDowngraderCommand = ReactiveCommand.CreateFromTask(LaunchDowngraderAsync);
        LaunchArchivePatcherCommand = ReactiveCommand.CreateFromTask(LaunchArchivePatcherAsync);
        LaunchExternalToolCommand = ReactiveCommand.CreateFromTask<string>(LaunchExternalToolAsync);
        OpenLogsFolderCommand = ReactiveCommand.CreateFromTask(OpenLogsFolderAsync);
        ClearCacheCommand = ReactiveCommand.CreateFromTask(ClearCacheAsync);
    }

    public ReactiveCommand<Unit, Unit> LaunchDowngraderCommand { get; }
    public ReactiveCommand<Unit, Unit> LaunchArchivePatcherCommand { get; }
    public ReactiveCommand<string, Unit> LaunchExternalToolCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenLogsFolderCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearCacheCommand { get; }

    private async Task LaunchDowngraderAsync()
    {
        try
        {
            await _toolLauncherService.LaunchDowngraderAsync();
        }
        catch (Exception ex)
        {
            // TODO: Show error message to user
            System.Diagnostics.Debug.WriteLine($"Error launching downgrader: {ex.Message}");
        }
    }

    private async Task LaunchArchivePatcherAsync()
    {
        try
        {
            await _toolLauncherService.LaunchArchivePatcherAsync();
        }
        catch (Exception ex)
        {
            // TODO: Show error message to user
            System.Diagnostics.Debug.WriteLine($"Error launching archive patcher: {ex.Message}");
        }
    }

    private async Task LaunchExternalToolAsync(string toolName)
    {
        try
        {
            await _toolLauncherService.LaunchExternalToolAsync(toolName);
        }
        catch (Exception ex)
        {
            // TODO: Show error message to user
            System.Diagnostics.Debug.WriteLine($"Error launching external tool {toolName}: {ex.Message}");
        }
    }

    private async Task OpenLogsFolderAsync()
    {
        try
        {
            await _toolLauncherService.OpenLogsFolderAsync();
        }
        catch (Exception ex)
        {
            // TODO: Show error message to user
            System.Diagnostics.Debug.WriteLine($"Error opening logs folder: {ex.Message}");
        }
    }

    private async Task ClearCacheAsync()
    {
        try
        {
            await _toolLauncherService.ClearCacheAsync();
        }
        catch (Exception ex)
        {
            // TODO: Show error message to user
            System.Diagnostics.Debug.WriteLine($"Error clearing cache: {ex.Message}");
        }
    }
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
