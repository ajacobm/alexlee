using System.Collections.Concurrent;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace AlexLee.Algorithms;

/// <summary>
/// File search utilities with parallel processing and cross-platform support
/// Handles Windows/PowerShell development and Docker container environments
/// </summary>
public static class FileSearchUtilities
{
    /// <summary>
    /// Problem #3: Searches for text in files within a directory using parallel processing
    /// Enhanced to handle Windows/Docker pathing scenarios
    /// </summary>
    /// <param name="sourceDirectoryPath">Directory to search in</param>
    /// <param name="searchText">Text to search for</param>
    /// <param name="destinationFileName">Output file name</param>
    /// <param name="logger">Optional logger for diagnostics</param>
    /// <returns>Search results containing file count, line count, and occurrence count</returns>
    public static async Task<FileSearchResult> SearchFilesParallelAsync(
        string sourceDirectoryPath, 
        string searchText, 
        string destinationFileName,
        ILogger? logger = null)
    {
        if (string.IsNullOrWhiteSpace(sourceDirectoryPath))
            throw new ArgumentException("Source directory path cannot be null or empty", nameof(sourceDirectoryPath));
            
        if (string.IsNullOrWhiteSpace(searchText))
            throw new ArgumentException("Search text cannot be null or empty", nameof(searchText));
            
        if (string.IsNullOrWhiteSpace(destinationFileName))
            throw new ArgumentException("Destination file name cannot be null or empty", nameof(destinationFileName));
    
        // Resolve and validate the search path
        var resolvedPath = ResolveCrossPlatformPath(sourceDirectoryPath, logger);
        if (!Directory.Exists(resolvedPath))
        {
            throw new DirectoryNotFoundException($"Directory not found: {sourceDirectoryPath} (resolved to: {resolvedPath})");
        }
        
        logger?.LogInformation("Starting file search in directory: {ResolvedPath}", resolvedPath);
    
        // Get all files in directory
        var files = Directory.GetFiles(resolvedPath, "*", SearchOption.AllDirectories)
            .Where(f => IsTextFile(f))
            .ToArray();
        
        logger?.LogInformation("Found {FileCount} text files to process", files.Length);
    
        // Thread-safe collections for results
        var matchingLines = new ConcurrentBag<string>();
        var processedFiles = 0;
        var totalOccurrences = 0;
        var linesWithMatches = 0;
    
        // Process files in parallel
        await Task.Run(() =>
        {
            Parallel.ForEach(files, file =>
            {
                try
                {
                    var fileResults = ProcessFile(file, searchText);
                    
                    // Add matching lines to collection
                    foreach (var line in fileResults.MatchingLines)
                    {
                        matchingLines.Add($"{Path.GetFileName(file)}: {line}");
                    }
                    
                    // Thread-safe increment
                    Interlocked.Increment(ref processedFiles);
                    Interlocked.Add(ref totalOccurrences, fileResults.OccurrenceCount);
                    Interlocked.Add(ref linesWithMatches, fileResults.MatchingLines.Count);
                }
                catch (Exception ex)
                {
                    // Log error but continue processing other files
                    logger?.LogWarning(ex, "Error processing file {FilePath}", file);
                }
            });
        });
    
        // Write results to destination file (handle output path resolution)
        var outputPath = ResolveOutputPath(destinationFileName, logger);
        var outputLines = matchingLines.OrderBy(line => line).ToArray();
        await File.WriteAllLinesAsync(outputPath, outputLines);
        
        logger?.LogInformation("File search completed. Results written to: {OutputPath}", outputPath);
    
        return new FileSearchResult
        {
            FilesProcessed = processedFiles,
            LinesWithMatches = linesWithMatches,
            TotalOccurrences = totalOccurrences,
            DestinationFile = outputPath
        };
    }
    
    /// <summary>
    /// Resolves cross-platform paths for both Windows development and Docker container environments
    /// </summary>
    public static string ResolveCrossPlatformPath(string inputPath, ILogger? logger = null)
    {
        logger?.LogInformation("Resolving path: {InputPath}", inputPath);
        
        // First, check if the path already exists as-is
        if (Directory.Exists(inputPath))
        {
            logger?.LogInformation("Path exists as-is: {InputPath}", inputPath);
            return inputPath;
        }
        
        // Check if we're in a Docker container
        var isDockerContainer = IsRunningInDockerContainer();
        logger?.LogInformation("Running in Docker container: {IsDocker}", isDockerContainer);
        
        if (isDockerContainer)
        {
            // In Docker, try mounted volume paths
            var dockerPaths = GetDockerSearchPaths(inputPath);
            foreach (var dockerPath in dockerPaths)
            {
                if (Directory.Exists(dockerPath))
                {
                    logger?.LogInformation("Found Docker mounted path: {DockerPath}", dockerPath);
                    return dockerPath;
                }
            }
        }
        else
        {
            // In Windows/PowerShell development environment
            var windowsPaths = GetWindowsSearchPaths(inputPath);
            foreach (var windowsPath in windowsPaths)
            {
                if (Directory.Exists(windowsPath))
                {
                    logger?.LogInformation("Found Windows path: {WindowsPath}", windowsPath);
                    return windowsPath;
                }
            }
        }
        
        // If no path found, return the original and let the caller handle the error
        logger?.LogWarning("No valid path found for: {InputPath}", inputPath);
        return inputPath;
    }
    
    /// <summary>
    /// Checks if the application is running inside a Docker container
    /// </summary>
    private static bool IsRunningInDockerContainer()
    {
        try
        {
            // Check for Docker-specific environment variables
            return Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != null ||
                   File.Exists("/.dockerenv") ||
                   Environment.GetEnvironmentVariable("DOCKER_CONTAINER") != null;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Gets potential Docker container paths for file searching
    /// </summary>
    private static IEnumerable<string> GetDockerSearchPaths(string inputPath)
    {
        var paths = new List<string>();
        
        // Check mounted Windows volume
        if (Directory.Exists("/app/search-files/windows"))
        {
            paths.Add("/app/search-files/windows");
            
            // If inputPath looks like a Windows path, try to map it
            if (inputPath.Contains("C:\\") || inputPath.Contains("c:\\"))
            {
                var relativePath = inputPath.Replace("C:\\", "").Replace("c:\\", "").Replace("\\", "/");
                paths.Add($"/app/search-files/windows/{relativePath}");
            }
        }
        
        // Check other common mounted paths
        var mountedPaths = new[]
        {
            "/app/search-files",
            "/app/data",
            "/tmp",
            "/var/tmp"
        };
        
        paths.AddRange(mountedPaths.Where(Directory.Exists));
        
        // If input looks like a relative path, try it under mounted directories
        if (!Path.IsPathRooted(inputPath))
        {
            foreach (var basePath in mountedPaths.Where(Directory.Exists))
            {
                paths.Add(Path.Combine(basePath, inputPath));
            }
        }
        
        return paths;
    }
    
    /// <summary>
    /// Gets potential Windows development environment paths
    /// </summary>
    private static IEnumerable<string> GetWindowsSearchPaths(string inputPath)
    {
        var paths = new List<string>();
        
        // Add the original path
        paths.Add(inputPath);
        
        // If it's a relative path, try common base directories
        if (!Path.IsPathRooted(inputPath))
        {
            var basePaths = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                @"C:\temp",
                @"C:\tmp",
                Directory.GetCurrentDirectory()
            };
            
            foreach (var basePath in basePaths.Where(Directory.Exists))
            {
                paths.Add(Path.Combine(basePath, inputPath));
            }
        }
        
        // Handle common Windows path variations
        if (inputPath.StartsWith("~"))
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            paths.Add(inputPath.Replace("~", userProfile));
        }
        
        return paths;
    }
    
    /// <summary>
    /// Resolves the output file path, ensuring it's writable
    /// </summary>
    private static string ResolveOutputPath(string fileName, ILogger? logger = null)
    {
        // If it's already a full path, use it
        if (Path.IsPathRooted(fileName))
        {
            return fileName;
        }
        
        // Try to write to current directory first
        var currentDir = Directory.GetCurrentDirectory();
        var currentDirPath = Path.Combine(currentDir, fileName);
        
        try
        {
            // Test if we can write to current directory
            var testPath = Path.Combine(currentDir, $"test-{Guid.NewGuid()}.tmp");
            File.WriteAllText(testPath, "test");  // Use synchronous version
            File.Delete(testPath);
            
            logger?.LogInformation("Using current directory for output: {OutputPath}", currentDirPath);
            return currentDirPath;
        }
        catch
        {
            logger?.LogWarning("Cannot write to current directory: {CurrentDir}", currentDir);
        }
        
        // Fall back to temp directory
        var tempPath = Path.Combine(Path.GetTempPath(), fileName);
        logger?.LogInformation("Using temp directory for output: {TempPath}", tempPath);
        return tempPath;
    }
    
    /// <summary>
    /// Gets available search paths for debugging/information purposes
    /// </summary>
    public static List<string> GetAvailableSearchPaths(ILogger? logger = null)
    {
        var availablePaths = new List<string>();
        var isDocker = IsRunningInDockerContainer();
        
        logger?.LogInformation("Getting available search paths. Docker container: {IsDocker}", isDocker);
        
        if (isDocker)
        {
            var dockerPaths = new[]
            {
                "/app/search-files",
                "/app/search-files/windows",
                "/app/data",
                "/tmp",
                "/var/tmp"
            };
            
            availablePaths.AddRange(dockerPaths.Where(Directory.Exists));
        }
        else
        {
            var windowsPaths = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                @"C:\temp",
                @"C:\tmp",
                Directory.GetCurrentDirectory()
            };
            
            availablePaths.AddRange(windowsPaths.Where(Directory.Exists));
        }
        
        logger?.LogInformation("Found {PathCount} available search paths", availablePaths.Count);
        return availablePaths;
    }
    
    /// <summary>
    /// Processes a single file and returns matching lines and occurrence count
    /// </summary>
    private static FileProcessResult ProcessFile(string filePath, string searchText)
    {
        var matchingLines = new List<string>();
        var occurrenceCount = 0;
        
        try
        {
            var lines = File.ReadAllLines(filePath);
            
            foreach (var line in lines)
            {
                var lineOccurrences = CountOccurrences(line, searchText);
                if (lineOccurrences > 0)
                {
                    matchingLines.Add(line);
                    occurrenceCount += lineOccurrences;
                }
            }
        }
        catch (Exception)
        {
            // Return empty result for files that can't be read
            return new FileProcessResult { MatchingLines = new List<string>(), OccurrenceCount = 0 };
        }
        
        return new FileProcessResult 
        { 
            MatchingLines = matchingLines, 
            OccurrenceCount = occurrenceCount 
        };
    }
    
    /// <summary>
    /// Counts occurrences of search text in a line (case-insensitive)
    /// </summary>
    private static int CountOccurrences(string line, string searchText)
    {
        if (string.IsNullOrEmpty(line) || string.IsNullOrEmpty(searchText))
            return 0;
            
        var count = 0;
        var index = 0;
        
        while ((index = line.IndexOf(searchText, index, StringComparison.OrdinalIgnoreCase)) != -1)
        {
            count++;
            index += searchText.Length;
        }
        
        return count;
    }
    
    /// <summary>
    /// Simple check to determine if file is likely a text file
    /// </summary>
    private static bool IsTextFile(string filePath)
    {
        var textExtensions = new[] { ".txt", ".cs", ".js", ".html", ".css", ".json", ".xml", ".md", ".log", ".config", ".yml", ".yaml", ".ini" };
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return textExtensions.Contains(extension);
    }
}

/// <summary>
/// Result of file search operation
/// </summary>
public record FileSearchResult
{
    public int FilesProcessed { get; init; }
    public int LinesWithMatches { get; init; }
    public int TotalOccurrences { get; init; }
    public string DestinationFile { get; init; } = string.Empty;
}

/// <summary>
/// Internal result for processing a single file
/// </summary>
internal record FileProcessResult
{
    public List<string> MatchingLines { get; init; } = new();
    public int OccurrenceCount { get; init; }
}