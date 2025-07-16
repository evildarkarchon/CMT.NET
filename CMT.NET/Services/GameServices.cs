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
using System.IO;
using System.Threading.Tasks;
using CMT.NET.Models;
using Microsoft.Extensions.Logging;
using System.IO.Hashing;

namespace CMT.NET.Services;

public class FileOperationService : IFileOperationService
{
    private readonly ILogger<FileOperationService> _logger;

    public FileOperationService(ILogger<FileOperationService> logger)
    {
        _logger = logger;
    }

    public async Task<string> CalculateCrc32Async(string filePath)
    {
        try
        {
            using var stream = File.OpenRead(filePath);
            var buffer = new byte[stream.Length];
            await stream.ReadExactlyAsync(buffer, 0, buffer.Length);
            var hash = Crc32.Hash(buffer);
            return Convert.ToHexString(hash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating CRC32 for {FilePath}", filePath);
            throw;
        }
    }

    public async Task<bool> ValidateFileIntegrityAsync(string filePath, string expectedHash)
    {
        try
        {
            var actualHash = await CalculateCrc32Async(filePath);
            return string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating file integrity for {FilePath}", filePath);
            return false;
        }
    }

    public async Task<Models.FileInfo> GetFileInfoAsync(string filePath)
    {
        try
        {
            var fileInfo = new System.IO.FileInfo(filePath);
            var hash = await CalculateCrc32Async(filePath);

            return new Models.FileInfo(
                fileInfo.Name,
                "Unknown", // TODO: Extract version from file
                fileInfo.CreationTime,
                fileInfo.LastWriteTime,
                fileInfo.Length,
                hash
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file info for {FilePath}", filePath);
            throw;
        }
    }

    public async Task<byte[]> ReadFileHeaderAsync(string filePath, int length)
    {
        try
        {
            using var stream = File.OpenRead(filePath);
            var buffer = new byte[length];
            await stream.ReadExactlyAsync(buffer, 0, length);
            return buffer;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading file header for {FilePath}", filePath);
            throw;
        }
    }
}

// Stub implementations for other services
public class ArchiveService : IArchiveService
{
    private readonly ILogger<ArchiveService> _logger;

    public ArchiveService(ILogger<ArchiveService> logger)
    {
        _logger = logger;
    }

    public Task<ArchiveInfo[]> ScanArchivesAsync(string dataPath) => Task.FromResult(Array.Empty<ArchiveInfo>());
    public Task<ArchiveVersion> GetArchiveVersionAsync(string archivePath) => Task.FromResult(ArchiveVersion.OG);
    public Task<bool> ValidateArchiveAsync(string archivePath) => Task.FromResult(true);
    public Task ConvertArchiveAsync(string archivePath, ArchiveVersion targetVersion) => Task.CompletedTask;
}

public class ModuleService : IModuleService
{
    private readonly ILogger<ModuleService> _logger;

    public ModuleService(ILogger<ModuleService> logger)
    {
        _logger = logger;
    }

    public Task<ModInfo[]> ScanModulesAsync(string dataPath) => Task.FromResult(Array.Empty<ModInfo>());
    public Task<float> GetModuleVersionAsync(string modulePath) => Task.FromResult(1.0f);
    public Task<bool> ValidateModuleAsync(string modulePath) => Task.FromResult(true);
    public Task<bool> IsModuleEnabledAsync(string modulePath) => Task.FromResult(true);
}

public class F4SeService : IF4SEService
{
    private readonly ILogger<F4SeService> _logger;

    public F4SeService(ILogger<F4SeService> logger)
    {
        _logger = logger;
    }

    public Task<F4SEInfo[]> ScanF4SEPluginsAsync(string f4SePath) => Task.FromResult(Array.Empty<F4SEInfo>());
    public Task<bool> ValidateF4SECompatibilityAsync(string pluginPath) => Task.FromResult(true);
    public Task<string> GetF4SEVersionAsync(string f4SePath) => Task.FromResult("Unknown");
    public Task<bool> IsPluginWhitelistedAsync(string pluginPath) => Task.FromResult(false);
}

public class ScannerService : IScannerService
{
    private readonly ILogger<ScannerService> _logger;

    public ScannerService(ILogger<ScannerService> logger)
    {
        _logger = logger;
    }

    public Task<ProblemInfo[]> ScanForProblemsAsync(GameInfo gameInfo) => Task.FromResult(Array.Empty<ProblemInfo>());
    public Task<ProblemInfo[]> ScanSpecificPathAsync(string path) => Task.FromResult(Array.Empty<ProblemInfo>());
    public Task<bool> CanFixProblemAsync(ProblemInfo problem) => Task.FromResult(false);
    public Task<bool> FixProblemAsync(ProblemInfo problem) => Task.FromResult(false);
}
