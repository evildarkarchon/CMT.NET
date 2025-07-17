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
using System.Threading.Tasks;
using CMT.NET.Models;
using CMT.NET.ViewModels;

namespace CMT.NET.Services;

public interface INavigationService
{
    event EventHandler<Tab>? TabChanged;
    Tab CurrentTab { get; }
    void NavigateTo(Tab tab);
    ViewModelBase? GetViewModel(Tab tab);
}

public interface ISettingsService
{
    AppSettings Settings { get; }
    Task LoadSettingsAsync();
    Task SaveSettingsAsync();
    Task ResetToDefaultsAsync();
}

public interface IGameDetectionService
{
    GameInfo? CurrentGame { get; }
    Task<GameInfo?> DetectGameAsync();
    Task<bool> ValidateGamePathAsync(string path);
    Task RefreshGameInfoAsync();
}

public interface IDowngraderService
{
    Task<IEnumerable<GameVersion>> GetAvailableVersionsAsync();
    Task<bool> HasBackupAsync();
    Task<string> GetBackupPathAsync();
    Task CreateBackupAsync(IProgress<DowngradeProgress> progress);
    Task RestoreFromBackupAsync(IProgress<DowngradeProgress> progress);
    Task DowngradeToVersionAsync(GameVersion version, IProgress<DowngradeProgress> progress);
    Task<bool> ValidateGameFileAsync(string filePath);
}

public interface IFileOperationService
{
    Task<uint> CalculateCrc32Async(string filePath);
    Task<bool> ValidateFileIntegrityAsync(string filePath, uint expectedCrc32);
    Task<FileInfo?> GetFileInfoAsync(string filePath);
    Task<string?> GetFileVersionAsync(string filePath);
    bool IsValidGameDirectory(string path);
    Task<bool> BackupFileAsync(string sourceFilePath, string backupDirectory);
}

public interface IIniFileService
{
    Task<IniFile?> ReadIniFileAsync(string filePath);
    Task<bool> WriteIniFileAsync(string filePath, IniFile iniFile);
    string? GetValue(IniFile iniFile, string section, string key, string? defaultValue = null);
    bool SetValue(IniFile iniFile, string section, string key, string value);
    bool HasSection(IniFile iniFile, string section);
    bool HasKey(IniFile iniFile, string section, string key);
    string[] GetSections(IniFile iniFile);
    string[] GetKeys(IniFile iniFile, string section);
    Task<Dictionary<string, string>> GetFallout4PreferencesAsync(string gamePath);
}

public interface IModuleAnalysisService
{
    Task<ModInfo?> AnalyzeModuleAsync(string filePath);
    Task<List<ModInfo>> AnalyzeModulesInDirectoryAsync(string directoryPath);
    Task<bool> IsValidModuleAsync(string filePath);
    Task<List<string>> GetMasterFilesAsync(ModInfo modInfo);
    bool IsLightModule(ModInfo modInfo);
    bool IsMasterModule(ModInfo modInfo);
}

public interface IArchiveAnalysisService
{
    Task<ArchiveInfo?> AnalyzeArchiveAsync(string filePath);
    Task<List<ArchiveInfo>> AnalyzeArchivesInDirectoryAsync(string directoryPath);
    Task<bool> IsValidArchiveAsync(string filePath);
    ArchiveVersion GetArchiveVersion(ArchiveInfo archiveInfo);
}

public interface ICmCheckerService
{
    GameAnalysisResult? LastAnalysisResult { get; }
    bool IsAnalyzing { get; }
    Task<GameAnalysisResult> AnalyzeGameInstallationAsync(string? gamePath = null);
    Task<List<Problem>> ScanForProblemsAsync();
    Task<SystemInfo> GetSystemInfoAsync();
}

public interface IArchivePatcherService
{
    Task<IEnumerable<ArchiveInfo>> GetArchivesAsync();
    Task PatchArchiveAsync(string archivePath, ArchiveVersion targetVersion, IProgress<ArchivePatchProgress> progress);
    Task<bool> CanPatchArchiveAsync(string archivePath, ArchiveVersion targetVersion);
    Task<IEnumerable<ArchiveVersion>> GetSupportedVersionsAsync();
}
