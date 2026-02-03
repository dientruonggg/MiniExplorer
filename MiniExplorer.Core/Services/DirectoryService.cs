using MiniExplorer.Core.Models;

namespace MiniExplorer.Core.Services;

public class DirectoryService
{
    /// <summary>
    /// Gets the list of files and folders in the specified directory
    /// </summary>
    /// <param name="path">Linux-style path (e.g., /home/user/documents)</param>
    /// <returns>List of FileSystemItems</returns>
    public List<FileSystemItem> GetDirectoryContents(string path)
    {
        var items = new List<FileSystemItem>();
        
        try
        {
            // Handle home directory shortcut
            if (path.StartsWith("~"))
            {
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
                    path.Substring(1).TrimStart('/'));
            }
            
            // Validate path exists
            if (!Directory.Exists(path))
            {
                return items;
            }
            
            // Get directories first
            var directories = Directory.GetDirectories(path);
            foreach (var dir in directories)
            {
                try
                {
                    var dirInfo = new DirectoryInfo(dir);
                    items.Add(new FileSystemItem
                    {
                        Name = dirInfo.Name,
                        FullPath = dirInfo.FullName,
                        IsDirectory = true,
                        LastModified = dirInfo.LastWriteTime,
                        Extension = string.Empty
                    });
                }
                catch (UnauthorizedAccessException)
                {
                    // Skip directories we don't have permission to access
                    continue;
                }
            }
            
            // Get files
            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                try
                {
                    var fileInfo = new FileInfo(file);
                    items.Add(new FileSystemItem
                    {
                        Name = fileInfo.Name,
                        FullPath = fileInfo.FullName,
                        IsDirectory = false,
                        Size = fileInfo.Length,
                        LastModified = fileInfo.LastWriteTime,
                        Extension = fileInfo.Extension
                    });
                }
                catch (UnauthorizedAccessException)
                {
                    // Skip files we don't have permission to access
                    continue;
                }
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Access denied to directory: {path} - {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading directory: {path} - {ex.Message}");
        }
        
        return items;
    }
    
    /// <summary>
    /// Gets the standard Linux places/locations
    /// </summary>
    public List<PlaceItem> GetStandardPlaces()
    {
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        
        return new List<PlaceItem>
        {
            new() { Name = "Home", Path = homeDir, Icon = "Home" },
            new() { Name = "Desktop", Path = Path.Combine(homeDir, "Desktop"), Icon = "Desktop" },
            new() { Name = "Documents", Path = Path.Combine(homeDir, "Documents"), Icon = "Documents" },
            new() { Name = "Downloads", Path = Path.Combine(homeDir, "Downloads"), Icon = "Downloads" },
            new() { Name = "Music", Path = Path.Combine(homeDir, "Music"), Icon = "Music" },
            new() { Name = "Pictures", Path = Path.Combine(homeDir, "Pictures"), Icon = "Pictures" },
            new() { Name = "Videos", Path = Path.Combine(homeDir, "Videos"), Icon = "Videos" },
            new() { Name = "Trash", Path = Path.Combine(homeDir, ".local/share/Trash/files"), Icon = "Trash" }
        };
    }
    
    /// <summary>
    /// Gets breadcrumb path segments from a full path
    /// </summary>
    public List<(string Name, string FullPath)> GetBreadcrumbSegments(string path)
    {
        var segments = new List<(string Name, string FullPath)>();
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        
        // Replace home directory with ~
        if (path.StartsWith(homeDir))
        {
            segments.Add(("Home", homeDir));
            path = path.Substring(homeDir.Length).TrimStart('/');
        }
        else
        {
            segments.Add(("/", "/"));
        }
        
        if (string.IsNullOrEmpty(path))
            return segments;
        
        var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var currentPath = segments[0].FullPath;
        
        foreach (var part in parts)
        {
            currentPath = Path.Combine(currentPath, part);
            segments.Add((part, currentPath));
        }
        
        return segments;
    }
    
    /// <summary>
    /// Gets the parent directory path
    /// </summary>
    public string? GetParentDirectory(string path)
    {
        try
        {
            var dirInfo = new DirectoryInfo(path);
            return dirInfo.Parent?.FullName;
        }
        catch
        {
            return null;
        }
    }
}
