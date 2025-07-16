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

namespace CMT.NET.Models;

public static class Tool
{
    public static readonly string[] xEdit = { "xedit.exe", "fo4edit.exe" };
    public static readonly string[] BSArch = { "bsarch.exe" };
    public static readonly string[] ComplexSorter = { "complex sorter (32bit).bat", "complex sorter.bat" };
}

public enum CSIDL
{
    Desktop = 0,
    Documents = 5,
    AppData = 26,
    AppDataLocal = 28
}

public enum InstallType
{
    OG, // Old-Gen
    DG, // Down-Grade
    NG, // Next-Gen
    Unknown,
    NotFound
}

public static class Magic
{
    public static readonly byte[] BTDX = { 0x42, 0x54, 0x44, 0x58 }; // "BTDX"
    public static readonly byte[] GNRL = { 0x47, 0x4E, 0x52, 0x4C }; // "GNRL"
    public static readonly byte[] DX10 = { 0x44, 0x58, 0x31, 0x30 }; // "DX10"
    public static readonly byte[] TES4 = { 0x54, 0x45, 0x53, 0x34 }; // "TES4"
    public static readonly byte[] HEDR = { 0x48, 0x45, 0x44, 0x52 }; // "HEDR"
    public static readonly byte[] DDS = { 0x44, 0x44, 0x53, 0x20 }; // "DDS "
}

public enum Tab
{
    Overview,
    F4SE,
    Scanner,
    Tools,
    Settings,
    About
}

public enum LogType
{
    Info,
    Good,
    Bad
}

public enum ArchiveVersion
{
    OG = 1,
    NG7 = 7,
    NG = 8
}

[Flags]
public enum ModuleFlag
{
    Light = 0x0200
}

public enum ProblemType
{
    JunkFile,
    UnexpectedFormat,
    MisplacedDLL,
    LoosePrevis,
    AnimTextDataFolder,
    InvalidArchive,
    InvalidModule,
    InvalidArchiveName,
    F4SEOverride,
    FileNotFound,
    WrongVersion,
    ComplexSorter
}

public enum SolutionType
{
    ArchiveOrDeleteFile,
    ArchiveOrDeleteFolder,
    DeleteFile,
    ConvertDeleteOrIgnoreFile,
    DeleteOrIgnoreFile,
    DeleteOrIgnoreFolder,
    RenameArchive,
    DownloadMod,
    VerifyFiles,
    UnknownFormat,
    ComplexSorterFix
}

public enum Language
{
    Chinese, // "cn"
    German, // "de"
    English, // "en"
    Spanish, // "es"
    SpanishLatinAmerica, // "esmx"
    French, // "fr"
    Italian, // "it"
    Japanese, // "ja"
    Polish, // "pl"
    BrazilianPortuguese, // "ptbr"
    Russian // "ru"
}
