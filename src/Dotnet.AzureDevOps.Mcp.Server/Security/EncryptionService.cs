using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace Dotnet.AzureDevOps.Mcp.Server.Security;

/// <summary>
/// Configuration options for encryption services.
/// </summary>
public class EncryptionOptions
{
    public const string SectionName = "Encryption";

    /// <summary>
    /// Base64 encoded encryption key. If not provided, a key will be generated.
    /// WARNING: In production, this should be stored securely (Azure Key Vault, etc.)
    /// </summary>
    public string? EncryptionKey { get; set; }
}

/// <summary>
/// Implementation of encryption services using AES encryption for sensitive data protection.
/// </summary>
public class EncryptionService : IEncryptionService, IDisposable
{
    private readonly ILogger<EncryptionService> _logger;
    private readonly byte[] _encryptionKey;
    private bool _disposed;

    public EncryptionService(ILogger<EncryptionService> logger, IOptions<EncryptionOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        EncryptionOptions config = options?.Value ?? new EncryptionOptions();

        // Initialize encryption key
        if(!string.IsNullOrEmpty(config.EncryptionKey))
        {
            try
            {
                _encryptionKey = Convert.FromBase64String(config.EncryptionKey);
                if(_encryptionKey.Length != 32) // 256-bit key
                {
                    throw new ArgumentException("Encryption key must be 256 bits (32 bytes)");
                }
                _logger.LogInformation("Using provided encryption key");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Invalid encryption key provided, generating new key");
                _encryptionKey = GenerateKey();
            }
        }
        else
        {
            _encryptionKey = GenerateKey();
            _logger.LogWarning("No encryption key provided, generated new key. This should not happen in production!");
        }
    }

    /// <summary>
    /// Finalizer to ensure resources are cleaned up if Dispose is not called explicitly.
    /// </summary>
    ~EncryptionService()
    {
        Dispose(disposing: false);
    }

    public string Encrypt(string plainText)
    {
        ThrowIfDisposed();

        if(string.IsNullOrEmpty(plainText))
            return string.Empty;

        try
        {
            using Aes aes = Aes.Create();
            aes.Key = _encryptionKey;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateIV();

            using ICryptoTransform encryptor = aes.CreateEncryptor();
            using MemoryStream memoryStream = new MemoryStream();

            // Write IV to the beginning of the stream
            memoryStream.Write(aes.IV, 0, aes.IV.Length);

            using(CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            using(StreamWriter writer = new StreamWriter(cryptoStream))
            {
                writer.Write(plainText);
            }

            byte[] encrypted = memoryStream.ToArray();
            return Convert.ToBase64String(encrypted);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error encrypting data");
            throw new InvalidOperationException("Failed to encrypt data", ex);
        }
    }

    public string Decrypt(string encryptedData)
    {
        ThrowIfDisposed();

        if(string.IsNullOrEmpty(encryptedData))
            return string.Empty;

        try
        {
            byte[] encrypted = Convert.FromBase64String(encryptedData);

            using Aes aes = Aes.Create();
            aes.Key = _encryptionKey;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Extract IV from the beginning of the encrypted data
            byte[] iv = new byte[16]; // AES block size
            Array.Copy(encrypted, 0, iv, 0, iv.Length);
            aes.IV = iv;

            using ICryptoTransform decryptor = aes.CreateDecryptor();
            using MemoryStream memoryStream = new MemoryStream(encrypted, iv.Length, encrypted.Length - iv.Length);
            using CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using StreamReader reader = new StreamReader(cryptoStream);

            return reader.ReadToEnd();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error decrypting data");
            throw new InvalidOperationException("Failed to decrypt data", ex);
        }
    }

    public string CreateHash(string input)
    {
        ThrowIfDisposed();

        if(string.IsNullOrEmpty(input))
            return string.Empty;

        try
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(hashBytes);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error creating hash");
            throw new InvalidOperationException("Failed to create hash", ex);
        }
    }

    public bool VerifyHash(string input, string hash)
    {
        ThrowIfDisposed();

        if(string.IsNullOrEmpty(input) || string.IsNullOrEmpty(hash))
            return false;

        try
        {
            string inputHash = CreateHash(input);
            return string.Equals(inputHash, hash, StringComparison.Ordinal);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error verifying hash");
            return false;
        }
    }

    public string MaskPersonalAccessToken(string pat)
    {
        ThrowIfDisposed();

        if(string.IsNullOrEmpty(pat))
            return "[EMPTY]";

        if(pat.Length < 8)
            return "[INVALID]";

        // Show first 4 and last 4 characters, mask the rest
        string masked = pat[..4] + new string('*', pat.Length - 8) + pat[^4..];
        return masked;
    }

    private static byte[] GenerateKey()
    {
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        byte[] key = new byte[32]; // 256-bit key
        rng.GetBytes(key);
        return key;
    }

    /// <summary>
    /// Throws ObjectDisposedException if the service has been disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
        if(_disposed)
        {
            throw new ObjectDisposedException(nameof(EncryptionService));
        }
    }

    /// <summary>
    /// Protected virtual dispose method to handle both managed and unmanaged resources.
    /// </summary>
    /// <param name="disposing">True if disposing managed resources; false if called from finalizer</param>
    protected virtual void Dispose(bool disposing)
    {
        if(!_disposed)
        {
            if(disposing)
            {
                // Dispose managed resources here if any
                // None in this case as all cryptographic objects are used with 'using' statements
            }

            // Clear sensitive data from memory (unmanaged-like cleanup)
            if(_encryptionKey != null)
            {
                Array.Clear(_encryptionKey, 0, _encryptionKey.Length);
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Public dispose method following the standard IDisposable pattern.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}