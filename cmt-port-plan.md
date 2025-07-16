# CMT.NET Implementation Plan - Python to C# Port

Reference code is available in the `Code To Port/` directory. This document outlines the plan to port the Collective Modding Toolkit (CMT) from Python/Tkinter to C# using the Avalonia MVVM Framework and ReactiveUI, ensuring feature parity while adhering to C# best practices.

## Overview
Port the Collective Modding Toolkit from Python/Tkinter to C# with Avalonia MVVM Framework and ReactiveUI, maintaining feature parity while following C# best practices.

## Phase 1: Foundation & Architecture ✓
### 1.1 Project Structure Setup
- [x] Configure CMT.NET.csproj with required dependencies
  - [x] Add Delta compression library (xdelta3.net)
  - [x] Add CRC32 calculation (System.IO.Hashing)
  - [x] Add HTTP client libraries
  - [x] Add Windows registry access
  - [x] Add process management libraries
  
### 1.2 Core Architecture
- [x] Implement dependency injection container
- [x] Set up logging infrastructure (Serilog recommended)
- [x] Create base ViewModelBase with ReactiveUI
- [x] Implement navigation service for tab switching
- [x] Create application settings service

### 1.3 Data Models
- [x] Port enums (InstallType, ArchiveVersion, LogType, etc.)
- [x] Create game info models (ModInfo, ArchiveInfo, F4SEInfo)
- [x] Implement configuration models
- [x] Define problem/issue reporting models

## Phase 2: Core Services & Business Logic ✓
### 2.1 Game Detection & Analysis
- [x] Port CMChecker core functionality
- [x] Implement registry reading for game paths
- [x] Create INI file parser service
- [x] Implement module (plugin) analysis service
- [x] Create archive (BA2) analysis service

### 2.2 File Operations
- [x] Implement CRC32 calculation utilities
- [x] Create file system abstraction layer
- [x] Port archive header reading logic
- [x] Implement module version detection

### 2.3 Configuration Management
- [x] Create settings service with JSON serialization
- [x] Implement user preferences storage
- [x] Add MO2/Vortex detection logic (basic structure)

## Phase 3: UI Foundation ✓
### 3.1 Main Window & Navigation
- [x] Convert MainWindow to MVVM pattern
- [x] Implement tab control with ViewModels
- [x] Create custom styles matching original theme
- [x] Set up window management (size, position persistence)

### 3.2 Common UI Components
- [x] Create reusable UserControls
  - [x] Logger control with colored output
  - [x] Progress indicators
  - [x] Tool tips system
  - [x] Separators and layout helpers

### 3.3 Reactive Bindings
- [x] Set up ReactiveUI command patterns
- [x] Implement property change notifications
- [x] Create observable collections for dynamic lists

## Phase 4: Feature Implementation ✅
### 4.1 Overview Tab
- [x] Create OverviewViewModel
- [x] Implement game info display
- [x] Add module count displays with limits
- [x] Create archive statistics view
- [x] Implement refresh functionality
- [x] Add problem detection display
- [x] Add system information display
- [x] Add mod manager detection framework

### 4.2 F4SE Tab
- [x] Create F4SEViewModel
- [x] Port DLL compatibility checking
- [x] Implement TreeView for DLL listing
- [x] Add version compatibility logic
- [x] Create whitelisting system
- [x] Add plugin selection and details
- [x] Implement compatibility statistics

### 4.3 Scanner Tab
- [x] Create ScannerViewModel
- [x] Implement async scanning engine
- [x] Port problem detection rules
- [x] Create results display with grouping
- [x] Add detail pane for selected issues
- [x] Implement scan settings panel
- [x] Add progress tracking with percentage
- [x] Add problem categorization (Error/Warning/Info)

## Phase 5: Tools Implementation
### 5.1 Downgrader Tool
- [ ] Create DowngraderViewModel
- [ ] Port delta patching functionality (using xdelta3.net)
- [ ] Implement file version detection
- [ ] Add download management with progress
- [ ] Create backup/restore system
- [ ] Implement modal dialog pattern

### 5.2 Archive Patcher Tool
- [ ] Create ArchivePatcherViewModel
- [ ] Port BA2 version conversion logic
- [ ] Implement batch processing
- [ ] Add filtering capabilities
- [ ] Create progress reporting

### 5.3 Tools Integration
- [ ] Implement tool launcher service
- [ ] Add external tool configurations
- [ ] Create about dialogs for tools

## Phase 6: Testing & Polish
### 6.1 Unit Testing
- [ ] Set up xUnit test project
- [ ] Create tests for core services
- [ ] Test file operations
- [ ] Validate game detection logic
- [ ] Test configuration management

### 6.2 Integration Testing
- [ ] Test with various game configurations
- [ ] Validate MO2/Vortex integration
- [ ] Test error handling scenarios
- [ ] Performance testing with large mod lists

### 6.3 UI Testing
- [ ] Implement view model tests
- [ ] Test reactive bindings
- [ ] Validate async UI operations
- [ ] Test modal dialog workflows

## Phase 7: Advanced Features & Optimization
### 7.1 Performance Optimization
- [ ] Implement lazy loading for large lists
- [ ] Add caching for repeated operations
- [ ] Optimize file scanning algorithms
- [ ] Profile and optimize memory usage

### 7.2 Enhanced Features
- [ ] Add multi-threading where beneficial
- [ ] Implement advanced filtering options
- [ ] Add export functionality for scan results
- [ ] Create update checking system

### 7.3 Accessibility
- [ ] Add keyboard navigation support
- [ ] Implement screen reader compatibility
- [ ] Ensure proper contrast ratios
- [ ] Add customizable font sizes

## Technical Considerations

### Key Libraries to Add
```xml
<PackageReference Include="Avalonia.ReactiveUI" Version="11.3.2" />
<PackageReference Include="ReactiveUI.Fody" Version="19.5.31" />
<PackageReference Include="Serilog" Version="4.2.0" />
<PackageReference Include="System.IO.Hashing" Version="9.0.0" />
<PackageReference Include="SharpCompress" Version="0.38.0" />
<PackageReference Include="Microsoft.Win32.Registry" Version="6.0.0" />
```

### Architecture Patterns
1. **MVVM with ReactiveUI**: Strict separation of concerns
2. **Dependency Injection**: Use Microsoft.Extensions.DependencyInjection
3. **Repository Pattern**: For data access abstraction
4. **Command Pattern**: For all user actions
5. **Observer Pattern**: For reactive updates

### C# Best Practices to Follow
1. Use nullable reference types throughout
2. Implement IDisposable where appropriate
3. Use async/await for all I/O operations
4. Apply SOLID principles
5. Use records for immutable data
6. Implement proper exception handling
7. Use LINQ for collection operations
8. Apply defensive programming techniques

### Testing Strategy
1. Unit tests for all services (>80% coverage)
2. Integration tests for file operations
3. UI tests for critical workflows
4. Performance benchmarks
5. Memory leak detection

## Checkpoints & Milestones
- **Milestone 1**: Core architecture complete, basic navigation working
- **Milestone 2**: Overview and F4SE tabs functional
- **Milestone 3**: Scanner fully implemented
- **Milestone 4**: All tools ported and working
- **Milestone 5**: Full test coverage, ready for beta

## AI Tool Assistance Guidelines
When working with AI tools:
1. Provide Python source code for accurate conversion
2. Request C# idiomatic implementations
3. Ask for ReactiveUI-specific patterns
4. Verify Avalonia control usage
5. Request unit test generation
6. Ask for performance optimization suggestions

## Notes on Specific Conversions
- Replace Tkinter grid system with Avalonia Grid/StackPanel
- Convert tkinter.messagebox to Avalonia MessageBox
- Replace threading.Thread with Task.Run
- Convert queue.Queue to Channel<T> or BlockingCollection<T>
- Replace Pillow image handling with Avalonia.Media.Imaging