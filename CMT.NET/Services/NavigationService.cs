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
using CMT.NET.Models;
using CMT.NET.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CMT.NET.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Tab, ViewModelBase> _viewModels = new();
    private Tab _currentTab = Tab.Overview;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public event EventHandler<Tab>? TabChanged;

    public Tab CurrentTab => _currentTab;

    public void NavigateTo(Tab tab)
    {
        if (_currentTab == tab) return;

        _currentTab = tab;
        TabChanged?.Invoke(this, tab);
    }

    public ViewModelBase? GetViewModel(Tab tab)
    {
        if (_viewModels.TryGetValue(tab, out var existing))
            return existing;

        ViewModelBase? viewModel = tab switch
        {
            Tab.Overview => _serviceProvider.GetService<OverviewViewModel>(),
            Tab.F4SE => _serviceProvider.GetService<F4SeViewModel>(),
            Tab.Scanner => _serviceProvider.GetService<ScannerViewModel>(),
            Tab.Tools => _serviceProvider.GetService<ToolsViewModel>(),
            Tab.Settings => _serviceProvider.GetService<SettingsViewModel>(),
            Tab.About => _serviceProvider.GetService<AboutViewModel>(),
            _ => null
        };

        if (viewModel != null)
            _viewModels[tab] = viewModel;

        return viewModel;
    }
}
