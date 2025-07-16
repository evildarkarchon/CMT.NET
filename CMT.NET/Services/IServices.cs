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

public interface IFileOperationService
{
    Task<string> CalculateCrc32Async(string filePath);
    Task<bool> ValidateFileIntegrityAsync(string filePath, string expectedHash);
    Task<Models.FileInfo> GetFileInfoAsync(string filePath);
    Task<byte[]> ReadFileHeaderAsync(string filePath, int length);
}

public interface IArchiveService
{
    Task<ArchiveInfo[]> ScanArchivesAsync(string dataPath);
    Task<ArchiveVersion> GetArchiveVersionAsync(string archivePath);
    Task<bool> ValidateArchiveAsync(string archivePath);
    Task ConvertArchiveAsync(string archivePath, ArchiveVersion targetVersion);
}

public interface IModuleService
{
    Task<ModInfo[]> ScanModulesAsync(string dataPath);
    Task<float> GetModuleVersionAsync(string modulePath);
    Task<bool> ValidateModuleAsync(string modulePath);
    Task<bool> IsModuleEnabledAsync(string modulePath);
}

public interface IF4SEService
{
    Task<F4SEInfo[]> ScanF4SEPluginsAsync(string f4sePath);
    Task<bool> ValidateF4SECompatibilityAsync(string pluginPath);
    Task<string> GetF4SEVersionAsync(string f4sePath);
    Task<bool> IsPluginWhitelistedAsync(string pluginPath);
}

public interface IScannerService
{
    Task<ProblemInfo[]> ScanForProblemsAsync(GameInfo gameInfo);
    Task<ProblemInfo[]> ScanSpecificPathAsync(string path);
    Task<bool> CanFixProblemAsync(ProblemInfo problem);
    Task<bool> FixProblemAsync(ProblemInfo problem);
}
