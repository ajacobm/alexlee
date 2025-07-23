using Microsoft.AspNetCore.Mvc;
using AlexLee.Algorithms;

namespace AlexLee.Api.Controllers;

/// <summary>
/// API Controller for demonstrating C# algorithm implementations
/// Problems 1-3 from the Alex Lee Developer Exercise with enhanced file search
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AlgorithmsController : ControllerBase
{
    private readonly ILogger<AlgorithmsController> _logger;

    public AlgorithmsController(ILogger<AlgorithmsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Problem 1: String Interleaving
    /// Interleave two strings character by character
    /// Example: "abc" + "123" = "a1b2c3"
    /// </summary>
    /// <param name="request">String interleave parameters</param>
    /// <returns>Interleaved string result</returns>
    [HttpPost("string-interleave")]
    [ProducesResponseType(typeof(StringInterleaveResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<StringInterleaveResponse> InterleaveStrings([FromBody] StringInterleaveRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest("Request body is required");
            }

            var result = request.First.InterleaveWithLinq(request.Second);

            _logger.LogInformation("Interleaved with Linq Zip: '{First}' with '{Second}' = '{Result}'", 
                request.First ?? "null", request.Second ?? "null", result);

            return Ok(new StringInterleaveResponse
            {
                First = request.First ?? string.Empty,
                Second = request.Second ?? string.Empty,
                Result = result,
                Length = result.Length
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error interleaving strings: {@Request}", request);
            return BadRequest("Error processing string interleaving");
        }
    }

    /// <summary>
    /// Problem 2: Palindrome Check
    /// Check if a string is a palindrome (ignoring case, spaces, and punctuation)
    /// </summary>
    /// <param name="request">Palindrome check parameters</param>
    /// <returns>Palindrome check result</returns>
    [HttpPost("palindrome-check")]
    [ProducesResponseType(typeof(PalindromeCheckResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<PalindromeCheckResponse> CheckPalindrome([FromBody] PalindromeCheckRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest("Request body is required");
            }

            var result = request.Input.GetPalindromeResult();
            var isPalidrome = result == "Palindrome";

            _logger.LogInformation("Palindrome check for '{Input}': {Result}", 
                request.Input ?? "null", result);

            return Ok(new PalindromeCheckResponse
            {
                Input = request.Input ?? string.Empty,
                Result = result,
                IsPalindrome = isPalidrome,
                ProcessedInput = request.Input?.ToLowerInvariant()
                    .Where(char.IsLetterOrDigit)
                    .Aggregate(string.Empty, (acc, c) => acc + c) ?? string.Empty
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking palindrome: {@Request}", request);
            return BadRequest("Error processing palindrome check");
        }
    }

    /// <summary>
    /// Problem 3: Parallel File Search (Enhanced for cross-platform support)
    /// Search for text across all files in a directory with Windows/Docker support
    /// </summary>
    /// <param name="request">File search parameters</param>
    /// <returns>Search results including file count and occurrences</returns>
    [HttpPost("file-search")]
    [ProducesResponseType(typeof(FileSearchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FileSearchResponse>> SearchFiles([FromBody] FileSearchRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest("Request body is required");
            }

            if (string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                return BadRequest("Search term is required");
            }

            // Use actual directory path if provided, otherwise use default search paths
            var searchPath = request.DirectoryPath ?? GetDefaultSearchPath();
            var outputFileName = $"search-results-{DateTime.UtcNow:yyyyMMdd-HHmmss}.txt";

            var startTime = DateTime.UtcNow;
            
            try
            {
                var searchResult = await FileSearchUtilities.SearchFilesParallelAsync(
                    searchPath, 
                    request.SearchTerm, 
                    outputFileName,
                    _logger);

                var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

                _logger.LogInformation("File search completed for '{SearchTerm}' in {ProcessingTime}ms: {FileCount} files, {LineCount} lines, {OccurrenceCount} occurrences", 
                    request.SearchTerm, processingTime, searchResult.FilesProcessed, searchResult.LinesWithMatches, searchResult.TotalOccurrences);

                return Ok(new FileSearchResponse
                {
                    SearchTerm = request.SearchTerm,
                    DirectoryPath = searchPath,
                    FileCount = searchResult.FilesProcessed,
                    LineCount = searchResult.LinesWithMatches,
                    OccurrenceCount = searchResult.TotalOccurrences,
                    OutputFile = searchResult.DestinationFile,
                    SearchCompleted = true,
                    ProcessingTimeMs = (int)processingTime,
                    FilesSearched = FileSearchUtilities.GetAvailableSearchPaths(_logger)
                });
            }
            catch (DirectoryNotFoundException ex)
            {
                _logger.LogWarning("Directory not found for file search: {DirectoryPath}. Available paths: {AvailablePaths}", 
                    searchPath, string.Join(", ", FileSearchUtilities.GetAvailableSearchPaths(_logger)));

                return BadRequest(new
                {
                    error = "Directory not found",
                    requestedPath = searchPath,
                    availablePaths = FileSearchUtilities.GetAvailableSearchPaths(_logger),
                    message = "Use one of the available paths or mount the desired directory in Docker"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing file search: {@Request}", request);
            return BadRequest($"Error processing file search: {ex.Message}");
        }
    }

    /// <summary>
    /// Get available file search paths for cross-platform environments
    /// </summary>
    /// <returns>List of available search paths</returns>
    [HttpGet("file-search/available-paths")]
    [ProducesResponseType(typeof(AvailablePathsResponse), StatusCodes.Status200OK)]
    public ActionResult<AvailablePathsResponse> GetAvailableSearchPaths()
    {
        try
        {
            var paths = FileSearchUtilities.GetAvailableSearchPaths(_logger);
            var defaultPath = GetDefaultSearchPath();
            
            return Ok(new AvailablePathsResponse
            {
                AvailablePaths = paths,
                DefaultPath = defaultPath,
                IsDockerContainer = IsRunningInDockerContainer(),
                RecommendedPath = paths.FirstOrDefault() ?? defaultPath
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available search paths");
            return BadRequest("Error retrieving available search paths");
        }
    }

    /// <summary>
    /// Get algorithm demonstration information
    /// </summary>
    /// <returns>Information about available algorithms</returns>
    [HttpGet("info")]
    [ProducesResponseType(typeof(AlgorithmInfoResponse), StatusCodes.Status200OK)]
    public ActionResult<AlgorithmInfoResponse> GetAlgorithmInfo()
    {
        var info = new AlgorithmInfoResponse
        {
            Algorithms = new List<AlgorithmInfo>
            {
                new() {
                    Name = "String Interleaving",
                    Description = "Interleave two strings character by character",
                    Example = "Input: 'abc', '123' → Output: 'a1b2c3'",
                    Endpoint = "/api/algorithms/string-interleave"
                },
                new() {
                    Name = "Palindrome Check",
                    Description = "Check if a string is a palindrome (ignoring case, spaces, punctuation)",
                    Example = "Input: 'A man a plan a canal Panama' → Output: 'Palindrome'",
                    Endpoint = "/api/algorithms/palindrome-check"
                },
                new() {
                    Name = "Parallel File Search (Cross-Platform)",
                    Description = "Search for text across multiple files using parallel processing with Windows/Docker support",
                    Example = "Search for 'TODO' in all text files, supports mounted volumes",
                    Endpoint = "/api/algorithms/file-search"
                }
            },
            TotalAlgorithms = 3,
            Author = "Alex Lee Developer Exercise - SQL Server Express Edition"
        };

        return Ok(info);
    }

    private string GetDefaultSearchPath()
    {
        if (IsRunningInDockerContainer())
        {
            // In Docker, prefer mounted Windows directory, fall back to other mounted paths
            var dockerPaths = new[] { "/app/search-files/windows", "/app/search-files", "/app", "/tmp" };
            return dockerPaths.FirstOrDefault(Directory.Exists) ?? "/tmp";
        }
        else
        {
            // In Windows development, use user profile or temp
            var windowsPaths = new[] 
            { 
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                @"C:\temp",
                @"C:\tmp",
                Path.GetTempPath()
            };
            return windowsPaths.FirstOrDefault(Directory.Exists) ?? Path.GetTempPath();
        }
    }

    private bool IsRunningInDockerContainer()
    {
        try
        {
            return Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != null ||
                   System.IO.File.Exists("/.dockerenv") ||
                   Environment.GetEnvironmentVariable("DOCKER_CONTAINER") != null;
        }
        catch
        {
            return false;
        }
    }
}

// Request/Response DTOs
public record StringInterleaveRequest
{
    public string? First { get; init; }
    public string? Second { get; init; }
}

public record StringInterleaveResponse
{
    public string First { get; init; } = string.Empty;
    public string Second { get; init; } = string.Empty;
    public string Result { get; init; } = string.Empty;
    public int Length { get; init; }
}

public record PalindromeCheckRequest
{
    public string? Input { get; init; }
}

public record PalindromeCheckResponse
{
    public string Input { get; init; } = string.Empty;
    public string Result { get; init; } = string.Empty;
    public bool IsPalindrome { get; init; }
    public string ProcessedInput { get; init; } = string.Empty;
}

public record FileSearchRequest
{
    public string SearchTerm { get; init; } = string.Empty;
    public string? DirectoryPath { get; init; }
}

public record FileSearchResponse
{
    public string SearchTerm { get; init; } = string.Empty;
    public string DirectoryPath { get; init; } = string.Empty;
    public int FileCount { get; init; }
    public int LineCount { get; init; }
    public int OccurrenceCount { get; init; }
    public string OutputFile { get; init; } = string.Empty;
    public bool SearchCompleted { get; init; }
    public int ProcessingTimeMs { get; init; }
    public List<string> FilesSearched { get; init; } = new();
}

public record AvailablePathsResponse
{
    public List<string> AvailablePaths { get; init; } = new();
    public string DefaultPath { get; init; } = string.Empty;
    public bool IsDockerContainer { get; init; }
    public string RecommendedPath { get; init; } = string.Empty;
}

public record AlgorithmInfo
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Example { get; init; } = string.Empty;
    public string Endpoint { get; init; } = string.Empty;
}

public record AlgorithmInfoResponse
{
    public List<AlgorithmInfo> Algorithms { get; init; } = new();
    public int TotalAlgorithms { get; init; }
    public string Author { get; init; } = string.Empty;
}
