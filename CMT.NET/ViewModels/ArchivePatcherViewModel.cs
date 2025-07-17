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
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CMT.NET.Models;
using CMT.NET.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace CMT.NET.ViewModels;

public class ArchivePatcherViewModel : ViewModelBase
{
    private readonly IArchivePatcherService _archivePatcherService;
    private readonly IGameDetectionService _gameDetectionService;

    public ArchivePatcherViewModel(IArchivePatcherService archivePatcherService,
        IGameDetectionService gameDetectionService)
    {
        _archivePatcherService = archivePatcherService;
        _gameDetectionService = gameDetectionService;

        RefreshCommand = ReactiveCommand.CreateFromTask(RefreshAsync);
        PatchSelectedCommand = ReactiveCommand.CreateFromTask(PatchSelectedAsync,
            this.WhenAnyValue(x => x.SelectedArchives, x => x.SelectedTargetVersion,
                (archives, version) => archives.Any() && version != ArchiveVersion.Unknown));
        PatchAllCommand = ReactiveCommand.CreateFromTask(PatchAllAsync,
            this.WhenAnyValue(x => x.SelectedTargetVersion, version => version != ArchiveVersion.Unknown));
        SelectAllCommand = ReactiveCommand.Create(SelectAll);
        SelectNoneCommand = ReactiveCommand.Create(SelectNone);
        ApplyFilterCommand = ReactiveCommand.Create(ApplyFilter);

        // Set up automatic updating of SelectedArchives based on IsSelected properties
        this.WhenAnyValue(x => x.FilteredArchives)
            .Subscribe(_ => UpdateSelectedArchives());

        // Initialize
        _ = RefreshAsync();
    }

    [Reactive] public ObservableCollection<ArchiveInfo> Archives { get; set; } = new();
    [Reactive] public ObservableCollection<ArchiveInfo> FilteredArchives { get; set; } = new();
    [Reactive] public ObservableCollection<ArchiveInfo> SelectedArchives { get; set; } = new();
    [Reactive] public ObservableCollection<ArchiveVersion> AvailableVersions { get; set; } = new();
    [Reactive] public ArchiveVersion SelectedTargetVersion { get; set; } = ArchiveVersion.Unknown;
    [Reactive] public string NameFilter { get; set; } = string.Empty;
    [Reactive] public ArchiveVersion VersionFilter { get; set; } = ArchiveVersion.Unknown;
    [Reactive] public bool IsProcessing { get; set; }
    [Reactive] public string ProcessingStatus { get; set; } = string.Empty;
    [Reactive] public double Progress { get; set; }
    [Reactive] public int TotalArchives { get; set; }
    [Reactive] public int ProcessedArchives { get; set; }
    [Reactive] public int SuccessfulPatches { get; set; }
    [Reactive] public int FailedPatches { get; set; }

    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
    public ReactiveCommand<Unit, Unit> PatchSelectedCommand { get; }
    public ReactiveCommand<Unit, Unit> PatchAllCommand { get; }
    public ReactiveCommand<Unit, Unit> SelectAllCommand { get; }
    public ReactiveCommand<Unit, Unit> SelectNoneCommand { get; }
    public ReactiveCommand<Unit, Unit> ApplyFilterCommand { get; }

    private async Task RefreshAsync()
    {
        IsProcessing = true;
        ProcessingStatus = "Loading archives...";

        try
        {
            // Load archives
            var archives = await _archivePatcherService.GetArchivesAsync();
            Archives.Clear();
            foreach (var archive in archives)
            {
                // Subscribe to IsSelected changes for each archive
                archive.WhenAnyValue(x => x.IsSelected)
                    .Subscribe(_ => UpdateSelectedArchives());
                Archives.Add(archive);
            }

            // Load available versions
            var versions = await _archivePatcherService.GetSupportedVersionsAsync();
            AvailableVersions.Clear();
            foreach (var version in versions)
            {
                AvailableVersions.Add(version);
            }

            // Apply current filter
            ApplyFilter();

            ProcessingStatus = $"Loaded {Archives.Count} archives";
        }
        catch (Exception ex)
        {
            ProcessingStatus = $"Error loading archives: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    private async Task PatchSelectedAsync()
    {
        if (SelectedArchives.Count == 0 || SelectedTargetVersion == ArchiveVersion.Unknown)
            return;

        await PatchArchivesAsync(SelectedArchives.ToList());
    }

    private async Task PatchAllAsync()
    {
        if (SelectedTargetVersion == ArchiveVersion.Unknown)
            return;

        // Filter archives that can be patched to the target version
        var patchableArchives = new List<ArchiveInfo>();
        if (SelectedTargetVersion != ArchiveVersion.Unknown)
        {
            foreach (var archive in FilteredArchives)
            {
                if (await _archivePatcherService.CanPatchArchiveAsync(archive.FilePath, SelectedTargetVersion))
                {
                    patchableArchives.Add(archive);
                }
            }
        }

        if (patchableArchives.Count == 0)
        {
            ProcessingStatus = "No archives can be patched to the selected version";
            return;
        }

        await PatchArchivesAsync(patchableArchives);
    }

    private async Task PatchArchivesAsync(List<ArchiveInfo> archivesToPatch)
    {
        var targetVersion = SelectedTargetVersion;
        if (targetVersion == ArchiveVersion.Unknown)
            return;
        IsProcessing = true;
        Progress = 0;
        ProcessedArchives = 0;
        SuccessfulPatches = 0;
        FailedPatches = 0;
        TotalArchives = archivesToPatch.Count;

        ProcessingStatus = $"Patching {TotalArchives} archives...";

        try
        {
            for (int i = 0; i < archivesToPatch.Count; i++)
            {
                var archive = archivesToPatch[i];
                ProcessingStatus = $"Patching {archive.FileName}... ({i + 1}/{TotalArchives})";

                try
                {
                    var progress = new Progress<ArchivePatchProgress>(OnArchivePatchProgress);
                    await _archivePatcherService.PatchArchiveAsync(archive.FilePath, targetVersion, progress);
                    SuccessfulPatches++;
                }
                catch (Exception ex)
                {
                    FailedPatches++;
                    // Log error but continue with other archives
                    ProcessingStatus = $"Failed to patch {archive.FileName}: {ex.Message}";
                    await Task.Delay(1000); // Brief pause to show error
                }

                ProcessedArchives++;
                Progress = (double)ProcessedArchives / TotalArchives * 100;
            }

            ProcessingStatus = $"Patching completed! {SuccessfulPatches} successful, {FailedPatches} failed";

            // Refresh archives to show updated versions
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            ProcessingStatus = $"Patching failed: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    private void SelectAll()
    {
        foreach (var archive in FilteredArchives)
        {
            archive.IsSelected = true;
        }

        UpdateSelectedArchives();
    }

    private void SelectNone()
    {
        foreach (var archive in FilteredArchives)
        {
            archive.IsSelected = false;
        }

        UpdateSelectedArchives();
    }

    private void UpdateSelectedArchives()
    {
        SelectedArchives.Clear();
        foreach (var archive in FilteredArchives.Where(a => a.IsSelected))
        {
            SelectedArchives.Add(archive);
        }
    }

    private void ApplyFilter()
    {
        FilteredArchives.Clear();

        foreach (var archive in Archives)
        {
            bool matchesNameFilter = string.IsNullOrEmpty(NameFilter) ||
                                     archive.FileName.Contains(NameFilter, StringComparison.OrdinalIgnoreCase);

            bool matchesVersionFilter = VersionFilter == ArchiveVersion.Unknown ||
                                        archive.ArchiveVersion == VersionFilter;

            if (matchesNameFilter && matchesVersionFilter)
            {
                FilteredArchives.Add(archive);
            }
        }

        UpdateSelectedArchives();
        ProcessingStatus = $"Showing {FilteredArchives.Count} of {Archives.Count} archives";
    }

    private void OnArchivePatchProgress(ArchivePatchProgress progress)
    {
        // Update sub-progress for current archive
        // This could be displayed in a detailed progress view
    }
}
