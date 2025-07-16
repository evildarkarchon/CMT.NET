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
using System.Text;
using System.Threading.Tasks;
using CMT.NET.Models;
using Microsoft.Extensions.Logging;

namespace CMT.NET.Services;

public class ArchiveAnalysisService : IArchiveAnalysisService
{
    private readonly ILogger<ArchiveAnalysisService> _logger;
    private readonly IFileOperationService _fileOperationService;

    // BA2 file format constants
    private const string BA2_MAGIC = "BTDX";
    private const int BA2_HEADER_SIZE = 36;
    private const int BA2_FILE_ENTRY_SIZE = 36;

    public ArchiveAnalysisService(ILogger<ArchiveAnalysisService> logger, IFileOperationService fileOperationService)
    {
        _logger = logger;
        _fileOperationService = fileOperationService;
    }

    public async Task<ArchiveInfo?> AnalyzeArchiveAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Archive file not found: {FilePath}", filePath);
                return null;
            }

            var fileInfo = new System.IO.FileInfo(filePath);
            var archiveInfo = new ArchiveInfo
            {
                FilePath = filePath,
                FileName = fileInfo.Name,
                FileSize = fileInfo.Length,
                CreationTime = fileInfo.CreationTime,
                LastWriteTime = fileInfo.LastWriteTime
            };

            // Determine archive type by extension
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            switch (extension)
            {
                case ".ba2":
                    await AnalyzeBa2ArchiveAsync(archiveInfo);
                    break;
                case ".bsa":
                    await AnalyzeBsaArchiveAsync(archiveInfo);
                    break;
                default:
                    _logger.LogWarning("Unknown archive type: {Extension}", extension);
                    return null;
            }

            _logger.LogDebug("Successfully analyzed archive: {FileName} - Version: {Version}, Files: {FileCount}",
                archiveInfo.FileName, archiveInfo.Version, archiveInfo.FileCount);

            return archiveInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing archive: {FilePath}", filePath);
            return null;
        }
    }

    public async Task<List<ArchiveInfo>> AnalyzeArchivesInDirectoryAsync(string directoryPath)
    {
        var archives = new List<ArchiveInfo>();

        try
        {
            if (!Directory.Exists(directoryPath))
            {
                _logger.LogWarning("Directory not found: {DirectoryPath}", directoryPath);
                return archives;
            }

            var archiveFiles = Directory.GetFiles(directoryPath, "*.ba2", SearchOption.TopDirectoryOnly)
                .Concat(Directory.GetFiles(directoryPath, "*.bsa", SearchOption.TopDirectoryOnly));

            foreach (var file in archiveFiles)
            {
                var archiveInfo = await AnalyzeArchiveAsync(file);
                if (archiveInfo != null)
                {
                    archives.Add(archiveInfo);
                }
            }

            _logger.LogInformation("Analyzed {Count} archives in directory: {DirectoryPath}",
                archives.Count, directoryPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing archives in directory: {DirectoryPath}", directoryPath);
        }

        return archives;
    }

    public async Task<bool> IsValidArchiveAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return false;

            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            if (extension != ".ba2" && extension != ".bsa")
                return false;

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var buffer = new byte[4];

            if (await stream.ReadAsync(buffer, 0, 4) != 4)
                return false;

            var magic = Encoding.ASCII.GetString(buffer);
            return magic == "BTDX" || magic == "BSA\0";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating archive: {FilePath}", filePath);
            return false;
        }
    }

    public ArchiveVersion GetArchiveVersion(ArchiveInfo archiveInfo)
    {
        if (archiveInfo.Version == null)
            return ArchiveVersion.Unknown;

        // BA2 version mapping based on Python source
        return archiveInfo.Version switch
        {
            1 => ArchiveVersion.BA2_GNRL,
            2 => ArchiveVersion.BA2_GNRL,
            3 => ArchiveVersion.BA2_DX10,
            7 => ArchiveVersion.BA2_GNMF,
            _ => ArchiveVersion.Unknown
        };
    }

    private async Task AnalyzeBa2ArchiveAsync(ArchiveInfo archiveInfo)
    {
        using var stream = new FileStream(archiveInfo.FilePath, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(stream);

        // Read BA2 header
        var magic = Encoding.ASCII.GetString(reader.ReadBytes(4));
        if (magic != "BTDX")
        {
            throw new InvalidDataException($"Invalid BA2 magic: {magic}");
        }

        var version = reader.ReadUInt32();
        var type = Encoding.ASCII.GetString(reader.ReadBytes(4));
        var fileCount = reader.ReadUInt32();
        var nameTableOffset = reader.ReadUInt64();

        archiveInfo.Version = (int)version;
        archiveInfo.FileCount = (int)fileCount;
        archiveInfo.ArchiveType = type;

        // Determine archive format based on type
        archiveInfo.Format = type switch
        {
            "GNRL" => "General",
            "DX10" => "Texture",
            "GNMF" => "PS4",
            _ => "Unknown"
        };

        // Calculate CRC32 for integrity checking
        stream.Position = 0;
        var buffer = new byte[8192];
        var crc32 = new System.IO.Hashing.Crc32();

        int bytesRead;
        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            crc32.Append(buffer.AsSpan(0, bytesRead));
        }

        archiveInfo.Crc32 = BitConverter.ToUInt32(crc32.GetHashAndReset());
    }

    private async Task AnalyzeBsaArchiveAsync(ArchiveInfo archiveInfo)
    {
        using var stream = new FileStream(archiveInfo.FilePath, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(stream);

        // Read BSA header
        var magic = Encoding.ASCII.GetString(reader.ReadBytes(4));
        if (magic != "BSA\0")
        {
            throw new InvalidDataException($"Invalid BSA magic: {magic}");
        }

        var version = reader.ReadUInt32();
        var folderRecordOffset = reader.ReadUInt32();
        var archiveFlags = reader.ReadUInt32();
        var folderCount = reader.ReadUInt32();
        var fileCount = reader.ReadUInt32();

        archiveInfo.Version = (int)version;
        archiveInfo.FileCount = (int)fileCount;
        archiveInfo.ArchiveType = "BSA";
        archiveInfo.Format = "Legacy";

        // Calculate CRC32 for integrity checking
        stream.Position = 0;
        var buffer = new byte[8192];
        var crc32 = new System.IO.Hashing.Crc32();

        int bytesRead;
        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            crc32.Append(buffer.AsSpan(0, bytesRead));
        }

        archiveInfo.Crc32 = BitConverter.ToUInt32(crc32.GetHashAndReset());
    }
}
