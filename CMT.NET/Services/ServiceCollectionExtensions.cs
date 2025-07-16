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
using CMT.NET.Models;
using CMT.NET.Services;
using CMT.NET.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

namespace CMT.NET;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCMTServices(this IServiceCollection services)
    {
        // Configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        services.AddSingleton<IConfiguration>(configuration);

        // Settings
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
        services.AddSingleton<ISettingsService, SettingsService>();

        // Core Services
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IGameDetectionService, GameDetectionService>();
        services.AddSingleton<IFileOperationService, FileOperationService>();
        services.AddSingleton<IIniFileService, IniFileService>();
        services.AddSingleton<IModuleAnalysisService, ModuleAnalysisService>();
        services.AddSingleton<IArchiveAnalysisService, ArchiveAnalysisService>();
        services.AddSingleton<ICmCheckerService, CMCheckerService>();

        // Logging
        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
            .WriteTo.Console(
                outputTemplate:
                "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                Path.Combine("logs", "cmt-.txt"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 10,
                outputTemplate:
                "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            );

        Log.Logger = loggerConfig.CreateLogger();
        services.AddSingleton<ILoggerFactory>(new SerilogLoggerFactory(Log.Logger));
        services.AddLogging();

        // Core Services
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IGameDetectionService, GameDetectionService>();
        services.AddSingleton<IFileOperationService, FileOperationService>();
        services.AddSingleton<IIniFileService, IniFileService>();
        services.AddSingleton<IModuleAnalysisService, ModuleAnalysisService>();
        services.AddSingleton<IArchiveAnalysisService, ArchiveAnalysisService>();
        services.AddSingleton<ICmCheckerService, CMCheckerService>();

        // ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<OverviewViewModel>();
        services.AddTransient<F4SeViewModel>();
        services.AddTransient<ScannerViewModel>();
        services.AddTransient<ToolsViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<AboutViewModel>();

        // HTTP Client
        services.AddHttpClient();

        return services;
    }
}
