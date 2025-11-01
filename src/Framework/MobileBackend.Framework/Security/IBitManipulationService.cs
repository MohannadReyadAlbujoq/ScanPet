namespace MobileBackend.Framework.Security;

/// <summary>
/// Interface for bit manipulation encryption/decryption
/// Uses 4th bit flipping for obfuscation before RSA encryption
/// </summary>
public interface IBitManipulationService
{
    /// <summary>
    /// Encrypt data by flipping the 4th bit of each byte
    /// </summary>
    /// <param name="data">Data to encrypt</param>
    /// <returns>Encrypted data</returns>
    byte[] EncryptData(byte[] data);

    /// <summary>
    /// Decrypt data by flipping the 4th bit back
    /// (XOR is reversible, so same operation)
    /// </summary>
    /// <param name="data">Data to decrypt</param>
    /// <returns>Decrypted data</returns>
    byte[] DecryptData(byte[] data);

    /// <summary>
    /// Encrypt string data
    /// </summary>
    /// <param name="plainText">Plain text to encrypt</param>
    /// <returns>Base64 encoded encrypted data</returns>
    string EncryptString(string plainText);

    /// <summary>
    /// Decrypt string data
    /// </summary>
    /// <param name="encryptedBase64">Base64 encoded encrypted data</param>
    /// <returns>Decrypted plain text</returns>
    string DecryptString(string encryptedBase64);
}
