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
using System.IO;
using System.Threading.Tasks;
using CMT.NET.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace CMT.NET.Services;

public class GameDetectionService : IGameDetectionService
{
    private readonly ILogger<GameDetectionService> _logger;
    private GameInfo? _currentGame;

    public GameDetectionService(ILogger<GameDetectionService> logger)
    {
        _logger = logger;
    }

    public GameInfo? CurrentGame => _currentGame;

    public async Task<GameInfo?> DetectGameAsync()
    {
        try
        {
            _logger.LogInformation("Starting game detection");

            // Check registry for game path
            var registryPath = GetRegistryGamePath();
            if (!string.IsNullOrEmpty(registryPath) && await ValidateGamePathAsync(registryPath))
            {
                _currentGame = new GameInfo { GamePath = registryPath };
                return _currentGame;
            }

            // Check current directory
            var currentDir = Directory.GetCurrentDirectory();
            if (await ValidateGamePathAsync(currentDir))
            {
                _currentGame = new GameInfo { GamePath = currentDir };
                return _currentGame;
            }

            _logger.LogWarning("Could not detect Fallout 4 installation");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during game detection");
            return null;
        }
    }

    public async Task<bool> ValidateGamePathAsync(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path)) return false;

            var fallout4Exe = Path.Combine(path, "Fallout4.exe");
            var dataDir = Path.Combine(path, "Data");

            return File.Exists(fallout4Exe) && Directory.Exists(dataDir);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating game path: {Path}", path);
            return false;
        }
    }

    public async Task RefreshGameInfoAsync()
    {
        if (_currentGame != null)
        {
            await DetectGameAsync();
        }
    }

    private string? GetRegistryGamePath()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Bethesda Softworks\Fallout4");
            return key?.GetValue("Installed Path") as string;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading registry");
            return null;
        }
    }
}
