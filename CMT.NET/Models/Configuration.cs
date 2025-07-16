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

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CMT.NET.Models;

public class AppSettings
{
    public string Version { get; set; } = "1.0.0";
    public LoggingSettings Logging { get; set; } = new();
    public ScanSettings Scan { get; set; } = new();
    public WindowSettings Window { get; set; } = new();
    public Dictionary<string, string> Paths { get; set; } = new();
    public Dictionary<string, bool> Features { get; set; } = new();

    // UI Properties
    public string? GamePath { get; set; }
    public string ModManager { get; set; } = "Auto-Detect";
    public string? ModManagerPath { get; set; }
    public bool EnableDeepScan { get; set; } = false;
    public bool ScanMissingMasters { get; set; } = true;
    public bool CheckConflicts { get; set; } = true;
    public bool ValidateF4SEPlugins { get; set; } = true;
    public string LogLevel { get; set; } = "Info";
    public bool CheckUpdates { get; set; } = true;
    public bool RememberWindowPosition { get; set; } = true;
    public bool MinimizeToTray { get; set; } = false;
    public string? XEditPath { get; set; }
    public string? BSArchPath { get; set; }
}

public class LoggingSettings
{
    public string LogLevel { get; set; } = "Information";
    public bool EnableFileLogging { get; set; } = true;
    public string LogPath { get; set; } = "logs";
    public int MaxLogFiles { get; set; } = 10;
    public bool EnableConsoleLogging { get; set; } = true;
}

public class ScanSettings
{
    public bool EnableJunkFileDetection { get; set; } = true;
    public bool EnableArchiveAnalysis { get; set; } = true;
    public bool EnableModuleAnalysis { get; set; } = true;
    public bool EnableF4SEAnalysis { get; set; } = true;
    public bool AutoScanOnStartup { get; set; } = false;
    public List<string> IgnorePatterns { get; set; } = new();
    public List<string> CustomPaths { get; set; } = new();
}

public class WindowSettings
{
    public double Width { get; set; } = 800;
    public double Height { get; set; } = 600;
    public double X { get; set; } = 100;
    public double Y { get; set; } = 100;
    public bool IsMaximized { get; set; } = false;
    public string Theme { get; set; } = "Light";
}

public class ModManagerInfo
{
    public string Name { get; set; } = string.Empty;
    public string ExecutablePath { get; set; } = string.Empty;
    public string GamePath { get; set; } = string.Empty;
    public string ModsPath { get; set; } = string.Empty;
    public string DownloadsPath { get; set; } = string.Empty;
    public string ProfilesPath { get; set; } = string.Empty;
    public bool IsPortable { get; set; } = false;
    public bool IsDetected { get; set; } = false;
    public Dictionary<string, string> Settings { get; set; } = new();
}
