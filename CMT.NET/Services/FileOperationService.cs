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
using System.IO.Hashing;
using System.Threading.Tasks;
using CMT.NET.Models;
using Microsoft.Extensions.Logging;

namespace CMT.NET.Services;

public class FileOperationService : IFileOperationService
{
    private readonly ILogger<FileOperationService> _logger;

    public FileOperationService(ILogger<FileOperationService> logger)
    {
        _logger = logger;
    }

    public async Task<uint> CalculateCrc32Async(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var crc32 = new Crc32();

            var buffer = new byte[8192];
            int bytesRead;

            while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                crc32.Append(buffer.AsSpan(0, bytesRead));
            }

            return BitConverter.ToUInt32(crc32.GetHashAndReset());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating CRC32 for file: {FilePath}", filePath);
            throw;
        }
    }

    public async Task<bool> ValidateFileIntegrityAsync(string filePath, uint expectedCrc32)
    {
        try
        {
            var actualCrc32 = await CalculateCrc32Async(filePath);
            return actualCrc32 == expectedCrc32;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating file integrity: {FilePath}", filePath);
            return false;
        }
    }

    public async Task<Models.FileInfo?> GetFileInfoAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            var fileInfo = new System.IO.FileInfo(filePath);
            var crc32 = await CalculateCrc32Async(filePath);

            return new Models.FileInfo
            {
                FilePath = filePath,
                FileName = fileInfo.Name,
                FileSize = fileInfo.Length,
                CreationTime = fileInfo.CreationTime,
                LastWriteTime = fileInfo.LastWriteTime,
                Crc32 = crc32
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file info: {FilePath}", filePath);
            return null;
        }
    }

    public async Task<string?> GetFileVersionAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(filePath);
            return versionInfo.FileVersion;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file version: {FilePath}", filePath);
            return null;
        }
    }

    public bool IsValidGameDirectory(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                return false;
            }

            var fallout4Exe = Path.Combine(path, "Fallout4.exe");
            var dataDir = Path.Combine(path, "Data");

            return File.Exists(fallout4Exe) && Directory.Exists(dataDir);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating game directory: {Path}", path);
            return false;
        }
    }

    public async Task<bool> BackupFileAsync(string sourceFilePath, string backupDirectory)
    {
        try
        {
            if (!File.Exists(sourceFilePath))
            {
                return false;
            }

            if (!Directory.Exists(backupDirectory))
            {
                Directory.CreateDirectory(backupDirectory);
            }

            var fileName = Path.GetFileName(sourceFilePath);
            var backupFilePath = Path.Combine(backupDirectory, $"{fileName}.backup");

            using var sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read);
            using var backupStream = new FileStream(backupFilePath, FileMode.Create, FileAccess.Write);

            await sourceStream.CopyToAsync(backupStream);

            _logger.LogInformation("File backed up successfully: {SourceFile} -> {BackupFile}",
                sourceFilePath, backupFilePath);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error backing up file: {SourceFile}", sourceFilePath);
            return false;
        }
    }
}
