using System.Text;

namespace MobileBackend.Framework.Security;

/// <summary>
/// Implementation of bit manipulation encryption/decryption
/// Flips the 4th bit of each byte using XOR with 0x08 (binary: 00001000)
/// This is for obfuscation, not cryptographic security (use RSA for that)
/// </summary>
public class BitManipulationService : IBitManipulationService
{
    private const byte BitMask = 0x08; // Binary: 00001000 (4th bit)

    public byte[] EncryptData(byte[] data)
    {
        if (data == null || data.Length == 0)
            throw new ArgumentException("Data cannot be null or empty", nameof(data));

        byte[] encrypted = new byte[data.Length];
        
        for (int i = 0; i < data.Length; i++)
        {
            // XOR with 0x08 to flip the 4th bit
            encrypted[i] = (byte)(data[i] ^ BitMask);
        }

        return encrypted;
    }

    public byte[] DecryptData(byte[] data)
    {
        // XOR is reversible: (data XOR mask) XOR mask = data
        // So decryption is the same as encryption
        return EncryptData(data);
    }

    public string EncryptString(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentException("Plain text cannot be null or empty", nameof(plainText));

        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] encryptedBytes = EncryptData(plainBytes);
        return Convert.ToBase64String(encryptedBytes);
    }

    public string DecryptString(string encryptedBase64)
    {
        if (string.IsNullOrEmpty(encryptedBase64))
            throw new ArgumentException("Encrypted data cannot be null or empty", nameof(encryptedBase64));

        byte[] encryptedBytes = Convert.FromBase64String(encryptedBase64);
        byte[] decryptedBytes = DecryptData(encryptedBytes);
        return Encoding.UTF8.GetString(decryptedBytes);
    }
}
