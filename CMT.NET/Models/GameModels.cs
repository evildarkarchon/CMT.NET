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

public record FileInfo(
    string Name,
    string Version,
    DateTime? CreationTime,
    DateTime? ModifiedTime,
    long Size,
    string? Hash = null
);

public record ModInfo(
    string Name,
    string FilePath,
    bool IsEnabled,
    bool IsLight,
    float Version,
    string? Description = null
);

public record ArchiveInfo(
    string Name,
    string FilePath,
    ArchiveVersion Version,
    bool IsEnabled,
    long Size,
    DateTime? CreationTime = null
);

public record F4SEInfo(
    string Name,
    string FilePath,
    string Version,
    bool IsCompatible,
    bool IsWhitelisted = false
);

public record ProblemInfo(
    ProblemType Type,
    string Description,
    string FilePath,
    SolutionType SolutionType,
    string? SolutionDetails = null
);

public class GameInfo
{
    public string Name { get; set; } = "Fallout4";
    public InstallType InstallType { get; set; } = InstallType.Unknown;
    public string? GamePath { get; set; }
    public string? DataPath { get; set; }
    public string? F4SEPath { get; set; }
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
