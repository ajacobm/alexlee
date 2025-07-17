using Microsoft.AspNetCore.Mvc;
using AlexLee.Algorithms;

namespace AlexLee.Api.Controllers;

/// <summary>
/// API Controller for demonstrating C# algorithm implementations
/// Problems 1-3 from the Alex Lee Developer Exercise
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
    /// <param name="first">First string to interleave</param>
    /// <param name="second">Second string to interleave</param>
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

            var result = request.First.InterleaveWith(request.Second);

            _logger.LogInformation("Interleaved '{First}' with '{Second}' = '{Result}'", 
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
    /// <param name="input">String to check for palindrome</param>
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
    /// Problem 3: Parallel File Search
    /// Search for a string across all files in a directory (demo version with file list)
    /// </summary>
    /// <param name="request">File search parameters</param>
    /// <returns>Search results including file count and occurrences</returns>
    [HttpPost("file-search")]
    [ProducesResponseType(typeof(FileSearchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<FileSearchResponse> SearchFiles([FromBody] FileSearchRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest("Request body is required");
            }

            // For API demo, we'll create a simulated search result
            // In a real application, this would use DirectorySearchUtility.SearchFiles
            var simulatedResults = SimulateFileSearch(request.SearchTerm);

            _logger.LogInformation("File search for '{SearchTerm}' found {FileCount} files with {LineCount} total lines and {OccurrenceCount} occurrences", 
                request.SearchTerm, simulatedResults.FileCount, simulatedResults.LineCount, simulatedResults.OccurrenceCount);

            return Ok(simulatedResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing file search: {@Request}", request);
            return BadRequest("Error processing file search");
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
                    Name = "Parallel File Search",
                    Description = "Search for text across multiple files using parallel processing",
                    Example = "Search for 'TODO' in all .cs files",
                    Endpoint = "/api/algorithms/file-search"
                }
            },
            TotalAlgorithms = 3,
            Author = "Alex Lee Developer Exercise"
        };

        return Ok(info);
    }

    private FileSearchResponse SimulateFileSearch(string searchTerm)
    {
        // Simulate realistic file search results for demo purposes
        var random = new Random();
        var fileCount = random.Next(5, 20);
        var lineCount = random.Next(100, 1000);
        var occurrenceCount = string.IsNullOrEmpty(searchTerm) ? 0 : random.Next(1, 50);

        return new FileSearchResponse
        {
            SearchTerm = searchTerm,
            FileCount = fileCount,
            LineCount = lineCount,
            OccurrenceCount = occurrenceCount,
            SearchCompleted = true,
            ProcessingTimeMs = random.Next(10, 500),
            FilesSearched = GenerateSimulatedFileList(fileCount)
        };
    }

    private List<string> GenerateSimulatedFileList(int count)
    {
        var fileExtensions = new[] { ".cs", ".txt", ".json", ".xml", ".md" };
        var fileNames = new[] { "Program", "Controller", "Service", "Model", "Helper", "Utility", "Test" };
        var files = new List<string>();

        var random = new Random();
        for (int i = 0; i < count; i++)
        {
            var name = fileNames[random.Next(fileNames.Length)];
            var ext = fileExtensions[random.Next(fileExtensions.Length)];
            files.Add($"/path/to/files/{name}{i + 1}{ext}");
        }

        return files;
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
    public int FileCount { get; init; }
    public int LineCount { get; init; }
    public int OccurrenceCount { get; init; }
    public bool SearchCompleted { get; init; }
    public int ProcessingTimeMs { get; init; }
    public List<string> FilesSearched { get; init; } = new();
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
