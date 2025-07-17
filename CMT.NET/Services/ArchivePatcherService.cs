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
using System.Threading.Tasks;
using CMT.NET.Models;
using Microsoft.Extensions.Logging;

namespace CMT.NET.Services;

public class ArchivePatcherService : IArchivePatcherService
{
    private readonly IArchiveAnalysisService _archiveAnalysisService;
    private readonly IGameDetectionService _gameDetectionService;
    private readonly IFileOperationService _fileOperationService;
    private readonly ILogger<ArchivePatcherService> _logger;

    private readonly Dictionary<ArchiveVersion, string> _versionDescriptions = new()
    {
        { ArchiveVersion.BA2_GNRL, "General Archives (GNRL)" },
        { ArchiveVersion.BA2_DX10, "DirectX 10 Archives (DX10)" },
        { ArchiveVersion.BA2_GNMF, "General Archives (GNMF)" }
    };

    public ArchivePatcherService(
        IArchiveAnalysisService archiveAnalysisService,
        IGameDetectionService gameDetectionService,
        IFileOperationService fileOperationService,
        ILogger<ArchivePatcherService> logger)
    {
        _archiveAnalysisService = archiveAnalysisService;
        _gameDetectionService = gameDetectionService;
        _fileOperationService = fileOperationService;
        _logger = logger;
    }

    public async Task<IEnumerable<ArchiveInfo>> GetArchivesAsync()
    {
        var gameInfo = await _gameDetectionService.DetectGameAsync();
        if (gameInfo == null)
        {
            _logger.LogWarning("No game detected, returning empty archive list");
            return Array.Empty<ArchiveInfo>();
        }

        var archives = new List<ArchiveInfo>();
        var dataPath = Path.Combine(gameInfo.GamePath, "Data");

        if (!Directory.Exists(dataPath))
        {
            _logger.LogWarning("Data directory not found: {DataPath}", dataPath);
            return archives;
        }

        try
        {
            var archiveFiles = Directory.GetFiles(dataPath, "*.ba2", SearchOption.TopDirectoryOnly);

            foreach (var archiveFile in archiveFiles)
            {
                try
                {
                    var archiveInfo = await _archiveAnalysisService.AnalyzeArchiveAsync(archiveFile);
                    if (archiveInfo != null)
                    {
                        archives.Add(archiveInfo);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error analyzing archive: {ArchiveFile}", archiveFile);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scanning archives in: {DataPath}", dataPath);
        }

        return archives;
    }

    public async Task PatchArchiveAsync(string archivePath, ArchiveVersion targetVersion,
        IProgress<ArchivePatchProgress> progress)
    {
        if (!File.Exists(archivePath))
            throw new FileNotFoundException($"Archive not found: {archivePath}");

        var archiveInfo = await _archiveAnalysisService.AnalyzeArchiveAsync(archivePath);
        if (archiveInfo == null)
            throw new InvalidOperationException($"Unable to analyze archive: {archivePath}");

        if (archiveInfo.ArchiveVersion == targetVersion)
        {
            progress.Report(new ArchivePatchProgress
            {
                Percentage = 100,
                Message = "Archive is already the target version",
                CurrentArchive = Path.GetFileName(archivePath)
            });
            return;
        }

        progress.Report(new ArchivePatchProgress
        {
            Percentage = 0,
            Message = "Starting archive patch...",
            CurrentArchive = Path.GetFileName(archivePath)
        });

        try
        {
            // Create backup
            var backupPath = archivePath + ".backup";
            if (!File.Exists(backupPath))
            {
                progress.Report(new ArchivePatchProgress
                {
                    Percentage = 10,
                    Message = "Creating backup...",
                    CurrentArchive = Path.GetFileName(archivePath)
                });

                File.Copy(archivePath, backupPath);
            }

            progress.Report(new ArchivePatchProgress
            {
                Percentage = 25,
                Message = "Reading archive structure...",
                CurrentArchive = Path.GetFileName(archivePath)
            });

            // Read and modify archive header
            await ModifyArchiveHeaderAsync(archivePath, targetVersion, progress);

            progress.Report(new ArchivePatchProgress
            {
                Percentage = 100,
                Message = "Archive patched successfully!",
                CurrentArchive = Path.GetFileName(archivePath)
            });

            _logger.LogInformation("Successfully patched archive {ArchivePath} to version {TargetVersion}",
                archivePath, targetVersion);
        }
        catch (Exception ex)
        {
            // Restore backup if patching failed
            var backupPath = archivePath + ".backup";
            if (File.Exists(backupPath))
            {
                File.Copy(backupPath, archivePath, true);
            }

            _logger.LogError(ex, "Failed to patch archive {ArchivePath} to version {TargetVersion}",
                archivePath, targetVersion);
            throw;
        }
    }

    public async Task<bool> CanPatchArchiveAsync(string archivePath, ArchiveVersion targetVersion)
    {
        try
        {
            var archiveInfo = await _archiveAnalysisService.AnalyzeArchiveAsync(archivePath);
            if (archiveInfo == null)
                return false;

            // Check if conversion is supported
            return IsConversionSupported(archiveInfo.ArchiveVersion, targetVersion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if archive can be patched: {ArchivePath}", archivePath);
            return false;
        }
    }

    public async Task<IEnumerable<ArchiveVersion>> GetSupportedVersionsAsync()
    {
        await Task.CompletedTask;
        return new[] { ArchiveVersion.BA2_GNRL, ArchiveVersion.BA2_DX10, ArchiveVersion.BA2_GNMF };
    }

    private async Task ModifyArchiveHeaderAsync(string archivePath, ArchiveVersion targetVersion,
        IProgress<ArchivePatchProgress> progress)
    {
        using var fileStream = new FileStream(archivePath, FileMode.Open, FileAccess.ReadWrite);
        using var reader = new BinaryReader(fileStream);
        using var writer = new BinaryWriter(fileStream);

        // Read the current header
        var headerBytes = reader.ReadBytes(4);
        var headerString = System.Text.Encoding.ASCII.GetString(headerBytes);

        progress.Report(new ArchivePatchProgress
        {
            Percentage = 50,
            Message = $"Converting from {headerString} to {GetVersionString(targetVersion)}...",
            CurrentArchive = Path.GetFileName(archivePath)
        });

        // Modify header based on target version
        var newHeaderBytes = GetVersionBytes(targetVersion);

        // Seek back to beginning and write new header
        fileStream.Seek(0, SeekOrigin.Begin);
        writer.Write(newHeaderBytes);

        progress.Report(new ArchivePatchProgress
        {
            Percentage = 75,
            Message = "Updating archive metadata...",
            CurrentArchive = Path.GetFileName(archivePath)
        });

        // Additional modifications might be needed based on version
        await ModifyArchiveMetadataAsync(fileStream, targetVersion, progress);
    }

    private async Task ModifyArchiveMetadataAsync(FileStream fileStream, ArchiveVersion targetVersion,
        IProgress<ArchivePatchProgress> progress)
    {
        // This is where version-specific metadata changes would be implemented
        // For now, just complete the operation
        await Task.CompletedTask;
    }

    private bool IsConversionSupported(ArchiveVersion sourceVersion, ArchiveVersion targetVersion)
    {
        // Define supported conversions
        var supportedConversions = new Dictionary<ArchiveVersion, ArchiveVersion[]>
        {
            { ArchiveVersion.BA2_GNRL, new[] { ArchiveVersion.BA2_DX10 } },
            { ArchiveVersion.BA2_DX10, new[] { ArchiveVersion.BA2_GNRL } },
            { ArchiveVersion.BA2_GNMF, new[] { ArchiveVersion.BA2_GNRL, ArchiveVersion.BA2_DX10 } }
        };

        return supportedConversions.ContainsKey(sourceVersion) &&
               supportedConversions[sourceVersion].Contains(targetVersion);
    }

    private byte[] GetVersionBytes(ArchiveVersion version)
    {
        return version switch
        {
            ArchiveVersion.BA2_GNRL => System.Text.Encoding.ASCII.GetBytes("GNRL"),
            ArchiveVersion.BA2_DX10 => System.Text.Encoding.ASCII.GetBytes("DX10"),
            ArchiveVersion.BA2_GNMF => System.Text.Encoding.ASCII.GetBytes("GNMF"),
            _ => throw new ArgumentException($"Unsupported archive version: {version}")
        };
    }

    private string GetVersionString(ArchiveVersion version)
    {
        return version switch
        {
            ArchiveVersion.BA2_GNRL => "GNRL",
            ArchiveVersion.BA2_DX10 => "DX10",
            ArchiveVersion.BA2_GNMF => "GNMF",
            _ => version.ToString()
        };
    }
}
