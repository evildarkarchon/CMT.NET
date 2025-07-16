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

namespace CMT.NET.Models;

public class FileInfo
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime LastWriteTime { get; set; }
    public uint Crc32 { get; set; }
    public string? Version { get; set; }
}

public class ModInfo
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime LastWriteTime { get; set; }
    public uint Crc32 { get; set; }
    public ModuleType ModuleType { get; set; }
    public uint? Flags { get; set; }
    public uint FormId { get; set; }
    public uint Version { get; set; }
    public float? HeaderVersion { get; set; }
    public uint? RecordCount { get; set; }
    public uint? NextObjectId { get; set; }
    public string? Author { get; set; }
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }
    public List<string> MasterFiles { get; set; } = new();
}

public class ArchiveInfo
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime LastWriteTime { get; set; }
    public uint Crc32 { get; set; }
    public int? Version { get; set; }
    public int FileCount { get; set; }
    public string? ArchiveType { get; set; }
    public string? Format { get; set; }
}

public class F4SeInfo
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsCompatible { get; set; }
    public bool IsWhitelisted { get; set; }
    public string? Description { get; set; }
    public List<F4SeInfo> Children { get; set; } = new();

    // UI Helper Properties
    public string VersionColor => IsCompatible ? "Good" : "Bad";
    public string StatusColor => IsCompatible ? "Good" : "Bad";
}

public class Problem
{
    public ProblemType Type { get; set; }
    public ProblemSeverity Severity { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Solution { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public string? Details { get; set; }

    // UI Helper Properties
    public string SeverityColor => Severity switch
    {
        ProblemSeverity.Info => "Info",
        ProblemSeverity.Warning => "Warning",
        ProblemSeverity.Error => "Bad",
        _ => "Info"
    };
}

public class ProblemGroup
{
    public ProblemType Type { get; set; }
    public Problem[] Problems { get; set; } = Array.Empty<Problem>();
    public int Count { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public int InfoCount { get; set; }

    // UI Helper Properties
    public string TypeName => Type.ToString();
    public string CountText => $"{Count} issue{(Count == 1 ? "" : "s")}";
    public bool HasErrors => ErrorCount > 0;
    public bool HasWarnings => WarningCount > 0;
}

public class IniFile
{
    public string FilePath { get; set; } = string.Empty;
    public Dictionary<string, Dictionary<string, string>> Sections { get; set; } = new();
}

public class GameAnalysisResult
{
    public DateTime AnalysisTimestamp { get; set; }
    public GameInfo? GameInfo { get; set; }
    public List<ModInfo> Modules { get; set; } = new();
    public List<ArchiveInfo> Archives { get; set; } = new();
    public List<F4SeInfo> F4SePlugins { get; set; } = new();
    public List<Problem> Problems { get; set; } = new();
    public Dictionary<string, string> GameConfiguration { get; set; } = new();
    public SystemInfo? SystemInfo { get; set; }
}

public class SystemInfo
{
    public string OperatingSystem { get; set; } = string.Empty;
    public bool Is64BitOperatingSystem { get; set; }
    public int ProcessorCount { get; set; }
    public string ProcessorName { get; set; } = string.Empty;
    public long AvailableMemory { get; set; }
    public string DotNetVersion { get; set; } = string.Empty;
    public string RuntimeVersion { get; set; } = string.Empty;
}

public class GameInfo
{
    public string Name { get; set; } = "Fallout4";
    public InstallType InstallType { get; set; } = InstallType.Unknown;
    public string? GamePath { get; set; }
    public string? GameVersion { get; set; }
    public string? DataPath { get; set; }
    public string? F4SePath { get; set; }
    public Language Language { get; set; } = Language.English;

    // Archive collections
    public HashSet<string> ArchivesGnrl { get; set; } = new();
    public HashSet<string> ArchivesDx10 { get; set; } = new();
    public HashSet<string> ArchivesOg { get; set; } = new();
    public HashSet<string> ArchivesNg { get; set; } = new();
    public HashSet<string> ArchivesEnabled { get; set; } = new();
    public HashSet<string> ArchivesUnreadable { get; set; } = new();

    // Module collections
    public HashSet<string> ModulesUnreadable { get; set; } = new();
    public HashSet<string> ModulesHedr95 { get; set; } = new();
    public Dictionary<string, float> ModulesHedrUnknown { get; set; } = new();
    public List<string> ModulesEnabled { get; set; } = new();

    // File information
    public Dictionary<string, FileInfo> FileInfo { get; set; } = new();

    // Game settings
    public Dictionary<string, Dictionary<string, string>> GameSettings { get; set; } = new();
    public Dictionary<string, Dictionary<string, string>> GamePrefs { get; set; } = new();

    // Counts
    public int Ba2CountGnrl { get; set; }
    public int Ba2CountDx10 { get; set; }
    public int ModuleCountFull { get; set; }
    public int ModuleCountLight { get; set; }
    public int ModuleCountV1 { get; set; }

    // Additional properties
    public string? AddressLibrary { get; set; }
    public bool CkFixesFound { get; set; }
    public string[] Ba2Suffixes { get; set; } = Array.Empty<string>();

    // UI Properties for display
    public string Version => GameVersion ?? "Unknown";
    public string OperatingSystem => Environment.OSVersion.ToString();
    public string Memory => $"{GC.GetTotalMemory(false) / (1024 * 1024 * 1024)}";
    public string Processor => Environment.ProcessorCount.ToString();
    public string GraphicsCard => "Unknown"; // TODO: Get actual graphics card info

    // Helper methods
    public bool IsFoOg() => InstallType is InstallType.OG or InstallType.DG;
    public bool IsFoNg() => InstallType == InstallType.NG;
    public bool IsFoDg() => InstallType == InstallType.DG;

    public void ResetBinaries()
    {
        InstallType = InstallType.Unknown;
        FileInfo.Clear();
        AddressLibrary = null;
        CkFixesFound = false;
    }

    public void ResetModules()
    {
        ModuleCountFull = 0;
        ModuleCountLight = 0;
        ModuleCountV1 = 0;
        ModulesHedr95.Clear();
        ModulesHedrUnknown.Clear();
        ModulesEnabled.Clear();
        ModulesUnreadable.Clear();
    }

    public void ResetArchives()
    {
        Ba2CountGnrl = 0;
        Ba2CountDx10 = 0;
        ArchivesGnrl.Clear();
        ArchivesDx10.Clear();
        ArchivesOg.Clear();
        ArchivesNg.Clear();
        ArchivesEnabled.Clear();
        ArchivesUnreadable.Clear();
    }
}
