using System.Text;
using System.Text.RegularExpressions;

namespace AlexLee.Algorithms;

/// <summary>
/// Extension methods for string manipulation algorithms
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Problem #1: Interleaves two strings by alternating characters
    /// Example: "abc".InterleaveWith("123") returns "a1b2c3"
    /// </summary>
    /// <param name="source">First source string</param>
    /// <param name="other">Second source string</param>
    /// <returns>Interleaved string with alternating characters</returns>
    public static string InterleaveWith(this string source, string other)
    {
        // Handle null inputs
        source ??= string.Empty;
        other ??= string.Empty;
        
        var result = new StringBuilder();
        var maxLength = Math.Max(source.Length, other.Length);
        
        for (var i = 0; i < maxLength; i++)
        {
            // Add character from source if available
            if (i < source.Length)
                result.Append(source[i]);
                
            // Add character from other if available
            if (i < other.Length)
                result.Append(other[i]);
        }
        
        return result.ToString();
    }
    
    /// <summary>
    /// Problem #2: Checks if a string is a palindrome
    /// Ignores spaces, punctuation, and case sensitivity
    /// Works with both strings and numeric strings
    /// </summary>
    /// <param name="source">String to check</param>
    /// <returns>True if palindrome, false otherwise</returns>
    public static bool IsPalindrome(this string source)
    {
        if (string.IsNullOrEmpty(source))
            return true; // Empty string is considered palindrome
            
        // Remove non-alphanumeric characters and convert to lowercase
        var cleaned = Regex.Replace(source, @"[^a-zA-Z0-9]", "").ToLowerInvariant();
        
        // Check if cleaned string equals its reverse
        var reversed = new string(cleaned.Reverse().ToArray());
        return cleaned.Equals(reversed, StringComparison.OrdinalIgnoreCase);
    }
    
    /// <summary>
    /// Helper method that returns "Palindrome" or "Not Palindrome" as per requirements
    /// </summary>
    /// <param name="source">String to check</param>
    /// <returns>"Palindrome" or "Not Palindrome"</returns>
    public static string GetPalindromeResult(this string source)
    {
        return source.IsPalindrome() ? "Palindrome" : "Not Palindrome";
    }
}