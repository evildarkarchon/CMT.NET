# CMT.NET Port Progress Checklist

## Quick Reference - Priority Order

### 🔧 Phase 1: Foundation (Week 1-2)
- [ ] Add NuGet packages to CMT.NET.csproj
- [ ] Create folder structure (Models, Views, ViewModels, Services, Converters)
- [ ] Set up DI container in App.axaml.cs
- [ ] Configure Serilog logging
- [ ] Create ViewModelBase with INotifyPropertyChanged
- [ ] Port all enums from Python enums.py
- [ ] Create GlobalConstants.cs from globals.py

### 📊 Phase 2: Core Services (Week 2-3)
- [ ] Create IGameDetectionService
- [ ] Create IFileService with async methods
- [ ] Create ISettingsService for JSON config
- [ ] Create IRegistryService for game paths
- [ ] Port INI parser from Python
- [ ] Implement CRC32 utilities
- [ ] Create ModuleInfo analyzer

### 🎨 Phase 3: UI Foundation (Week 3-4)
- [ ] Convert MainWindow to proper MVVM
- [ ] Create TabControlViewModel
- [ ] Port color scheme and styles
- [ ] Create LoggerControl UserControl
- [ ] Implement tooltip system
- [ ] Set up window size/position persistence

### 📋 Phase 4: Feature Tabs (Week 4-6)
#### Overview Tab
- [ ] Create OverviewViewModel
- [ ] Port module counting logic
- [ ] Port archive analysis
- [ ] Implement refresh command
- [ ] Add problem detection

#### F4SE Tab
- [ ] Create F4SEViewModel
- [ ] Port DLL scanning logic
- [ ] Implement TreeView binding
- [ ] Add whitelist system

#### Scanner Tab
- [ ] Create ScannerViewModel
- [ ] Port scanning engine
- [ ] Implement async scanning
- [ ] Create problem grouping
- [ ] Add details pane

### 🛠️ Phase 5: Tools (Week 6-8)
#### Downgrader
- [ ] Create DowngraderWindow
- [ ] Port version detection
- [ ] Implement delta patching
- [ ] Add download manager
- [ ] Create progress reporting

#### Archive Patcher
- [ ] Create ArchivePatcherWindow
- [ ] Port BA2 conversion logic
- [ ] Add batch processing
- [ ] Implement filtering

### ✅ Phase 6: Testing (Week 8-9)
- [ ] Set up xUnit project
- [ ] Create service tests
- [ ] Add ViewModel tests
- [ ] Test file operations
- [ ] Integration testing

### 🚀 Phase 7: Polish (Week 9-10)
- [ ] Performance profiling
- [ ] Memory optimization
- [ ] Add keyboard shortcuts
- [ ] Implement update checker
- [ ] Final bug fixes

## AI Assistant Prompts

### For Model Conversion:
"Convert this Python class to a C# record with proper nullable annotations and init-only properties"

### For Service Implementation:
"Create an async C# service interface and implementation for [functionality] following SOLID principles"

### For ViewModel Creation:
"Create a ReactiveUI ViewModel for [feature] with reactive commands and observable properties"

### For UI Conversion:
"Convert this Tkinter layout to Avalonia XAML using Grid/StackPanel with proper data binding"

### For Test Generation:
"Generate xUnit tests for [class] with proper mocking using Moq"

## Key File Mappings

| Python File | C# Equivalent |
|------------|---------------|
| cm_checker.py | Services/GameDetectionService.cs |
| globals.py | Constants/GlobalConstants.cs |
| enums.py | Models/Enums.cs |
| utils.py | Utilities/FileUtilities.cs |
| tabs/_overview.py | ViewModels/OverviewViewModel.cs |
| tabs/_scanner.py | ViewModels/ScannerViewModel.cs |
| downgrader.py | ViewModels/Tools/DowngraderViewModel.cs |
| patcher/_archives.py | Services/ArchivePatcherService.cs |

## Development Environment Setup
1. Install .NET 8.0 SDK
2. Install Avalonia VS Code extension
3. Set up Avalonia previewer
4. Configure code formatting (`.editorconfig`)
5. Set up git hooks for tests

## Daily Progress Tracking
- [ ] Morning: Review Python code section
- [ ] Convert to C# following patterns
- [ ] Write unit tests immediately
- [ ] Test UI changes in previewer
- [ ] Commit with descriptive messages
- [ ] Update this checklist

## Success Metrics
- ✅ All Python features ported
- ✅ No regression in functionality  
- ✅ Improved performance (target: 2x faster scanning)
- ✅ Test coverage > 80%
- ✅ Memory usage < Python version
- ✅ Native Windows integration