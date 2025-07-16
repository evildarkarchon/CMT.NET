# CMT.NET Implementation Plan - Python to C# Port

Reference code is available in the `Code To Port/` directory. This document outlines the plan to port the Collective Modding Toolkit (CMT) from Python/Tkinter to C# using the Avalonia MVVM Framework and ReactiveUI, ensuring feature parity while adhering to C# best practices.

## Overview
Port the Collective Modding Toolkit from Python/Tkinter to C# with Avalonia MVVM Framework and ReactiveUI, maintaining feature parity while following C# best practices.

## Phase 1: Foundation & Architecture ✓
### 1.1 Project Structure Setup
- [ ] Configure CMT.NET.csproj with required dependencies
  - [ ] Add Delta compression library (xdelta3.net)
  - [ ] Add CRC32 calculation (System.IO.Hashing)
  - [ ] Add HTTP client libraries
  - [ ] Add Windows registry access
  - [ ] Add process management libraries
  
### 1.2 Core Architecture
- [ ] Implement dependency injection container
- [ ] Set up logging infrastructure (Serilog recommended)
- [ ] Create base ViewModelBase with ReactiveUI
- [ ] Implement navigation service for tab switching
- [ ] Create application settings service

### 1.3 Data Models
- [ ] Port enums (InstallType, ArchiveVersion, LogType, etc.)
- [ ] Create game info models (ModInfo, ArchiveInfo, F4SEInfo)
- [ ] Implement configuration models
- [ ] Define problem/issue reporting models

## Phase 2: Core Services & Business Logic
### 2.1 Game Detection & Analysis
- [ ] Port CMChecker core functionality
- [ ] Implement registry reading for game paths
- [ ] Create INI file parser service
- [ ] Implement module (plugin) analysis service
- [ ] Create archive (BA2) analysis service

### 2.2 File Operations
- [ ] Implement CRC32 calculation utilities
- [ ] Create file system abstraction layer
- [ ] Port archive header reading logic
- [ ] Implement module version detection

### 2.3 Configuration Management
- [ ] Create settings service with JSON serialization
- [ ] Implement user preferences storage
- [ ] Add MO2/Vortex detection logic

## Phase 3: UI Foundation
### 3.1 Main Window & Navigation
- [ ] Convert MainWindow to MVVM pattern
- [ ] Implement tab control with ViewModels
- [ ] Create custom styles matching original theme
- [ ] Set up window management (size, position persistence)

### 3.2 Common UI Components
- [ ] Create reusable UserControls
  - [ ] Logger control with colored output
  - [ ] Progress indicators
  - [ ] Tool tips system
  - [ ] Separators and layout helpers

### 3.3 Reactive Bindings
- [ ] Set up ReactiveUI command patterns
- [ ] Implement property change notifications
- [ ] Create observable collections for dynamic lists

## Phase 4: Feature Implementation
### 4.1 Overview Tab
- [ ] Create OverviewViewModel
- [ ] Implement game info display
- [ ] Add module count displays with limits
- [ ] Create archive statistics view
- [ ] Implement refresh functionality
- [ ] Add problem detection display

### 4.2 F4SE Tab
- [ ] Create F4SEViewModel
- [ ] Port DLL compatibility checking
- [ ] Implement TreeView for DLL listing
- [ ] Add version compatibility logic
- [ ] Create whitelisting system

### 4.3 Scanner Tab
- [ ] Create ScannerViewModel
- [ ] Implement async scanning engine
- [ ] Port problem detection rules
- [ ] Create results display with grouping
- [ ] Add detail pane for selected issues
- [ ] Implement scan settings panel

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