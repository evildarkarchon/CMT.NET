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

public class ModuleAnalysisService : IModuleAnalysisService
{
    private readonly ILogger<ModuleAnalysisService> _logger;
    private readonly IFileOperationService _fileOperationService;

    // ESP/ESM file format constants
    private const string ESP_MAGIC = "TES4";
    private const int HEADER_SIZE = 24;
    private const int SUBRECORD_HEADER_SIZE = 6;

    public ModuleAnalysisService(ILogger<ModuleAnalysisService> logger, IFileOperationService fileOperationService)
    {
        _logger = logger;
        _fileOperationService = fileOperationService;
    }

    public async Task<ModInfo?> AnalyzeModuleAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Module file not found: {FilePath}", filePath);
                return null;
            }

            var fileInfo = new System.IO.FileInfo(filePath);
            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            if (extension != ".esp" && extension != ".esm" && extension != ".esl")
            {
                _logger.LogWarning("Invalid module file extension: {Extension}", extension);
                return null;
            }

            var modInfo = new ModInfo
            {
                FilePath = filePath,
                FileName = fileInfo.Name,
                FileSize = fileInfo.Length,
                CreationTime = fileInfo.CreationTime,
                LastWriteTime = fileInfo.LastWriteTime,
                ModuleType = extension switch
                {
                    ".esm" => ModuleType.Master,
                    ".esl" => ModuleType.Light,
                    _ => ModuleType.Plugin
                }
            };

            await AnalyzeModuleHeaderAsync(modInfo);

            // Calculate CRC32 for integrity checking
            modInfo.Crc32 = await _fileOperationService.CalculateCrc32Async(filePath);

            _logger.LogDebug("Successfully analyzed module: {FileName} - Version: {Version}, Records: {RecordCount}",
                modInfo.FileName, modInfo.Version, modInfo.RecordCount);

            return modInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing module: {FilePath}", filePath);
            return null;
        }
    }

    public async Task<List<ModInfo>> AnalyzeModulesInDirectoryAsync(string directoryPath)
    {
        var modules = new List<ModInfo>();

        try
        {
            if (!Directory.Exists(directoryPath))
            {
                _logger.LogWarning("Directory not found: {DirectoryPath}", directoryPath);
                return modules;
            }

            var moduleFiles = Directory.GetFiles(directoryPath, "*.esp", SearchOption.TopDirectoryOnly)
                .Concat(Directory.GetFiles(directoryPath, "*.esm", SearchOption.TopDirectoryOnly))
                .Concat(Directory.GetFiles(directoryPath, "*.esl", SearchOption.TopDirectoryOnly));

            foreach (var file in moduleFiles)
            {
                var modInfo = await AnalyzeModuleAsync(file);
                if (modInfo != null)
                {
                    modules.Add(modInfo);
                }
            }

            _logger.LogInformation("Analyzed {Count} modules in directory: {DirectoryPath}",
                modules.Count, directoryPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing modules in directory: {DirectoryPath}", directoryPath);
        }

        return modules;
    }

    public async Task<bool> IsValidModuleAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return false;

            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            if (extension != ".esp" && extension != ".esm" && extension != ".esl")
                return false;

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var buffer = new byte[4];

            if (await stream.ReadAsync(buffer, 0, 4) != 4)
                return false;

            var magic = Encoding.ASCII.GetString(buffer);
            return magic == "TES4";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating module: {FilePath}", filePath);
            return false;
        }
    }

    public async Task<List<string>> GetMasterFilesAsync(ModInfo modInfo)
    {
        var masterFiles = new List<string>();

        try
        {
            using var stream = new FileStream(modInfo.FilePath, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(stream);

            // Skip to the header record
            stream.Position = 0;

            // Read TES4 header
            var magic = Encoding.ASCII.GetString(reader.ReadBytes(4));
            if (magic != "TES4")
            {
                return masterFiles;
            }

            var dataSize = reader.ReadUInt32();
            var flags = reader.ReadUInt32();
            var formId = reader.ReadUInt32();
            var timestamp = reader.ReadUInt32();
            var version = reader.ReadUInt32();
            var unknown = reader.ReadUInt32();

            // Read subrecords to find master files
            var endPosition = stream.Position + dataSize - 24;

            while (stream.Position < endPosition)
            {
                var subrecordType = Encoding.ASCII.GetString(reader.ReadBytes(4));
                var subrecordSize = reader.ReadUInt16();

                if (subrecordType == "MAST")
                {
                    var masterFileName = Encoding.UTF8.GetString(reader.ReadBytes(subrecordSize));
                    masterFiles.Add(masterFileName.TrimEnd('\0'));
                }
                else
                {
                    // Skip other subrecords
                    stream.Position += subrecordSize;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting master files for module: {FilePath}", modInfo.FilePath);
        }

        return masterFiles;
    }

    public bool IsLightModule(ModInfo modInfo)
    {
        return modInfo.ModuleType == ModuleType.Light ||
               (modInfo.Flags.HasValue && (modInfo.Flags.Value & 0x200) != 0);
    }

    public bool IsMasterModule(ModInfo modInfo)
    {
        return modInfo.ModuleType == ModuleType.Master ||
               (modInfo.Flags.HasValue && (modInfo.Flags.Value & 0x01) != 0);
    }

    private async Task AnalyzeModuleHeaderAsync(ModInfo modInfo)
    {
        using var stream = new FileStream(modInfo.FilePath, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(stream);

        // Read TES4 header
        var magic = Encoding.ASCII.GetString(reader.ReadBytes(4));
        if (magic != "TES4")
        {
            throw new InvalidDataException($"Invalid module magic: {magic}");
        }

        var dataSize = reader.ReadUInt32();
        var flags = reader.ReadUInt32();
        var formId = reader.ReadUInt32();
        var timestamp = reader.ReadUInt32();
        var version = reader.ReadUInt32();
        var unknown = reader.ReadUInt32();

        modInfo.Flags = flags;
        modInfo.FormId = formId;
        modInfo.Version = version;

        // Read subrecords to get more details
        var endPosition = stream.Position + dataSize - 24;

        while (stream.Position < endPosition)
        {
            var subrecordType = Encoding.ASCII.GetString(reader.ReadBytes(4));
            var subrecordSize = reader.ReadUInt16();

            switch (subrecordType)
            {
                case "HEDR":
                    if (subrecordSize >= 12)
                    {
                        var headerVersion = reader.ReadSingle();
                        var recordCount = reader.ReadUInt32();
                        var nextObjectId = reader.ReadUInt32();

                        modInfo.HeaderVersion = headerVersion;
                        modInfo.RecordCount = recordCount;
                        modInfo.NextObjectId = nextObjectId;
                    }

                    break;

                case "CNAM":
                    var author = Encoding.UTF8.GetString(reader.ReadBytes(subrecordSize));
                    modInfo.Author = author.TrimEnd('\0');
                    break;

                case "SNAM":
                    var description = Encoding.UTF8.GetString(reader.ReadBytes(subrecordSize));
                    modInfo.Description = description.TrimEnd('\0');
                    break;

                default:
                    // Skip unknown subrecords
                    stream.Position += subrecordSize;
                    break;
            }
        }
    }
}
