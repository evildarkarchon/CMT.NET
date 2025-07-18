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

using Avalonia.Controls;
using Avalonia.Interactivity;
using CMT.NET.ViewModels;

namespace CMT.NET.Views;

public partial class ArchivePatcherWindow : Window
{
    public ArchivePatcherWindow()
    {
        InitializeComponent();
    }

    public ArchivePatcherWindow(ArchivePatcherViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
