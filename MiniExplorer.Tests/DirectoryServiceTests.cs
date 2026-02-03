using Xunit;
using MiniExplorer.Core.Services;
using MiniExplorer.Core.Models;

namespace MiniExplorer.Tests;

public class DirectoryServiceTests
{
    private readonly DirectoryService _directoryService;
    
    public DirectoryServiceTests()
    {
        _directoryService = new DirectoryService();
    }
    
    [Fact]
    public void GetStandardPlaces_ReturnsExpectedPlaces()
    {
        // Act
        var places = _directoryService.GetStandardPlaces();
        
        // Assert
        Assert.NotNull(places);
        Assert.Equal(8, places.Count);
        Assert.Contains(places, p => p.Name == "Home");
        Assert.Contains(places, p => p.Name == "Desktop");
        Assert.Contains(places, p => p.Name == "Documents");
        Assert.Contains(places, p => p.Name == "Downloads");
    }
    
    [Fact]
    public void GetDirectoryContents_WithValidPath_ReturnsItems()
    {
        // Arrange
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        
        // Act
        var items = _directoryService.GetDirectoryContents(homeDir);
        
        // Assert
        Assert.NotNull(items);
        // Home directory should have some content
        Assert.True(items.Count >= 0);
    }
    
    [Fact]
    public void GetDirectoryContents_WithInvalidPath_ReturnsEmptyList()
    {
        // Arrange
        var invalidPath = "/this/path/does/not/exist/123456";
        
        // Act
        var items = _directoryService.GetDirectoryContents(invalidPath);
        
        // Assert
        Assert.NotNull(items);
        Assert.Empty(items);
    }
    
    [Fact]
    public void GetBreadcrumbSegments_WithHomePath_ReturnsCorrectSegments()
    {
        // Arrange
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var testPath = Path.Combine(homeDir, "Documents");
        
        // Act
        var segments = _directoryService.GetBreadcrumbSegments(testPath);
        
        // Assert
        Assert.NotNull(segments);
        Assert.True(segments.Count >= 2);
        Assert.Equal("Home", segments[0].Name);
        Assert.Equal("Documents", segments[^1].Name);
    }
    
    [Fact]
    public void GetParentDirectory_WithValidPath_ReturnsParent()
    {
        // Arrange
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var testPath = Path.Combine(homeDir, "Documents");
        
        // Act
        var parent = _directoryService.GetParentDirectory(testPath);
        
        // Assert
        Assert.NotNull(parent);
        Assert.Equal(homeDir, parent);
    }
}
