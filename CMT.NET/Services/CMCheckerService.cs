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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CMT.NET.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace CMT.NET.Services;

public class CMCheckerService : ICmCheckerService
{
    private readonly ILogger<CMCheckerService> _logger;
    private readonly IGameDetectionService _gameDetectionService;
    private readonly IModuleAnalysisService _moduleAnalysisService;
    private readonly IArchiveAnalysisService _archiveAnalysisService;
    private readonly IFileOperationService _fileOperationService;
    private readonly IIniFileService _iniFileService;
    private readonly ISettingsService _settingsService;

    private GameAnalysisResult? _lastAnalysisResult;
    private bool _isAnalyzing;

    public CMCheckerService(
        ILogger<CMCheckerService> logger,
        IGameDetectionService gameDetectionService,
        IModuleAnalysisService moduleAnalysisService,
        IArchiveAnalysisService archiveAnalysisService,
        IFileOperationService fileOperationService,
        IIniFileService iniFileService,
        ISettingsService settingsService)
    {
        _logger = logger;
        _gameDetectionService = gameDetectionService;
        _moduleAnalysisService = moduleAnalysisService;
        _archiveAnalysisService = archiveAnalysisService;
        _fileOperationService = fileOperationService;
        _iniFileService = iniFileService;
        _settingsService = settingsService;
    }

    public GameAnalysisResult? LastAnalysisResult => _lastAnalysisResult;
    public bool IsAnalyzing => _isAnalyzing;

    public async Task<GameAnalysisResult> AnalyzeGameInstallationAsync(string? gamePath = null)
    {
        if (_isAnalyzing)
        {
            throw new InvalidOperationException("Analysis is already in progress");
        }

        _isAnalyzing = true;

        try
        {
            _logger.LogInformation("Starting game installation analysis");

            var result = new GameAnalysisResult
            {
                AnalysisTimestamp = DateTime.Now,
                Problems = new List<Problem>()
            };

            // Step 1: Detect or validate game installation
            GameInfo? gameInfo;
            if (!string.IsNullOrEmpty(gamePath))
            {
                if (await _gameDetectionService.ValidateGamePathAsync(gamePath))
                {
                    gameInfo = new GameInfo { GamePath = gamePath };
                }
                else
                {
                    result.Problems.Add(new Problem
                    {
                        Type = ProblemType.GamePath,
                        Severity = ProblemSeverity.Critical,
                        Description = $"Invalid game path: {gamePath}",
                        Solution = "Please select a valid Fallout 4 installation directory"
                    });
                    return result;
                }
            }
            else
            {
                gameInfo = await _gameDetectionService.DetectGameAsync();
                if (gameInfo == null)
                {
                    result.Problems.Add(new Problem
                    {
                        Type = ProblemType.GamePath,
                        Severity = ProblemSeverity.Critical,
                        Description = "Could not detect Fallout 4 installation",
                        Solution = "Please manually select your Fallout 4 installation directory"
                    });
                    return result;
                }
            }

            result.GameInfo = gameInfo;

            // Step 2: Analyze game version and executable
            await AnalyzeGameVersionAsync(result);

            // Step 3: Analyze Data directory
            var dataPath = Path.Combine(gameInfo.GamePath, "Data");
            if (Directory.Exists(dataPath))
            {
                await AnalyzeDataDirectoryAsync(result, dataPath);
            }
            else
            {
                result.Problems.Add(new Problem
                {
                    Type = ProblemType.GamePath,
                    Severity = ProblemSeverity.Critical,
                    Description = "Data directory not found",
                    Solution = "Verify game installation integrity"
                });
            }

            // Step 4: Analyze game configuration
            await AnalyzeGameConfigurationAsync(result);

            // Step 5: Detect mod managers
            await DetectModManagersAsync(result);

            // Step 6: Check for common issues
            await CheckCommonIssuesAsync(result);

            _lastAnalysisResult = result;

            _logger.LogInformation("Game installation analysis completed. Found {ProblemCount} problems",
                result.Problems.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during game installation analysis");
            throw;
        }
        finally
        {
            _isAnalyzing = false;
        }
    }

    public async Task<List<Problem>> ScanForProblemsAsync()
    {
        if (_lastAnalysisResult == null)
        {
            throw new InvalidOperationException(
                "No analysis result available. Run AnalyzeGameInstallationAsync first.");
        }

        var problems = new List<Problem>();

        try
        {
            _logger.LogInformation("Starting problem scan");

            // Check module limits
            await CheckModuleLimitsAsync(_lastAnalysisResult, problems);

            // Check for missing masters
            await CheckMissingMastersAsync(_lastAnalysisResult, problems);

            // Check archive versions
            await CheckArchiveVersionsAsync(_lastAnalysisResult, problems);

            // Check for plugin conflicts
            await CheckPluginConflictsAsync(_lastAnalysisResult, problems);

            // Check INI settings
            await CheckIniSettingsAsync(_lastAnalysisResult, problems);

            _logger.LogInformation("Problem scan completed. Found {ProblemCount} additional problems",
                problems.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during problem scan");
            throw;
        }

        return problems;
    }

    public async Task<SystemInfo> GetSystemInfoAsync()
    {
        var systemInfo = new SystemInfo();

        try
        {
            // Get OS information
            systemInfo.OperatingSystem = Environment.OSVersion.ToString();
            systemInfo.Is64BitOperatingSystem = Environment.Is64BitOperatingSystem;
            systemInfo.ProcessorCount = Environment.ProcessorCount;

            // Get memory information
            var memoryInfo = GC.GetTotalMemory(false);
            systemInfo.AvailableMemory = memoryInfo;

            // Get CPU information from registry
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
                if (key != null)
                {
                    systemInfo.ProcessorName = key.GetValue("ProcessorNameString") as string ?? "Unknown";
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not retrieve CPU information from registry");
                systemInfo.ProcessorName = "Unknown";
            }

            // Get .NET information
            systemInfo.DotNetVersion = Environment.Version.ToString();
            systemInfo.RuntimeVersion = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;

            _logger.LogDebug("Retrieved system information");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving system information");
        }

        return systemInfo;
    }

    private async Task AnalyzeGameVersionAsync(GameAnalysisResult result)
    {
        try
        {
            var fallout4ExePath = Path.Combine(result.GameInfo!.GamePath, "Fallout4.exe");
            var version = await _fileOperationService.GetFileVersionAsync(fallout4ExePath);

            if (version != null)
            {
                result.GameInfo.GameVersion = version;

                // Check if it's a supported version
                var supportedVersions = new[] { "1.10.163.0", "1.10.162.0", "1.10.138.0", "1.10.130.0" };
                if (!supportedVersions.Contains(version))
                {
                    result.Problems.Add(new Problem
                    {
                        Type = ProblemType.GameVersion,
                        Severity = ProblemSeverity.Warning,
                        Description = $"Unsupported game version: {version}",
                        Solution = "Consider downgrading to a supported version"
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing game version");
            result.Problems.Add(new Problem
            {
                Type = ProblemType.GameVersion,
                Severity = ProblemSeverity.Warning,
                Description = "Could not determine game version",
                Solution = "Verify game installation integrity"
            });
        }
    }

    private async Task AnalyzeDataDirectoryAsync(GameAnalysisResult result, string dataPath)
    {
        try
        {
            // Analyze modules
            result.Modules = await _moduleAnalysisService.AnalyzeModulesInDirectoryAsync(dataPath);

            // Analyze archives
            result.Archives = await _archiveAnalysisService.AnalyzeArchivesInDirectoryAsync(dataPath);

            _logger.LogInformation("Found {ModuleCount} modules and {ArchiveCount} archives",
                result.Modules.Count, result.Archives.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing data directory");
            result.Problems.Add(new Problem
            {
                Type = ProblemType.DataDirectory,
                Severity = ProblemSeverity.Error,
                Description = "Error analyzing data directory",
                Solution = "Check file permissions and disk space"
            });
        }
    }

    private async Task AnalyzeGameConfigurationAsync(GameAnalysisResult result)
    {
        try
        {
            result.GameConfiguration = await _iniFileService.GetFallout4PreferencesAsync(result.GameInfo!.GamePath);

            _logger.LogInformation("Loaded {ConfigCount} configuration settings",
                result.GameConfiguration.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing game configuration");
            result.Problems.Add(new Problem
            {
                Type = ProblemType.Configuration,
                Severity = ProblemSeverity.Warning,
                Description = "Could not load game configuration",
                Solution = "Check if configuration files exist and are readable"
            });
        }
    }

    private async Task DetectModManagersAsync(GameAnalysisResult result)
    {
        // This would be implemented to detect MO2, Vortex, etc.
        // For now, just a placeholder
        await Task.CompletedTask;
    }

    private async Task CheckCommonIssuesAsync(GameAnalysisResult result)
    {
        // This would check for common issues like missing DLLs, etc.
        // For now, just a placeholder
        await Task.CompletedTask;
    }

    private async Task CheckModuleLimitsAsync(GameAnalysisResult result, List<Problem> problems)
    {
        if (result.Modules == null) return;

        var regularModules = result.Modules.Where(m => m.ModuleType == ModuleType.Plugin).Count();
        var lightModules = result.Modules.Where(m => m.ModuleType == ModuleType.Light).Count();

        if (regularModules > 255)
        {
            problems.Add(new Problem
            {
                Type = ProblemType.ModuleLimit,
                Severity = ProblemSeverity.Critical,
                Description = $"Too many regular modules: {regularModules}/255",
                Solution = "Convert some plugins to light modules or remove unnecessary plugins"
            });
        }

        if (lightModules > 4096)
        {
            problems.Add(new Problem
            {
                Type = ProblemType.ModuleLimit,
                Severity = ProblemSeverity.Critical,
                Description = $"Too many light modules: {lightModules}/4096",
                Solution = "Remove unnecessary light modules"
            });
        }
    }

    private async Task CheckMissingMastersAsync(GameAnalysisResult result, List<Problem> problems)
    {
        if (result.Modules == null) return;

        var availableModules = result.Modules.Select(m => m.FileName).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var module in result.Modules)
        {
            var masterFiles = await _moduleAnalysisService.GetMasterFilesAsync(module);

            foreach (var masterFile in masterFiles)
            {
                if (!availableModules.Contains(masterFile))
                {
                    problems.Add(new Problem
                    {
                        Type = ProblemType.MissingMaster,
                        Severity = ProblemSeverity.Error,
                        Description = $"Missing master file '{masterFile}' required by '{module.FileName}'",
                        Solution = "Install the missing master file or remove the dependent plugin"
                    });
                }
            }
        }
    }

    private async Task CheckArchiveVersionsAsync(GameAnalysisResult result, List<Problem> problems)
    {
        if (result.Archives == null) return;

        foreach (var archive in result.Archives)
        {
            var version = _archiveAnalysisService.GetArchiveVersion(archive);

            if (version == ArchiveVersion.Unknown)
            {
                problems.Add(new Problem
                {
                    Type = ProblemType.ArchiveVersion,
                    Severity = ProblemSeverity.Warning,
                    Description = $"Unknown archive version: {archive.FileName}",
                    Solution = "Check if archive is corrupted or unsupported"
                });
            }
        }
    }

    private async Task CheckPluginConflictsAsync(GameAnalysisResult result, List<Problem> problems)
    {
        // This would check for plugin conflicts
        // For now, just a placeholder
        await Task.CompletedTask;
    }

    private async Task CheckIniSettingsAsync(GameAnalysisResult result, List<Problem> problems)
    {
        if (result.GameConfiguration == null) return;

        // Check for important settings
        var archiveInvalidation = result.GameConfiguration.GetValueOrDefault("Archive.bInvalidateOlderFiles", "0");
        if (archiveInvalidation != "1")
        {
            problems.Add(new Problem
            {
                Type = ProblemType.Configuration,
                Severity = ProblemSeverity.Warning,
                Description = "Archive invalidation is not enabled",
                Solution = "Enable archive invalidation in Fallout4.ini"
            });
        }
    }
}
