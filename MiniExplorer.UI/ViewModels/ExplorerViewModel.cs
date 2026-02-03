using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiniExplorer.Core.Models;
using MiniExplorer.Core.Services;

namespace MiniExplorer.UI.ViewModels;

public partial class ExplorerViewModel : ViewModelBase
{
    private readonly DirectoryService _directoryService;
    private readonly Stack<string> _backHistory;
    private readonly Stack<string> _forwardHistory;
    
    [ObservableProperty]
    private string _currentPath;
    
    [ObservableProperty]
    private ObservableCollection<FileSystemItem> _items;
    
    [ObservableProperty]
    private ObservableCollection<PlaceItem> _places;
    
    [ObservableProperty]
    private ObservableCollection<BreadcrumbSegment> _breadcrumbs;
    
    [ObservableProperty]
    private bool _canGoBack;
    
    [ObservableProperty]
    private bool _canGoForward;
    
    [ObservableProperty]
    private FileSystemItem? _selectedItem;
    
    [ObservableProperty]
    private PlaceItem? _selectedPlace;
    
    public ExplorerViewModel()
    {
        _directoryService = new DirectoryService();
        _backHistory = new Stack<string>();
        _forwardHistory = new Stack<string>();
        
        _currentPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        _items = new ObservableCollection<FileSystemItem>();
        _places = new ObservableCollection<PlaceItem>();
        _breadcrumbs = new ObservableCollection<BreadcrumbSegment>();
        
        InitializePlaces();
        LoadDirectory(_currentPath);
        
        // Add property changed handlers for selection
        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(SelectedItem) && SelectedItem?.IsDirectory == true)
            {
                // Double-click simulation on selection for now
            }
            else if (e.PropertyName == nameof(SelectedPlace) && SelectedPlace != null)
            {
                NavigateToPath(SelectedPlace.Path);
            }
        };
    }
    
    private void InitializePlaces()
    {
        var places = _directoryService.GetStandardPlaces();
        Places.Clear();
        foreach (var place in places)
        {
            Places.Add(place);
        }
    }
    
    private void LoadDirectory(string path)
    {
        try
        {
            var contents = _directoryService.GetDirectoryContents(path);
            Items.Clear();
            
            // Sort: directories first, then files
            var sortedContents = contents
                .OrderByDescending(x => x.IsDirectory)
                .ThenBy(x => x.Name);
            
            foreach (var item in sortedContents)
            {
                Items.Add(item);
            }
            
            UpdateBreadcrumbs();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading directory: {ex.Message}");
        }
    }
    
    private void UpdateBreadcrumbs()
    {
        var segments = _directoryService.GetBreadcrumbSegments(CurrentPath);
        Breadcrumbs.Clear();
        
        foreach (var segment in segments)
        {
            Breadcrumbs.Add(new BreadcrumbSegment 
            { 
                Name = segment.Name, 
                FullPath = segment.FullPath 
            });
        }
    }
    
    [RelayCommand]
    private void NavigateToPath(string path)
    {
        if (string.IsNullOrEmpty(path) || path == CurrentPath)
            return;
        
        _backHistory.Push(CurrentPath);
        _forwardHistory.Clear();
        
        CurrentPath = path;
        LoadDirectory(path);
        
        CanGoBack = _backHistory.Count > 0;
        CanGoForward = false;
    }
    
    [RelayCommand]
    private void ItemDoubleClick(FileSystemItem? item)
    {
        if (item == null || !item.IsDirectory)
            return;
        
        NavigateToPath(item.FullPath);
    }
    
    [RelayCommand]
    private void GoBack()
    {
        if (_backHistory.Count == 0)
            return;
        
        _forwardHistory.Push(CurrentPath);
        CurrentPath = _backHistory.Pop();
        LoadDirectory(CurrentPath);
        
        CanGoBack = _backHistory.Count > 0;
        CanGoForward = true;
    }
    
    [RelayCommand]
    private void GoForward()
    {
        if (_forwardHistory.Count == 0)
            return;
        
        _backHistory.Push(CurrentPath);
        CurrentPath = _forwardHistory.Pop();
        LoadDirectory(CurrentPath);
        
        CanGoBack = true;
        CanGoForward = _forwardHistory.Count > 0;
    }
    
    [RelayCommand]
    private void NavigateToBreadcrumb(BreadcrumbSegment? segment)
    {
        if (segment == null)
            return;
        
        NavigateToPath(segment.FullPath);
    }
    
    [RelayCommand]
    private void NavigateToPlace(PlaceItem? place)
    {
        if (place == null)
            return;
        
        NavigateToPath(place.Path);
    }
}

public class BreadcrumbSegment
{
    public string Name { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
}
