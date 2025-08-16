namespace Dotnet.AzureDevOps.Mcp.Server.Security;

/// <summary>
/// Provides encryption and decryption services for sensitive data.
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encrypts sensitive data using AES encryption.
    /// </summary>
    /// <param name="plainText">The data to encrypt</param>
    /// <returns>Base64 encoded encrypted data</returns>
    string Encrypt(string plainText);

    /// <summary>
    /// Decrypts data that was encrypted using the Encrypt method.
    /// </summary>
    /// <param name="encryptedData">Base64 encoded encrypted data</param>
    /// <returns>Decrypted plain text</returns>
    string Decrypt(string encryptedData);

    /// <summary>
    /// Creates a secure hash of the input data using SHA256.
    /// </summary>
    /// <param name="input">The data to hash</param>
    /// <returns>Base64 encoded hash</returns>
    string CreateHash(string input);

    /// <summary>
    /// Verifies that input data matches the provided hash.
    /// </summary>
    /// <param name="input">The data to verify</param>
    /// <param name="hash">The hash to compare against</param>
    /// <returns>True if the input matches the hash</returns>
    bool VerifyHash(string input, string hash);

    /// <summary>
    /// Securely masks a Personal Access Token for logging purposes.
    /// </summary>
    /// <param name="pat">The PAT to mask</param>
    /// <returns>Masked PAT showing only first 4 and last 4 characters</returns>
    string MaskPersonalAccessToken(string pat);
}