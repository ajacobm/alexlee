using System.Collections.Concurrent;
using System.Text;

namespace AlexLee.Algorithms;

/// <summary>
/// File search utilities with parallel processing
/// </summary>
public static class FileSearchUtilities
{
    /// <summary>
    /// Problem #3: Searches for text in files within a directory using parallel processing
    /// </summary>
    /// <param name="sourceDirectoryPath">Directory to search in</param>
    /// <param name="searchText">Text to search for</param>
    /// <param name="destinationFileName">Output file name</param>
    /// <returns>Search results containing file count, line count, and occurrence count</returns>
    public static async Task<FileSearchResult> SearchFilesParallelAsync(
        string sourceDirectoryPath, 
        string searchText, 
        string destinationFileName)
    {
        if (string.IsNullOrWhiteSpace(sourceDirectoryPath))
            throw new ArgumentException("Source directory path cannot be null or empty", nameof(sourceDirectoryPath));
            
        if (string.IsNullOrWhiteSpace(searchText))
            throw new ArgumentException("Search text cannot be null or empty", nameof(searchText));
            
        if (string.IsNullOrWhiteSpace(destinationFileName))
            throw new ArgumentException("Destination file name cannot be null or empty", nameof(destinationFileName));
            
        if (!Directory.Exists(sourceDirectoryPath))
            throw new DirectoryNotFoundException($"Directory not found: {sourceDirectoryPath}");
    
        // Get all files in directory
        var files = Directory.GetFiles(sourceDirectoryPath, "*", SearchOption.AllDirectories)
            .Where(f => IsTextFile(f))
            .ToArray();
    
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
                    Console.WriteLine($"Error processing file {file}: {ex.Message}");
                }
            });
        });
    
        // Write results to destination file
        var outputLines = matchingLines.OrderBy(line => line).ToArray();
        await File.WriteAllLinesAsync(destinationFileName, outputLines);
    
        return new FileSearchResult
        {
            FilesProcessed = processedFiles,
            LinesWithMatches = linesWithMatches,
            TotalOccurrences = totalOccurrences,
            DestinationFile = destinationFileName
        };
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
        var textExtensions = new[] { ".txt", ".cs", ".js", ".html", ".css", ".json", ".xml", ".md", ".log" };
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