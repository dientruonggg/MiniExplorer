# MiniExplorer - Quick Reference

## 🚀 Quick Commands

```bash
# Build
dotnet build

# Run
dotnet run --project MiniExplorer.UI/MiniExplorer.UI.csproj

# Test
dotnet test

# Quick start (uses script)
./start.sh

# Clean
dotnet clean

# Restore packages
dotnet restore
```

## 📁 File Locations

| Component | Path |
|-----------|------|
| Main Layout | `MiniExplorer.UI/Views/MainWindow.axaml` |
| Main ViewModel | `MiniExplorer.UI/ViewModels/ExplorerViewModel.cs` |
| File Operations | `MiniExplorer.Core/Services/DirectoryService.cs` |
| Models | `MiniExplorer.Core/Models/` |
| Converters | `MiniExplorer.UI/Converters/Converters.cs` |
| Tests | `MiniExplorer.Tests/DirectoryServiceTests.cs` |

## 🎨 Color Palette

| Element | Hex Code | Usage |
|---------|----------|-------|
| Background | `#1E1E1E` | Main window |
| Sidebar | `#252525` | Left panel |
| Hover | `#2A2A2A` | Interactive feedback |
| Zorin Blue | `#3584E4` | Selection, folders |
| Border | `#3A3A3A` | Dividers |
| Text | `#FFFFFF` | All text, file icons |
| Gray | `#666666` | Breadcrumb separator |

## 🔧 Key Classes

### DirectoryService Methods
```csharp
GetDirectoryContents(string path)       // List files/folders
GetStandardPlaces()                     // Sidebar locations
GetBreadcrumbSegments(string path)      // Path segments
GetParentDirectory(string path)         // Go up one level
```

### ExplorerViewModel Commands
```csharp
NavigateToPathCommand           // Go to specific path
GoBackCommand                   // Navigate backward
GoForwardCommand                // Navigate forward
ItemDoubleClickCommand          // Open folder
NavigateToBreadcrumbCommand     // Jump to breadcrumb
NavigateToPlaceCommand          // Sidebar navigation
```

## 🎯 Data Binding Examples

### Bind to ViewModel Property
```xml
<TextBlock Text="{Binding Explorer.CurrentPath}"/>
```

### Bind to Command
```xml
<Button Command="{Binding Explorer.GoBackCommand}"/>
```

### Bind with Converter
```xml
<PathIcon Foreground="{Binding IsDirectory, 
          Converter={StaticResource FolderColorConverter}}"/>
```

### Bind Collection
```xml
<ListBox ItemsSource="{Binding Explorer.Items}"/>
```

## 🔍 Troubleshooting

### Build Errors

| Error | Solution |
|-------|----------|
| Missing using | Add `using System.Collections.Generic;` |
| Type not found | Check project references |
| XAML binding error | Check property names (case-sensitive) |
| Converter not found | Register in `App.axaml` Resources |

### Runtime Issues

| Issue | Check |
|-------|-------|
| Empty list | Path permissions, valid directory |
| Icons not showing | Converter registration, SVG path data |
| Commands not working | `[RelayCommand]` attribute, binding syntax |
| Navigation broken | History stack, CurrentPath updates |

## 📊 Project Stats

- **Lines of Code**: ~1500
- **Projects**: 3 (Core, UI, Tests)
- **Tests**: 5 (all passing)
- **Dependencies**: Avalonia UI, CommunityToolkit.Mvvm
- **Target**: .NET 8, Linux

## 🎮 User Interactions

| Action | Result |
|--------|--------|
| Click sidebar item | Navigate to location |
| Double-click folder | Open folder |
| Click breadcrumb | Jump to that level |
| Click Back | Previous directory |
| Click Forward | Next directory (after back) |

## 📝 Code Patterns

### Observable Property
```csharp
[ObservableProperty]
private string _currentPath;
// Auto-generates CurrentPath property with INotifyPropertyChanged
```

### Relay Command
```csharp
[RelayCommand]
private void GoBack()
{
    // Implementation
}
// Auto-generates GoBackCommand
```

### Collection Update
```csharp
Items.Clear();
foreach (var item in newItems)
{
    Items.Add(item);
}
// UI updates automatically
```

## 🏗️ Architecture Flow

```
User Click
    ↓
MainWindow.axaml (View)
    ↓
ExplorerViewModel (ViewModel)
    ↓
DirectoryService (Core)
    ↓
File System
    ↓
ObservableCollection Update
    ↓
UI Auto-refresh
```

## 🧪 Testing Pattern

```csharp
[Fact]
public void Method_Scenario_Expected()
{
    // Arrange
    var service = new DirectoryService();
    var testPath = "/home/user";
    
    // Act
    var result = service.GetDirectoryContents(testPath);
    
    // Assert
    Assert.NotNull(result);
    Assert.True(result.Count >= 0);
}
```

## 🎨 Icon Resources

All icons use Material Design SVG paths:
- Folder: Closed folder icon
- File: Document icon
- Home: House icon
- Desktop: Monitor icon
- Documents: Paper with text
- Downloads: Arrow down to tray
- Music: Musical note
- Pictures: Image/photo
- Videos: Video camera
- Trash: Trash bin

## 📦 NuGet Packages

```xml
<PackageReference Include="Avalonia" Version="11.0.10" />
<PackageReference Include="Avalonia.Themes.Fluent" Version="11.0.10" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
```

## 🌐 Resources

- **Avalonia Docs**: https://docs.avaloniaui.net/
- **MVVM Toolkit**: https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/
- **Material Icons**: https://materialdesignicons.com/

---

**Version**: 1.0  
**Platform**: Linux (Zorin OS)  
**Framework**: .NET 8 + Avalonia  
**License**: Educational/Demo
