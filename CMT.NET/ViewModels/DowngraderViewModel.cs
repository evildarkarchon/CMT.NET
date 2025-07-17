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
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using CMT.NET.Models;
using CMT.NET.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace CMT.NET.ViewModels;

public class DowngraderViewModel : ViewModelBase
{
    private readonly IGameDetectionService _gameDetectionService;
    private readonly IDowngraderService _downgraderService;

    public DowngraderViewModel(IGameDetectionService gameDetectionService, IDowngraderService downgraderService)
    {
        _gameDetectionService = gameDetectionService;
        _downgraderService = downgraderService;

        DowngradeCommand = ReactiveCommand.CreateFromTask(DowngradeAsync, this.WhenAnyValue(x => x.CanDowngrade));
        UpgradeCommand = ReactiveCommand.CreateFromTask(UpgradeAsync, this.WhenAnyValue(x => x.CanUpgrade));
        RefreshCommand = ReactiveCommand.CreateFromTask(RefreshAsync);
        CreateBackupCommand = ReactiveCommand.CreateFromTask(CreateBackupAsync);
        RestoreBackupCommand = ReactiveCommand.CreateFromTask(RestoreBackupAsync, this.WhenAnyValue(x => x.HasBackup));

        // Initialize
        _ = RefreshAsync();
    }

    [Reactive] public string GamePath { get; set; } = string.Empty;
    [Reactive] public string CurrentVersion { get; set; } = "Unknown";
    [Reactive] public string TargetVersion { get; set; } = "1.10.163";
    [Reactive] public bool IsProcessing { get; set; }
    [Reactive] public string ProcessingStatus { get; set; } = string.Empty;
    [Reactive] public double Progress { get; set; }
    [Reactive] public bool CanDowngrade { get; set; }
    [Reactive] public bool CanUpgrade { get; set; }
    [Reactive] public bool HasBackup { get; set; }
    [Reactive] public string BackupPath { get; set; } = string.Empty;
    [Reactive] public ObservableCollection<GameVersion> AvailableVersions { get; set; } = new();
    [Reactive] public GameVersion? SelectedVersion { get; set; }

    public ReactiveCommand<Unit, Unit> DowngradeCommand { get; }
    public ReactiveCommand<Unit, Unit> UpgradeCommand { get; }
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
    public ReactiveCommand<Unit, Unit> CreateBackupCommand { get; }
    public ReactiveCommand<Unit, Unit> RestoreBackupCommand { get; }

    private async Task RefreshAsync()
    {
        IsProcessing = true;
        ProcessingStatus = "Detecting game...";

        try
        {
            var gameInfo = await _gameDetectionService.DetectGameAsync();
            if (gameInfo != null)
            {
                GamePath = gameInfo.GamePath;
                CurrentVersion = gameInfo.GameVersion;

                // Check for backup
                HasBackup = await _downgraderService.HasBackupAsync();
                if (HasBackup)
                {
                    BackupPath = await _downgraderService.GetBackupPathAsync();
                }

                // Load available versions
                var versions = await _downgraderService.GetAvailableVersionsAsync();
                AvailableVersions.Clear();
                foreach (var version in versions)
                {
                    AvailableVersions.Add(version);
                }

                // Set default target version
                if (AvailableVersions.Count > 0)
                {
                    SelectedVersion = AvailableVersions[0];
                }

                UpdateCanDowngradeUpgrade();
            }
        }
        catch (Exception ex)
        {
            ProcessingStatus = $"Error: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    private async Task DowngradeAsync()
    {
        if (SelectedVersion == null) return;

        IsProcessing = true;
        Progress = 0;
        ProcessingStatus = "Starting downgrade...";

        try
        {
            await _downgraderService.DowngradeToVersionAsync(SelectedVersion,
                new Progress<DowngradeProgress>(OnProgressChanged));
            ProcessingStatus = "Downgrade completed successfully!";
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            ProcessingStatus = $"Downgrade failed: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    private async Task UpgradeAsync()
    {
        if (!HasBackup) return;

        IsProcessing = true;
        Progress = 0;
        ProcessingStatus = "Starting upgrade...";

        try
        {
            await _downgraderService.RestoreFromBackupAsync(new Progress<DowngradeProgress>(OnProgressChanged));
            ProcessingStatus = "Upgrade completed successfully!";
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            ProcessingStatus = $"Upgrade failed: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    private async Task CreateBackupAsync()
    {
        IsProcessing = true;
        Progress = 0;
        ProcessingStatus = "Creating backup...";

        try
        {
            await _downgraderService.CreateBackupAsync(new Progress<DowngradeProgress>(OnProgressChanged));
            ProcessingStatus = "Backup created successfully!";
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            ProcessingStatus = $"Backup creation failed: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    private async Task RestoreBackupAsync()
    {
        IsProcessing = true;
        Progress = 0;
        ProcessingStatus = "Restoring backup...";

        try
        {
            await _downgraderService.RestoreFromBackupAsync(new Progress<DowngradeProgress>(OnProgressChanged));
            ProcessingStatus = "Backup restored successfully!";
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            ProcessingStatus = $"Backup restore failed: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    private void OnProgressChanged(DowngradeProgress progress)
    {
        Progress = progress.Percentage;
        ProcessingStatus = progress.Message;
    }

    private void UpdateCanDowngradeUpgrade()
    {
        CanDowngrade = !string.IsNullOrEmpty(GamePath) && SelectedVersion != null &&
                       CurrentVersion != SelectedVersion.Version;
        CanUpgrade = HasBackup && !string.IsNullOrEmpty(BackupPath);
    }
}
