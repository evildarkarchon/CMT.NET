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
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using CMT.NET.ViewModels;
using CMT.NET.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CMT.NET.Services;

public interface IToolLauncherService
{
    Task LaunchDowngraderAsync(Window? parent = null);
    Task LaunchArchivePatcherAsync(Window? parent = null);
    Task LaunchExternalToolAsync(string toolName);
    Task OpenLogsFolderAsync();
    Task ClearCacheAsync();
}

public class ToolLauncherService : IToolLauncherService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ToolLauncherService> _logger;

    public ToolLauncherService(IServiceProvider serviceProvider, ILogger<ToolLauncherService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task LaunchDowngraderAsync(Window? parent = null)
    {
        try
        {
            var viewModel = _serviceProvider.GetRequiredService<DowngraderViewModel>();
            var window = new DowngraderWindow(viewModel);

            if (parent != null)
            {
                await window.ShowDialog(parent);
            }
            else
            {
                window.Show();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error launching downgrader");
            throw;
        }
    }

    public async Task LaunchArchivePatcherAsync(Window? parent = null)
    {
        try
        {
            var viewModel = _serviceProvider.GetRequiredService<ArchivePatcherViewModel>();
            var window = new ArchivePatcherWindow(viewModel);

            if (parent != null)
            {
                await window.ShowDialog(parent);
            }
            else
            {
                window.Show();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error launching archive patcher");
            throw;
        }
    }

    public async Task LaunchExternalToolAsync(string toolName)
    {
        try
        {
            var executablePath = await FindExternalToolAsync(toolName);
            if (string.IsNullOrEmpty(executablePath))
            {
                throw new FileNotFoundException($"External tool '{toolName}' not found");
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                UseShellExecute = true
            };

            Process.Start(startInfo);
            _logger.LogInformation("Launched external tool: {ToolName} at {Path}", toolName, executablePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error launching external tool: {ToolName}", toolName);
            throw;
        }
    }

    public async Task OpenLogsFolderAsync()
    {
        try
        {
            var logsPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");

            if (!Directory.Exists(logsPath))
            {
                Directory.CreateDirectory(logsPath);
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = logsPath,
                UseShellExecute = true,
                Verb = "open"
            };

            Process.Start(startInfo);
            _logger.LogInformation("Opened logs folder: {LogsPath}", logsPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening logs folder");
            throw;
        }
    }

    public async Task ClearCacheAsync()
    {
        try
        {
            var cacheDirectories = new[]
            {
                Path.Combine(Path.GetTempPath(), "CMT.NET"),
                Path.Combine(Directory.GetCurrentDirectory(), "cache"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CMT.NET",
                    "cache")
            };

            foreach (var cacheDir in cacheDirectories)
            {
                if (Directory.Exists(cacheDir))
                {
                    Directory.Delete(cacheDir, true);
                    _logger.LogInformation("Cleared cache directory: {CacheDir}", cacheDir);
                }
            }

            _logger.LogInformation("Cache cleared successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
            throw;
        }
    }

    private async Task<string?> FindExternalToolAsync(string toolName)
    {
        await Task.CompletedTask;

        // Common paths for modding tools
        var commonPaths = new[]
        {
            @"C:\Program Files\xEdit",
            @"C:\Program Files (x86)\xEdit",
            @"C:\Tools\xEdit",
            @"C:\Modding\xEdit"
        };

        var executableNames = toolName.ToLower() switch
        {
            "xedit" => new[] { "FO4Edit.exe", "xEdit.exe" },
            "bsarch" => new[] { "BSArch.exe" },
            "complex sorter" => new[] { "Complex Sorter.exe" },
            _ => new[] { $"{toolName}.exe" }
        };

        foreach (var basePath in commonPaths)
        {
            if (!Directory.Exists(basePath))
                continue;

            foreach (var execName in executableNames)
            {
                var fullPath = Path.Combine(basePath, execName);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }
        }

        return null;
    }
}
