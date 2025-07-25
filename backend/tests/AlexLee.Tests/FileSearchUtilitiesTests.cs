using AlexLee.Algorithms;

namespace AlexLee.Tests;

public class FileSearchUtilitiesTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly string _outputFile;
    
    public FileSearchUtilitiesTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}");
        _outputFile = Path.Combine(Path.GetTempPath(), $"output_{Guid.NewGuid():N}.txt");
        Directory.CreateDirectory(_testDirectory);
    }
    
    [Fact]
    public async Task SearchFilesParallelReduxAsync_WithValidInput_ShouldReturnCorrectResults()
    {
        // Arrange
        await File.WriteAllTextAsync(Path.Combine(_testDirectory, "file1.txt"), "Hello world\nThis is a test\nHello again");
        await File.WriteAllTextAsync(Path.Combine(_testDirectory, "file2.txt"), "Testing hello\nAnother line\nHello world test");
        await File.WriteAllTextAsync(Path.Combine(_testDirectory, "file3.cs"), "No match here\nJust code\nNothing relevant");
        
        // Act
        var result = await FileSearchUtilities.SearchFilesParallelReduxAsync(_testDirectory, "hello", _outputFile);
        
        // Assert
        Assert.True(result.FilesProcessed >= 2); // At least the text files should be processed
        Assert.True(result.LinesWithMatches >= 3); // At least 3 lines should match
        Assert.True(result.TotalOccurrences >= 4); // At least 4 occurrences of "hello"
        
        // Verify output file exists and contains results
        Assert.True(File.Exists(_outputFile));
        var outputLines = await File.ReadAllLinesAsync(_outputFile);
        Assert.True(outputLines.Length > 0);
    }
    
    [Fact]
    public async Task SearchFilesParallelReduxAsync_WithEmptyDirectory_ShouldReturnZeroResults()
    {
        // Act
        var result = await FileSearchUtilities.SearchFilesParallelReduxAsync(_testDirectory, "test", _outputFile);
        
        // Assert
        Assert.Equal(0, result.FilesProcessed);
        Assert.Equal(0, result.LinesWithMatches);
        Assert.Equal(0, result.TotalOccurrences);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task SearchFilesParallelReduxAsync_WithInvalidSearchText_ShouldThrowArgumentException(string searchText)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            FileSearchUtilities.SearchFilesParallelReduxAsync(_testDirectory, searchText, _outputFile));
    }
    
    [Fact]
    public async Task SearchFilesParallelReduxAsync_WithNonExistentDirectory_ShouldThrowDirectoryNotFoundException()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid():N}");
        
        // Act & Assert
        await Assert.ThrowsAsync<DirectoryNotFoundException>(() => 
            FileSearchUtilities.SearchFilesParallelReduxAsync(nonExistentPath, "test", _outputFile));
    }
    
    [Fact]
    public async Task SearchFilesParallelReduxAsync_WithCaseSensitivity_ShouldFindMatches()
    {
        // Arrange
        await File.WriteAllTextAsync(Path.Combine(_testDirectory, "case_test.txt"), "Hello\nhello\nHELLO\nworld");
        
        // Act
        var result = await FileSearchUtilities.SearchFilesParallelReduxAsync(_testDirectory, "hello", _outputFile);
        
        // Assert - Should find all 3 variations due to case-insensitive search
        Assert.True(result.TotalOccurrences >= 3);
        Assert.True(result.LinesWithMatches >= 3);
    }
    
    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDirectory))
                Directory.Delete(_testDirectory, true);
                
            if (File.Exists(_outputFile))
                File.Delete(_outputFile);
        }
        catch
        {
            // Ignore cleanup errors in tests
        }
    }
}