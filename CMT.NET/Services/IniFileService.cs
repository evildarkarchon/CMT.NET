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
using System.Text;
using System.Threading.Tasks;
using CMT.NET.Models;
using Microsoft.Extensions.Logging;

namespace CMT.NET.Services;

public class IniFileService : IIniFileService
{
    private readonly ILogger<IniFileService> _logger;

    public IniFileService(ILogger<IniFileService> logger)
    {
        _logger = logger;
    }

    public async Task<IniFile?> ReadIniFileAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("INI file not found: {FilePath}", filePath);
                return null;
            }

            var lines = await File.ReadAllLinesAsync(filePath, Encoding.UTF8);
            var iniFile = new IniFile { FilePath = filePath };
            var currentSection = string.Empty;

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                // Skip empty lines and comments
                if (string.IsNullOrEmpty(line) || line.StartsWith(';') || line.StartsWith('#'))
                    continue;

                // Parse section headers
                if (line.StartsWith('[') && line.EndsWith(']'))
                {
                    currentSection = line[1..^1].Trim();
                    if (!iniFile.Sections.ContainsKey(currentSection))
                    {
                        iniFile.Sections[currentSection] = new Dictionary<string, string>();
                    }

                    continue;
                }

                // Parse key-value pairs
                var equalIndex = line.IndexOf('=');
                if (equalIndex > 0)
                {
                    var key = line[..equalIndex].Trim();
                    var value = line[(equalIndex + 1)..].Trim();

                    if (!iniFile.Sections.ContainsKey(currentSection))
                    {
                        iniFile.Sections[currentSection] = new Dictionary<string, string>();
                    }

                    iniFile.Sections[currentSection][key] = value;
                }
            }

            _logger.LogDebug("Successfully parsed INI file: {FilePath} with {SectionCount} sections",
                filePath, iniFile.Sections.Count);

            return iniFile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading INI file: {FilePath}", filePath);
            return null;
        }
    }

    public async Task<bool> WriteIniFileAsync(string filePath, IniFile iniFile)
    {
        try
        {
            var lines = new List<string>();

            foreach (var section in iniFile.Sections)
            {
                if (lines.Count > 0)
                    lines.Add(string.Empty); // Add blank line between sections

                lines.Add($"[{section.Key}]");

                foreach (var kvp in section.Value)
                {
                    lines.Add($"{kvp.Key}={kvp.Value}");
                }
            }

            await File.WriteAllLinesAsync(filePath, lines, Encoding.UTF8);

            _logger.LogInformation("Successfully wrote INI file: {FilePath}", filePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing INI file: {FilePath}", filePath);
            return false;
        }
    }

    public string? GetValue(IniFile iniFile, string section, string key, string? defaultValue = null)
    {
        try
        {
            if (iniFile.Sections.TryGetValue(section, out var sectionData) &&
                sectionData.TryGetValue(key, out var value))
            {
                return value;
            }

            return defaultValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting INI value: [{Section}] {Key}", section, key);
            return defaultValue;
        }
    }

    public bool SetValue(IniFile iniFile, string section, string key, string value)
    {
        try
        {
            if (!iniFile.Sections.ContainsKey(section))
            {
                iniFile.Sections[section] = new Dictionary<string, string>();
            }

            iniFile.Sections[section][key] = value;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting INI value: [{Section}] {Key} = {Value}", section, key, value);
            return false;
        }
    }

    public bool HasSection(IniFile iniFile, string section)
    {
        return iniFile.Sections.ContainsKey(section);
    }

    public bool HasKey(IniFile iniFile, string section, string key)
    {
        return iniFile.Sections.TryGetValue(section, out var sectionData) &&
               sectionData.ContainsKey(key);
    }

    public string[] GetSections(IniFile iniFile)
    {
        return iniFile.Sections.Keys.ToArray();
    }

    public string[] GetKeys(IniFile iniFile, string section)
    {
        if (iniFile.Sections.TryGetValue(section, out var sectionData))
        {
            return sectionData.Keys.ToArray();
        }

        return Array.Empty<string>();
    }

    public async Task<Dictionary<string, string>> GetFallout4PreferencesAsync(string gamePath)
    {
        var preferences = new Dictionary<string, string>();

        try
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var fallout4IniPath = Path.Combine(documentsPath, "My Games", "Fallout4", "Fallout4.ini");
            var fallout4PrefsPath = Path.Combine(documentsPath, "My Games", "Fallout4", "Fallout4Prefs.ini");

            // Read main Fallout4.ini
            var mainIni = await ReadIniFileAsync(fallout4IniPath);
            if (mainIni != null)
            {
                foreach (var section in mainIni.Sections)
                {
                    foreach (var kvp in section.Value)
                    {
                        preferences[$"{section.Key}.{kvp.Key}"] = kvp.Value;
                    }
                }
            }

            // Read Fallout4Prefs.ini (overrides main ini)
            var prefsIni = await ReadIniFileAsync(fallout4PrefsPath);
            if (prefsIni != null)
            {
                foreach (var section in prefsIni.Sections)
                {
                    foreach (var kvp in section.Value)
                    {
                        preferences[$"{section.Key}.{kvp.Key}"] = kvp.Value;
                    }
                }
            }

            _logger.LogInformation("Loaded {Count} Fallout 4 preferences", preferences.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Fallout 4 preferences");
        }

        return preferences;
    }
}
