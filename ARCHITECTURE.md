# MiniExplorer - Architecture & Data Flow Diagram

## 🏗️ System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         USER INTERFACE                          │
│                    (Avalonia XAML - Views)                      │
│                                                                 │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │                   MainWindow.axaml                       │  │
│  │                                                          │  │
│  │  ┌─────────────┐  ┌───────────────────────────────┐    │  │
│  │  │  Nav Bar    │  │  ┌──────┐  ┌──────────────┐  │    │  │
│  │  │  [◀][▶]     │  │  │Side  │  │ Main Content │  │    │  │
│  │  │  Breadcrumb │  │  │bar   │  │   (Grid)     │  │    │  │
│  │  │  [🔍]       │  │  │      │  │              │  │    │  │
│  │  └─────────────┘  │  │Places│  │  📁 📁 📄    │  │    │  │
│  │                    │  │      │  │  📁 📄 📄    │  │    │  │
│  │                    │  └──────┘  └──────────────┘  │    │  │
│  └──────────────────────────────────────────────────────────┘  │
│                              ↕ Data Binding                     │
└─────────────────────────────────────────────────────────────────┘
                               ↕
┌─────────────────────────────────────────────────────────────────┐
│                         VIEW MODELS                             │
│                    (MVVM Pattern - Logic)                       │
│                                                                 │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │              ExplorerViewModel                           │  │
│  │                                                          │  │
│  │  Properties:                    Commands:                │  │
│  │  • CurrentPath                  • NavigateToPath         │  │
│  │  • Items (Observable)           • GoBack                 │  │
│  │  • Places (Observable)          • GoForward              │  │
│  │  • Breadcrumbs (Observable)     • ItemDoubleClick        │  │
│  │  • CanGoBack/Forward            • NavigateToBreadcrumb   │  │
│  │                                 • NavigateToPlace        │  │
│  │  History:                                                │  │
│  │  • Stack<string> _backHistory                            │  │
│  │  • Stack<string> _forwardHistory                         │  │
│  └──────────────────────────────────────────────────────────┘  │
│                              ↕ Service Calls                    │
└─────────────────────────────────────────────────────────────────┘
                               ↕
┌─────────────────────────────────────────────────────────────────┐
│                         CORE LAYER                              │
│                   (Business Logic - Services)                   │
│                                                                 │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │              DirectoryService                            │  │
│  │                                                          │  │
│  │  Methods:                                                │  │
│  │  • GetDirectoryContents(path)                            │  │
│  │    → List<FileSystemItem>                                │  │
│  │                                                          │  │
│  │  • GetStandardPlaces()                                   │  │
│  │    → List<PlaceItem>                                     │  │
│  │                                                          │  │
│  │  • GetBreadcrumbSegments(path)                           │  │
│  │    → List<(Name, FullPath)>                              │  │
│  │                                                          │  │
│  │  • GetParentDirectory(path)                              │  │
│  │    → string?                                             │  │
│  └──────────────────────────────────────────────────────────┘  │
│                              ↕ System.IO                        │
└─────────────────────────────────────────────────────────────────┘
                               ↕
┌─────────────────────────────────────────────────────────────────┐
│                      FILE SYSTEM (Linux)                        │
│                                                                 │
│         /home/user/                                             │
│         ├── Desktop/                                            │
│         ├── Documents/                                          │
│         ├── Downloads/                                          │
│         ├── Music/                                              │
│         ├── Pictures/                                           │
│         ├── Videos/                                             │
│         └── .local/share/Trash/                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🔄 Data Flow - Navigation Example

### Scenario: User double-clicks on "Documents" folder

```
1. USER ACTION
   ↓
   Double-click on folder item in grid
   ↓

2. VIEW (MainWindow.axaml.cs)
   ↓
   OnDoubleTapped event handler
   ↓
   vm.Explorer.ItemDoubleClickCommand.Execute(item)
   ↓

3. VIEW MODEL (ExplorerViewModel.cs)
   ↓
   ItemDoubleClick(FileSystemItem item)
   ↓
   if (item.IsDirectory) → NavigateToPath(item.FullPath)
   ↓
   _backHistory.Push(CurrentPath)  // Save current to history
   _forwardHistory.Clear()         // Clear forward history
   CurrentPath = item.FullPath     // Update property
   LoadDirectory(item.FullPath)    // Load new content
   ↓

4. CORE SERVICE (DirectoryService.cs)
   ↓
   GetDirectoryContents("/home/user/Documents")
   ↓
   Directory.GetDirectories(path)   // System.IO
   Directory.GetFiles(path)         // System.IO
   ↓
   Returns List<FileSystemItem>
   ↓

5. VIEW MODEL (ExplorerViewModel.cs)
   ↓
   Items.Clear()
   foreach (item in contents)
       Items.Add(item)  // ObservableCollection
   UpdateBreadcrumbs()
   ↓

6. VIEW (MainWindow.axaml)
   ↓
   ListBox ItemsSource binding auto-updates
   ↓
   UI re-renders with new files/folders
   ↓

7. USER SEES
   ↓
   Updated grid showing Documents folder contents
   Breadcrumb shows: Home › user › Documents
   Back button now enabled
```

---

## 🎯 MVVM Pattern Implementation

```
┌─────────────────────────────────────────────────────────────┐
│                         VIEW (XAML)                         │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  <ListBox ItemsSource="{Binding Explorer.Items}">    │  │
│  │      <ListBox.ItemTemplate>                           │  │
│  │          <DataTemplate>                               │  │
│  │              <StackPanel>                             │  │
│  │                  <PathIcon                            │  │
│  │                      Foreground="{Binding             │  │
│  │                          IsDirectory,                 │  │
│  │                          Converter={StaticResource    │  │
│  │                              FolderColorConverter}}"  │  │
│  │                  <TextBlock Text="{Binding Name}"/>   │  │
│  │              </StackPanel>                            │  │
│  │          </DataTemplate>                              │  │
│  │      </ListBox.ItemTemplate>                          │  │
│  │  </ListBox>                                           │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                               ↕
                      {Binding} Data Binding
                               ↕
┌─────────────────────────────────────────────────────────────┐
│                    VIEW MODEL (C#)                          │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  [ObservableProperty]                                 │  │
│  │  private ObservableCollection<FileSystemItem> _items; │  │
│  │  // Auto-generates:                                   │  │
│  │  // public ObservableCollection<FileSystemItem> Items │  │
│  │  //     get => _items;                                │  │
│  │  //     set { _items = value; OnPropertyChanged(); }  │  │
│  │                                                        │  │
│  │  [RelayCommand]                                       │  │
│  │  private void ItemDoubleClick(FileSystemItem item)    │  │
│  │  {                                                     │  │
│  │      NavigateToPath(item.FullPath);                   │  │
│  │  }                                                     │  │
│  │  // Auto-generates: ItemDoubleClickCommand            │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                               ↕
                     Service Method Calls
                               ↕
┌─────────────────────────────────────────────────────────────┐
│                       MODEL/SERVICE (C#)                    │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  public class FileSystemItem                          │  │
│  │  {                                                     │  │
│  │      public string Name { get; set; }                 │  │
│  │      public string FullPath { get; set; }             │  │
│  │      public bool IsDirectory { get; set; }            │  │
│  │      public long Size { get; set; }                   │  │
│  │      public DateTime LastModified { get; set; }       │  │
│  │  }                                                     │  │
│  │                                                        │  │
│  │  public class DirectoryService                        │  │
│  │  {                                                     │  │
│  │      public List<FileSystemItem>                      │  │
│  │          GetDirectoryContents(string path)            │  │
│  │      {                                                 │  │
│  │          // System.IO operations                      │  │
│  │          return items;                                │  │
│  │      }                                                 │  │
│  │  }                                                     │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

---

## 🎨 Value Converter Flow

```
┌─────────────────────────────────────────────────────────────┐
│                      DATA (Model)                           │
│                                                             │
│   FileSystemItem {                                          │
│       Name = "Projects",                                    │
│       IsDirectory = true  ──┐                               │
│   }                         │                               │
└─────────────────────────────┼───────────────────────────────┘
                              │
                              ↓ Binding with Converter
┌─────────────────────────────┼───────────────────────────────┐
│              CONVERTER (FolderColorConverter)               │
│                             │                               │
│   Convert(value, ...)       │                               │
│   {                         │                               │
│       if (value is bool isDirectory && isDirectory)         │
│           return new SolidColorBrush(Color.Parse("#3584E4"))│
│       return new SolidColorBrush(Colors.White);             │
│   }                         │                               │
│                             │                               │
│   Input: true ─────────────┘                                │
│   Output: SolidColorBrush(#3584E4)  ← Zorin Blue            │
└─────────────────────────────┼───────────────────────────────┘
                              │
                              ↓ Applied to UI
┌─────────────────────────────┼───────────────────────────────┐
│                       UI (XAML)                             │
│                             │                               │
│   <PathIcon                 │                               │
│       Data="M10,4H4C2.89..." (folder icon SVG)              │
│       Foreground="#3584E4"  ← Result from converter         │
│       Width="48"                                            │
│       Height="48"/>         │                               │
│                             │                               │
│   Renders: 📁 (blue folder icon)                            │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔄 Navigation History Flow

```
Initial State:
CurrentPath: /home/user
BackStack: []
ForwardStack: []

User navigates to Documents:
CurrentPath: /home/user/Documents
BackStack: [/home/user]
ForwardStack: []

User navigates to Projects:
CurrentPath: /home/user/Documents/Projects
BackStack: [/home/user, /home/user/Documents]
ForwardStack: []

User clicks Back:
CurrentPath: /home/user/Documents
BackStack: [/home/user]
ForwardStack: [/home/user/Documents/Projects]

User clicks Back again:
CurrentPath: /home/user
BackStack: []
ForwardStack: [/home/user/Documents/Projects, /home/user/Documents]

User clicks Forward:
CurrentPath: /home/user/Documents
BackStack: [/home/user]
ForwardStack: [/home/user/Documents/Projects]

User navigates to Downloads (clears forward):
CurrentPath: /home/user/Downloads
BackStack: [/home/user, /home/user/Documents]
ForwardStack: []  ← Cleared!
```

---

## 🎯 Dependency Flow

```
┌─────────────────────────────────────────────────────────────┐
│                    MiniExplorer.UI                          │
│                    (Presentation)                           │
│                         │                                   │
│                         │ References                        │
│                         ↓                                   │
│  ┌──────────────────────────────────────────────────────┐  │
│  │             MiniExplorer.Core                        │  │
│  │             (Business Logic)                         │  │
│  │                     │                                │  │
│  │                     │ Uses                           │  │
│  │                     ↓                                │  │
│  │           System.IO (Framework)                      │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                             │
│  Note: Core has NO dependency on UI                        │
│        UI depends on Core (one-way dependency)              │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                  MiniExplorer.Tests                         │
│                  (Unit Testing)                             │
│                         │                                   │
│                         │ References                        │
│                         ↓                                   │
│  ┌──────────────────────────────────────────────────────┐  │
│  │             MiniExplorer.Core                        │  │
│  │             (Tests the Core logic)                   │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                             │
│  Note: Tests can test Core independently of UI             │
└─────────────────────────────────────────────────────────────┘
```

---

## 📊 Component Interaction Diagram

```
User
 │
 │ Interacts with
 ↓
MainWindow (View)
 │
 │ Data Binding
 ↓
MainWindowViewModel
 │
 │ Contains
 ↓
ExplorerViewModel ─────────→ DirectoryService
 │                                  │
 │ Updates                          │ Calls
 ↓                                  ↓
ObservableCollections          System.IO APIs
 │                                  │
 │ Triggers                         │ Returns
 ↓                                  ↓
UI Auto-Update               File/Folder Data
 │                                  │
 │ Renders                          │
 ↓                                  ↓
Visual Display  ←─────────────── Models
(Grid, Icons, Text)          (FileSystemItem)
```

---

## 🎨 Styling Pipeline

```
XAML Style Definition
    ↓
Style Selector (e.g., Button.nav-button:pointerover)
    ↓
Setter Properties (Background, Foreground, etc.)
    ↓
Applied to Control Template
    ↓
Rendered on Screen with Visual Effects
```

---

## 🔧 Build & Run Process

```
Developer
    ↓
    Writes/Modifies Code
    ↓
dotnet build ──→ Compile C# → Generate Assemblies
    │                              │
    │                              ↓
    │                       MiniExplorer.Core.dll
    │                       MiniExplorer.UI.dll
    │                       MiniExplorer.Tests.dll
    ↓
dotnet test ───→ Run xUnit Tests → Test Results
    ↓
dotnet run ────→ Launch Application
    │
    ↓
Avalonia Renderer ──→ Display UI
    │
    ↓
User Interaction Loop
```

---

## 📦 Package Dependencies

```
MiniExplorer.UI
├── Avalonia (11.0.10)
│   ├── Avalonia.Desktop
│   ├── Avalonia.Themes.Fluent
│   └── Avalonia.Diagnostics (Debug)
├── CommunityToolkit.Mvvm (8.2.2)
│   ├── ObservableObject
│   ├── ObservableProperty
│   └── RelayCommand
└── MiniExplorer.Core (Project Reference)

MiniExplorer.Core
└── .NET 8 Base Class Library
    └── System.IO

MiniExplorer.Tests
├── xUnit
├── xUnit.runner.visualstudio
└── MiniExplorer.Core (Project Reference)
```

---

## 🎯 Summary

This architecture provides:

1. **Clean Separation**: UI ← ViewModel ← Service → File System
2. **Testability**: Core logic tested independently
3. **Maintainability**: Each layer has clear responsibility
4. **Scalability**: Easy to add features without breaking existing code
5. **MVVM Benefits**: Automatic UI updates, command binding
6. **Linux Native**: Direct file system access with proper error handling

---

*Architecture designed for clarity, testability, and maintainability*
