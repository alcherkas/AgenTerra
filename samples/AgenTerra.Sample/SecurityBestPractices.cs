using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AgenTerra.Sample.Security;

/// <summary>
/// Sample demonstrating secure coding practices that CodeQL security scanning validates.
/// This file serves as documentation and examples of security best practices.
/// </summary>
public class SecurityBestPractices
{
    /// <summary>
    /// Example: Secure random number generation
    /// ✓ Uses cryptographically secure RandomNumberGenerator instead of Random
    /// </summary>
    public static byte[] GenerateSecureToken(int length = 32)
    {
        byte[] token = new byte[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(token);
        }
        return token;
    }

    /// <summary>
    /// Example: Secure password hashing
    /// ✓ Uses industry-standard PBKDF2 with sufficient iterations
    /// ❌ Never store passwords in plain text or use weak hashing (MD5, SHA1)
    /// </summary>
    public static string HashPassword(string password, byte[] salt)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));
        
        if (salt == null || salt.Length < 16)
            throw new ArgumentException("Salt must be at least 16 bytes", nameof(salt));

        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations: 100000,
            HashAlgorithmName.SHA256,
            outputLength: 32);
        
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Example: Safe file path handling to prevent path traversal attacks
    /// ✓ Validates and sanitizes file paths
    /// ❌ Never concatenate user input directly into file paths
    /// </summary>
    public static string GetSafeFilePath(string baseDirectory, string userFileName)
    {
        if (string.IsNullOrEmpty(baseDirectory))
            throw new ArgumentException("Base directory cannot be null or empty", nameof(baseDirectory));
        
        if (string.IsNullOrEmpty(userFileName))
            throw new ArgumentException("File name cannot be null or empty", nameof(userFileName));

        // Remove any directory traversal attempts
        string fileName = Path.GetFileName(userFileName);
        
        // Combine paths safely
        string fullPath = Path.Combine(baseDirectory, fileName);
        
        // Validate the path is within the allowed directory
        string normalizedPath = Path.GetFullPath(fullPath);
        string normalizedBase = Path.GetFullPath(baseDirectory);
        
        if (!normalizedPath.StartsWith(normalizedBase, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Access to path outside base directory is not allowed");
        }
        
        return normalizedPath;
    }

    /// <summary>
    /// Example: Input validation to prevent injection attacks
    /// ✓ Validates and sanitizes user input
    /// </summary>
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // Use built-in validation where possible
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
