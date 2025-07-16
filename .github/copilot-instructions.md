# Copilot Instructions for CMT.NET

## Project Overview
This is a C# port of the Collective Modding Toolkit (CMT) from Python/Tkinter to Avalonia MVVM with ReactiveUI. The toolkit troubleshoots and optimizes Fallout 4 mod setups, providing game version detection, archive patching, F4SE DLL scanning, and mod configuration analysis.

## Architecture Patterns

### MVVM with ReactiveUI
- All ViewModels inherit from `ViewModelBase : ReactiveObject`
- Use ReactiveUI patterns for property binding and command handling
- Views are AXAML files with code-behind for Avalonia UI
- Main entry point: `Program.cs` → `App.axaml.cs` → `MainWindow`

### Project Structure
```
CMT.NET/
├── Models/          # Data models (game info, mod info, archive info)
├── Services/        # Business logic (game detection, file operations)
├── ViewModels/      # MVVM ViewModels with ReactiveUI
├── Views/           # AXAML UI files
└── Assets/          # Resources (icons, fonts)
```

### Key Dependencies
- **Avalonia 11.3.2**: Cross-platform UI framework
- **ReactiveUI + Fody**: MVVM with property weaving
- **Microsoft.Extensions.DependencyInjection**: Service container
- **Serilog**: Logging framework
- **xdelta3.net**: Delta compression for game patching
- **System.IO.Hashing**: CRC32 calculations for file integrity

## Core Domain Concepts

### Game Analysis Pipeline
1. **Game Detection**: Registry reading (`Microsoft.Win32.Registry`) to find Fallout 4 installations
2. **Module Analysis**: ESP/ESM plugin scanning with version detection
3. **Archive Processing**: BA2 file header analysis (BTDX, GNRL, DX10 formats)
4. **F4SE Scanning**: DLL version compatibility checking

### File Operations
- CRC32 checksums for integrity verification
- Archive header reading (reference: `Code to Port/src/patcher/_base.py`)
- Delta patching for game version upgrades/downgrades
- INI file parsing for mod manager configurations

### Configuration Management
- JSON-based settings storage (`Microsoft.Extensions.Configuration.Json`)
- User preferences for mod manager paths (MO2/Vortex detection)
- Log level configuration with Serilog integration

## Development Workflow

### Building & Testing
```powershell
# Build solution
dotnet build CMT.NET.sln

# Run tests
dotnet test CMT.NET.Tests/

# Run application
dotnet run --project CMT.NET/
```

### Key Files to Reference
- `Code to Port/src/`: Python source code being ported
- `cmt-port-plan.md`: Detailed implementation roadmap
- `CMT.NET.csproj`: Dependency configuration
- `ViewModels/ViewModelBase.cs`: Base class for all ViewModels

## Code Conventions

### C# Specific Patterns
- Use `nullable` reference types (enabled in project)
- Follow async/await patterns for file operations
- Implement `IDisposable` for resource management
- Use `ConfigureAwait(false)` for library code

### ReactiveUI Patterns
- Properties: `[Reactive] public string Property { get; set; }`
- Commands: `ReactiveCommand<TParam, TResult>`
- Validation: Use `ReactiveUI.Validation` for form validation

### Error Handling
- Use Serilog for structured logging
- Implement proper exception handling in service layers
- Validate file operations before processing

## Integration Points

### Windows Registry
- Game path detection via Steam/Epic Games registry keys
- MOD manager installation detection
- System architecture detection (x86/x64)

### File System Operations
- Archive file parsing (BA2 format specifics in Python source)
- Module file analysis (ESP/ESM headers)
- Delta patch application for version switching

### External Tools
- F4SE DLL compatibility checking
- MOD manager integration (MO2/Vortex)
- Archive creation/modification tools

## Testing Strategy
- Unit tests in `CMT.NET.Tests/` using xUnit
- Use Moq for service mocking
- FluentAssertions for readable test assertions
- Focus on testing business logic in Services layer

## Common Tasks
- Adding new tabs: Create ViewModel + View pair, register in DI container
- File operations: Use async patterns, implement proper error handling
- Game detection: Reference Python `cm_checker.py` for logic
- UI updates: Use ReactiveUI property binding, avoid direct UI manipulation
