# MiniExplorer Development Guide

## Project Structure Overview

```
MiniExplorer/
├── MiniExplorer.sln                    # Solution file
├── README.md                           # User documentation
├── DEVELOPMENT.md                      # This file
├── start.sh                            # Quick launch script
│
├── MiniExplorer.Core/                  # Core Business Logic Layer
│   ├── Models/
│   │   ├── FileSystemItem.cs          # Represents a file or folder
│   │   └── PlaceItem.cs               # Represents sidebar locations
│   └── Services/
│       └── DirectoryService.cs        # File system operations
│
├── MiniExplorer.UI/                    # Presentation Layer (Avalonia)
│   ├── App.axaml                      # Application entry point
│   ├── App.axaml.cs
│   ├── Assets/                        # Icons and resources
│   ├── Converters/
│   │   └── Converters.cs              # Value converters for data binding
│   ├── ViewModels/
│   │   ├── ViewModelBase.cs           # Base ViewModel class
│   │   ├── MainWindowViewModel.cs     # Main window VM
│   │   └── ExplorerViewModel.cs       # File explorer VM (main logic)
│   └── Views/
│       ├── MainWindow.axaml           # Main window XAML
│       └── MainWindow.axaml.cs        # Code-behind for events
│
└── MiniExplorer.Tests/                 # Unit Tests
    └── DirectoryServiceTests.cs       # Tests for DirectoryService
```

## Key Components Explained

### 1. Core Layer (MiniExplorer.Core)

#### DirectoryService.cs
**Purpose**: Handles all file system operations

**Key Methods**:
- `GetDirectoryContents(string path)`: Returns files and folders in a directory
  - Handles UnauthorizedAccessException gracefully
  - Sorts directories before files
  - Returns empty list for invalid paths

- `GetStandardPlaces()`: Returns the 8 standard Linux locations
  - Home, Desktop, Documents, Downloads, Music, Pictures, Videos, Trash
  - Uses `Environment.SpecialFolder` for cross-platform compatibility

- `GetBreadcrumbSegments(string path)`: Converts path to breadcrumb segments
  - Replaces home directory with "Home" label
  - Splits path into clickable segments

- `GetParentDirectory(string path)`: Returns parent directory or null

**Linux Path Handling**:
```csharp
// Supports tilde expansion
if (path.StartsWith("~"))
{
    path = Path.Combine(Environment.GetFolderPath(
        Environment.SpecialFolder.UserProfile), 
        path.Substring(1).TrimStart('/'));
}
```

### 2. UI Layer (MiniExplorer.UI)

#### ExplorerViewModel.cs
**Purpose**: Main ViewModel managing the file explorer state

**Properties**:
- `CurrentPath`: Current directory path
- `Items`: ObservableCollection of files/folders to display
- `Places`: ObservableCollection of sidebar locations
- `Breadcrumbs`: Path segments for navigation bar
- `CanGoBack` / `CanGoForward`: Navigation state

**Commands**:
- `NavigateToPathCommand`: Navigate to a specific path
- `GoBackCommand` / `GoForwardCommand`: History navigation
- `ItemDoubleClickCommand`: Open folder on double-click
- `NavigateToBreadcrumbCommand`: Jump to breadcrumb segment
- `NavigateToPlaceCommand`: Navigate from sidebar

**Navigation History**:
```csharp
private readonly Stack<string> _backHistory;
private readonly Stack<string> _forwardHistory;
```

#### MainWindow.axaml
**Purpose**: Main UI layout in XAML

**Layout Structure**:
```xml
<Grid>
    <!-- Top Navigation Bar -->
    <Border> <!-- Back/Forward + Breadcrumbs + Search -->
    
    <Grid> <!-- Main Content -->
        <!-- Left Sidebar -->
        <Border>
            <ListBox ItemsSource="{Binding Explorer.Places}"/>
        </Border>
        
        <!-- Main File View -->
        <ScrollViewer>
            <ListBox ItemsSource="{Binding Explorer.Items}">
                <ListBox.ItemsPanel>
                    <WrapPanel/> <!-- Grid layout -->
                </ListBox.ItemsPanel>
            </ListBox>
        </ScrollViewer>
    </Grid>
</Grid>
```

**Styling Classes**:
- `.nav-button`: Transparent buttons with hover effect
- `.breadcrumb`: Breadcrumb button style
- `.place-item`: Sidebar item style
- `.file-item`: File/folder item style

#### Converters.cs
**Purpose**: Convert data for UI display

**Converters**:
1. `FileIconConverter`: Returns SVG path geometry for file/folder icons
2. `FolderColorConverter`: Returns Zorin Blue for folders, white for files
3. `IconConverter`: Maps place names (Home, Desktop, etc.) to icons

### 3. Data Binding Flow

```
User Action (Click) 
    → Command in ViewModel 
    → DirectoryService operation 
    → Update ObservableCollection 
    → UI auto-updates via binding
```

**Example**: Double-clicking a folder
```csharp
// MainWindow.axaml.cs - Event handler
private void OnDoubleTapped(object? sender, TappedEventArgs e)
{
    vm.Explorer.ItemDoubleClickCommand.Execute(vm.Explorer.SelectedItem);
}

// ExplorerViewModel.cs - Command implementation
[RelayCommand]
private void ItemDoubleClick(FileSystemItem? item)
{
    if (item == null || !item.IsDirectory)
        return;
    
    NavigateToPath(item.FullPath);
}

// NavigateToPath updates CurrentPath and calls LoadDirectory
// LoadDirectory updates Items ObservableCollection
// UI automatically reflects changes
```

## Development Workflow

### Adding a New Feature

1. **Core Logic** (if needed)
   - Add methods to `DirectoryService.cs`
   - Add unit tests to `DirectoryServiceTests.cs`

2. **ViewModel**
   - Add properties/commands to `ExplorerViewModel.cs`
   - Use `[ObservableProperty]` for data binding
   - Use `[RelayCommand]` for user actions

3. **UI**
   - Update `MainWindow.axaml` with new controls
   - Bind to ViewModel properties/commands
   - Add styling in Window.Styles or App.axaml

4. **Test**
   - Build: `dotnet build`
   - Test: `dotnet test`
   - Run: `dotnet run --project MiniExplorer.UI`

### Debugging Tips

**Console Logging**:
```csharp
Console.WriteLine($"Error loading directory: {ex.Message}");
```
Run with: `dotnet run --project MiniExplorer.UI` to see console output

**Check Bindings**:
- Use Avalonia DevTools (F12 in debug mode)
- Check DataContext is set correctly
- Verify property names match exactly

**XAML Errors**:
- Most binding errors appear in build output
- Use `d:DataType="vm:ViewModelName"` for IntelliSense

## Code Style Guidelines

### C# Conventions
- Use nullable reference types (`FileSystemItem?`)
- Prefer `var` for local variables
- Use expression-bodied members where appropriate
- Handle exceptions explicitly

### MVVM Best Practices
- ViewModels should NOT reference Views
- Use Commands for user actions, not event handlers
- Keep business logic in Core layer
- Use ObservableCollections for dynamic lists

### XAML Conventions
- Use semantic naming (descriptive classes/IDs)
- Group related styles together
- Use Grid for complex layouts, StackPanel for simple ones
- Prefer binding over code-behind

## Testing

### Running Tests
```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test
dotnet test --filter "FullyQualifiedName~GetDirectoryContents"
```

### Writing Tests
```csharp
[Fact]
public void MethodName_Scenario_ExpectedResult()
{
    // Arrange
    var service = new DirectoryService();
    
    // Act
    var result = service.SomeMethod();
    
    // Assert
    Assert.NotNull(result);
}
```

## Building and Running

### Development
```bash
# Build in Debug mode
dotnet build

# Run in Debug mode
dotnet run --project MiniExplorer.UI/MiniExplorer.UI.csproj
```

### Production
```bash
# Build in Release mode
dotnet build --configuration Release

# Publish as self-contained
dotnet publish MiniExplorer.UI/MiniExplorer.UI.csproj \
    -c Release \
    -r linux-x64 \
    --self-contained true \
    -o ./publish
```

### Quick Start Script
```bash
# Use the provided script
./start.sh
```

## Common Issues and Solutions

### Issue: "Type or namespace name could not be found"
**Solution**: Check using directives, ensure project references are correct

### Issue: XAML binding not working
**Solution**: 
1. Check property names (case-sensitive)
2. Verify DataContext is set
3. Ensure property uses `[ObservableProperty]`

### Issue: Command not executing
**Solution**:
1. Check command is marked with `[RelayCommand]`
2. Verify binding syntax in XAML
3. Check command parameter types match

### Issue: Icons not showing
**Solution**: 
1. Verify converters are registered in App.axaml
2. Check SVG path data is correct
3. Ensure `StaticResource` names match

## Avalonia-Specific Tips

### Hot Reload
- Avalonia supports XAML hot reload in some scenarios
- Changes to XAML may reflect without rebuild
- ViewModel changes require rebuild

### DevTools
- Press F12 in running application (Debug mode)
- Inspect visual tree
- Check property values
- Debug data bindings

### Performance
- Use virtualization for large lists (VirtualizingStackPanel)
- Lazy load images/data when possible
- Avoid complex bindings in DataTemplates

## Resources

- [Avalonia Documentation](https://docs.avaloniaui.net/)
- [MVVM Toolkit](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [Material Design Icons](https://materialdesignicons.com/)

## Next Steps

1. Implement search functionality
2. Add file operations (copy, move, delete)
3. Create context menus
4. Add keyboard shortcuts
5. Implement drag-and-drop

---

Happy coding! 🚀
