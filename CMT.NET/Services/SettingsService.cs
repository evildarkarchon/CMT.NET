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
using System.Text.Json;
using System.Threading.Tasks;
using CMT.NET.Models;
using Microsoft.Extensions.Logging;

namespace CMT.NET.Services;

public class SettingsService : ISettingsService
{
    private readonly ILogger<SettingsService> _logger;
    private readonly string _settingsPath;
    private readonly JsonSerializerOptions _jsonOptions;

    public SettingsService(ILogger<SettingsService> logger)
    {
        _logger = logger;
        _settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CMT.NET",
            "settings.json"
        );

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        Settings = new AppSettings();
    }

    public AppSettings Settings { get; private set; }

    public async Task LoadSettingsAsync()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                _logger.LogInformation("Settings file not found, using defaults");
                Settings = new AppSettings();
                await SaveSettingsAsync();
                return;
            }

            var json = await File.ReadAllTextAsync(_settingsPath);
            Settings = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions) ?? new AppSettings();

            _logger.LogInformation("Settings loaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load settings, using defaults");
            Settings = new AppSettings();
        }
    }

    public async Task SaveSettingsAsync()
    {
        try
        {
            var directory = Path.GetDirectoryName(_settingsPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(Settings, _jsonOptions);
            await File.WriteAllTextAsync(_settingsPath, json);

            _logger.LogInformation("Settings saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save settings");
            throw;
        }
    }

    public async Task ResetToDefaultsAsync()
    {
        Settings = new AppSettings();
        await SaveSettingsAsync();
        _logger.LogInformation("Settings reset to defaults");
    }
}
