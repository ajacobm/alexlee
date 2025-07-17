using AlexLee.Algorithms;

namespace AlexLee.Tests;

public class StringExtensionsTests
{
    [Theory]
    [InlineData("abc", "123", "a1b2c3")]
    [InlineData("hello", "world", "hweolrllod")]
    [InlineData("a", "1234", "a1234")]
    [InlineData("abcd", "1", "a1bcd")]
    [InlineData("", "123", "123")]
    [InlineData("abc", "", "abc")]
    [InlineData("", "", "")]
    public void InterleaveWith_ShouldReturnCorrectResult(string source, string other, string expected)
    {
        // Act
        var result = source.InterleaveWith(other);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public void InterleaveWith_WithNullInputs_ShouldHandleGracefully()
    {
        // Arrange
        string nullString = null!;
        
        // Act & Assert
        Assert.Equal("abc", "abc".InterleaveWith(nullString));
        Assert.Equal("123", nullString.InterleaveWith("123"));
    }
    
    [Theory]
    [InlineData("madam", true)]
    [InlineData("step on no pets", true)]
    [InlineData("1221", true)]
    [InlineData("A man, a plan, a canal: Panama", true)]
    [InlineData("book", false)]
    [InlineData("hello", false)]
    [InlineData("123", false)]
    [InlineData("", true)] // Empty string is palindrome
    public void IsPalindrome_ShouldReturnCorrectResult(string input, bool expected)
    {
        // Act
        var result = input.IsPalindrome();
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("madam", "Palindrome")]
    [InlineData("step on no pets", "Palindrome")]
    [InlineData("1221", "Palindrome")]
    [InlineData("book", "Not Palindrome")]
    public void GetPalindromeResult_ShouldReturnCorrectMessage(string input, string expected)
    {
        // Act
        var result = input.GetPalindromeResult();
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public void IsPalindrome_WithNullInput_ShouldReturnTrue()
    {
        // Arrange
        string nullString = null!;
        
        // Act
        var result = nullString.IsPalindrome();
        
        // Assert
        Assert.True(result);
    }
}