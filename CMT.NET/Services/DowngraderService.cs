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
using System.IO.Hashing;
using System.Net.Http;
using System.Threading.Tasks;
using CMT.NET.Models;
using Microsoft.Extensions.Logging;
using xdelta3.net;

namespace CMT.NET.Services;

public class DowngraderService : IDowngraderService
{
    private readonly IGameDetectionService _gameDetectionService;
    private readonly IFileOperationService _fileOperationService;
    private readonly ILogger<DowngraderService> _logger;
    private readonly HttpClient _httpClient;

    private const string PatchUrlBase =
        "https://github.com/wxMichael/Collective-Modding-Toolkit/releases/download/delta-patches/";

    private const string BackupFolderName = "CMT_Backup";

    private readonly Dictionary<string, GameVersion> _availableVersions = new()
    {
        {
            "1.10.163", new GameVersion
            {
                Version = "1.10.163",
                DisplayName = "Version 1.10.163 (Original)",
                Description = "Original Steam version with full mod support",
                PatchUrl = PatchUrlBase + "1.10.163.xdelta",
                ExpectedCrc32 = 0x1234567, // TODO: Get actual CRC32
                FileSize = 64424960, // TODO: Get actual file size
                IsOriginal = true,
                IsNextGen = false,
                IsDowngrade = true
            }
        },
        {
            "1.10.980", new GameVersion
            {
                Version = "1.10.980",
                DisplayName = "Version 1.10.980 (Next-Gen)",
                Description = "Next-Gen update version",
                PatchUrl = string.Empty,
                ExpectedCrc32 = 0x7654321, // TODO: Get actual CRC32
                FileSize = 65536000, // TODO: Get actual file size
                IsOriginal = false,
                IsNextGen = true,
                IsDowngrade = false
            }
        }
    };

    public DowngraderService(
        IGameDetectionService gameDetectionService,
        IFileOperationService fileOperationService,
        ILogger<DowngraderService> logger,
        HttpClient httpClient)
    {
        _gameDetectionService = gameDetectionService;
        _fileOperationService = fileOperationService;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<GameVersion>> GetAvailableVersionsAsync()
    {
        await Task.CompletedTask;
        return _availableVersions.Values;
    }

    public async Task<bool> HasBackupAsync()
    {
        var gameInfo = await _gameDetectionService.DetectGameAsync();
        if (gameInfo?.GamePath == null)
            return false;

        var backupPath = GetBackupPath(gameInfo.GamePath);
        return Directory.Exists(backupPath) && File.Exists(Path.Combine(backupPath, "Fallout4.exe"));
    }

    public async Task<string> GetBackupPathAsync()
    {
        var gameInfo = await _gameDetectionService.DetectGameAsync();
        if (gameInfo?.GamePath == null)
            throw new InvalidOperationException("Game not detected");

        return GetBackupPath(gameInfo.GamePath);
    }

    public async Task CreateBackupAsync(IProgress<DowngradeProgress> progress)
    {
        var gameInfo = await _gameDetectionService.DetectGameAsync();
        if (gameInfo?.GamePath == null)
            throw new InvalidOperationException("Game not detected");

        var backupPath = GetBackupPath(gameInfo.GamePath);
        var gameExePath = Path.Combine(gameInfo.GamePath, "Fallout4.exe");

        if (!File.Exists(gameExePath))
            throw new FileNotFoundException("Fallout4.exe not found in game directory");

        progress.Report(new DowngradeProgress
        {
            Percentage = 0,
            Message = "Creating backup directory..."
        });

        Directory.CreateDirectory(backupPath);

        progress.Report(new DowngradeProgress
        {
            Percentage = 25,
            Message = "Copying game executable..."
        });

        var backupExePath = Path.Combine(backupPath, "Fallout4.exe");
        File.Copy(gameExePath, backupExePath, true);

        progress.Report(new DowngradeProgress
        {
            Percentage = 50,
            Message = "Copying additional files..."
        });

        // Copy other important files
        var filesToBackup = new[]
        {
            "Fallout4Launcher.exe",
            "steam_api64.dll",
            "Fallout4.exe"
        };

        var totalFiles = filesToBackup.Length;
        var processedFiles = 0;

        foreach (var file in filesToBackup)
        {
            var sourceFile = Path.Combine(gameInfo.GamePath, file);
            var targetFile = Path.Combine(backupPath, file);

            if (File.Exists(sourceFile))
            {
                File.Copy(sourceFile, targetFile, true);
            }

            processedFiles++;
            var percentage = 50 + (processedFiles * 50 / totalFiles);
            progress.Report(new DowngradeProgress
            {
                Percentage = percentage,
                Message = $"Backed up {file}..."
            });
        }

        progress.Report(new DowngradeProgress
        {
            Percentage = 100,
            Message = "Backup completed successfully!"
        });

        _logger.LogInformation("Backup created at: {BackupPath}", backupPath);
    }

    public async Task RestoreFromBackupAsync(IProgress<DowngradeProgress> progress)
    {
        var gameInfo = await _gameDetectionService.DetectGameAsync();
        if (gameInfo?.GamePath == null)
            throw new InvalidOperationException("Game not detected");

        var backupPath = GetBackupPath(gameInfo.GamePath);
        if (!Directory.Exists(backupPath))
            throw new DirectoryNotFoundException("Backup directory not found");

        progress.Report(new DowngradeProgress
        {
            Percentage = 0,
            Message = "Restoring from backup..."
        });

        var backupFiles = Directory.GetFiles(backupPath);
        var totalFiles = backupFiles.Length;
        var processedFiles = 0;

        foreach (var backupFile in backupFiles)
        {
            var fileName = Path.GetFileName(backupFile);
            var targetFile = Path.Combine(gameInfo.GamePath, fileName);

            File.Copy(backupFile, targetFile, true);

            processedFiles++;
            var percentage = processedFiles * 100 / totalFiles;
            progress.Report(new DowngradeProgress
            {
                Percentage = percentage,
                Message = $"Restored {fileName}..."
            });
        }

        progress.Report(new DowngradeProgress
        {
            Percentage = 100,
            Message = "Restore completed successfully!"
        });

        _logger.LogInformation("Files restored from backup: {BackupPath}", backupPath);
    }

    public async Task DowngradeToVersionAsync(GameVersion version, IProgress<DowngradeProgress> progress)
    {
        var gameInfo = await _gameDetectionService.DetectGameAsync();
        if (gameInfo?.GamePath == null)
            throw new InvalidOperationException("Game not detected");

        var gameExePath = Path.Combine(gameInfo.GamePath, "Fallout4.exe");
        if (!File.Exists(gameExePath))
            throw new FileNotFoundException("Fallout4.exe not found");

        progress.Report(new DowngradeProgress
        {
            Percentage = 0,
            Message = "Validating current game file..."
        });

        // Validate current file
        if (!await ValidateGameFileAsync(gameExePath))
        {
            throw new InvalidOperationException("Current game file is corrupted or unknown version");
        }

        progress.Report(new DowngradeProgress
        {
            Percentage = 10,
            Message = "Downloading patch file..."
        });

        // Download patch file
        var patchData = await DownloadPatchAsync(version.PatchUrl);

        progress.Report(new DowngradeProgress
        {
            Percentage = 50,
            Message = "Applying patch..."
        });

        // Apply patch
        var originalData = await File.ReadAllBytesAsync(gameExePath);
        var patchedData = await ApplyPatchAsync(originalData, patchData);

        progress.Report(new DowngradeProgress
        {
            Percentage = 90,
            Message = "Saving patched file..."
        });

        // Save patched file
        var tempPath = gameExePath + ".tmp";
        await File.WriteAllBytesAsync(tempPath, patchedData);

        // Verify patched file
        if (!await ValidatePatchedFileAsync(tempPath, version.ExpectedCrc32))
        {
            File.Delete(tempPath);
            throw new InvalidOperationException("Patched file validation failed");
        }

        // Replace original file
        File.Replace(tempPath, gameExePath, null);

        progress.Report(new DowngradeProgress
        {
            Percentage = 100,
            Message = $"Successfully downgraded to version {version.Version}!"
        });

        _logger.LogInformation("Game downgraded to version: {Version}", version.Version);
    }

    public async Task<bool> ValidateGameFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return false;

        try
        {
            var crc32 = await _fileOperationService.CalculateCrc32Async(filePath);

            // Check if CRC32 matches any known version
            foreach (var version in _availableVersions.Values)
            {
                if (version.ExpectedCrc32 == crc32)
                    return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating game file: {FilePath}", filePath);
            return false;
        }
    }

    private string GetBackupPath(string gamePath)
    {
        return Path.Combine(Path.GetDirectoryName(gamePath)!, BackupFolderName);
    }

    private async Task<byte[]> DownloadPatchAsync(string url)
    {
        if (string.IsNullOrEmpty(url))
            throw new ArgumentException("Patch URL is empty");

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsByteArrayAsync();
    }

    private async Task<byte[]> ApplyPatchAsync(byte[] originalData, byte[] patchData)
    {
        return await Task.Run(() =>
        {
            try
            {
                // Validate input data
                if (originalData == null || originalData.Length == 0)
                    throw new ArgumentException("Original data cannot be null or empty", nameof(originalData));

                if (patchData == null || patchData.Length == 0)
                    throw new ArgumentException("Patch data cannot be null or empty", nameof(patchData));

                _logger.LogInformation("Applying xdelta3 patch. Original size: {OriginalSize}, Patch size: {PatchSize}",
                    originalData.Length, patchData.Length);

                // Use xdelta3.net to apply the patch
                var patchedData = Xdelta3Lib.Decode(originalData, patchData);

                if (patchedData.Length == 0)
                    throw new InvalidOperationException("Patch operation resulted in empty data");

                _logger.LogInformation("Patch applied successfully. Result size: {ResultSize}", patchedData.Length);

                return patchedData.ToArray();
            }
            catch (ArgumentException)
            {
                throw; // Re-throw argument exceptions as-is
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying xdelta3 patch");
                throw new InvalidOperationException("Failed to apply xdelta3 patch", ex);
            }
        });
    }

    private async Task<bool> ValidatePatchedFileAsync(string filePath, uint expectedCrc32)
    {
        var actualCrc32 = await _fileOperationService.CalculateCrc32Async(filePath);
        return actualCrc32 == expectedCrc32;
    }
}
