using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using System.Linq;

namespace Project.Helpers
{
    public class PasswordResetCodeHelper
    {
        private readonly byte[] _encryptionKey;
        private const int EXPIRATION_TIME = 5;
        private static readonly ConcurrentDictionary<string, bool> _usedCodes = new();

        public PasswordResetCodeHelper(IConfiguration configuration)
        {
            var key = configuration["PasswordReset:EncryptionKey"] ?? throw new ArgumentNullException("EncryptionKey configuration is missing");
            _encryptionKey = Encoding.UTF8.GetBytes(key);
        }

        public string GenerateResetCode(string userId)
        {
            var expirationTime = DateTime.UtcNow.AddHours(EXPIRATION_TIME);
            string payload = $"{userId}|{expirationTime.Ticks}|{DateTime.UtcNow.Ticks}";

            using (var hmac = new HMACSHA256(_encryptionKey))
            {
                byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
                byte[] hashBytes = hmac.ComputeHash(payloadBytes);

                // Combine payload and hash, then convert to URL-safe base64
                byte[] combined = new byte[payloadBytes.Length + hashBytes.Length];
                Buffer.BlockCopy(payloadBytes, 0, combined, 0, payloadBytes.Length);
                Buffer.BlockCopy(hashBytes, 0, combined, payloadBytes.Length, hashBytes.Length);

                return Convert.ToBase64String(combined)
                    .TrimEnd('=')
                    .Replace('+', '-')
                    .Replace('/', '_');
            }
        }

        public (bool isValid, string? userId, string message) ValidateResetCode(string code)
        {
            try
            {
                if (string.IsNullOrEmpty(code))
                {
                    return (false, null, "Invalid code");
                }

                // Kiểm tra xem code đã được sử dụng chưa
                if (_usedCodes.ContainsKey(code))
                {
                    return (false, null, "This reset code has already been used");
                }

                // Convert from URL-safe base64
                string base64 = code.Replace('-', '+').Replace('_', '/');
                switch (base64.Length % 4)
                {
                    case 2: base64 += "=="; break;
                    case 3: base64 += "="; break;
                }

                byte[] combined = Convert.FromBase64String(base64);
                if (combined.Length <= 32) // 32 is HMACSHA256 hash length
                {
                    return (false, null, "Invalid code format");
                }

                // Split payload and hash
                int payloadLength = combined.Length - 32;
                byte[] payloadBytes = new byte[payloadLength];
                byte[] hashBytes = new byte[32];
                Buffer.BlockCopy(combined, 0, payloadBytes, 0, payloadLength);
                Buffer.BlockCopy(combined, payloadLength, hashBytes, 0, 32);

                // Verify hash
                using (var hmac = new HMACSHA256(_encryptionKey))
                {
                    byte[] computedHash = hmac.ComputeHash(payloadBytes);
                    if (!ByteArraysEqual(hashBytes, computedHash))
                    {
                        return (false, null, "Invalid code");
                    }
                }

                // Parse payload
                string payload = Encoding.UTF8.GetString(payloadBytes);
                var parts = payload.Split('|');
                if (parts.Length != 3)
                {
                    return (false, null, "Invalid code format");
                }

                string userId = parts[0];
                if (!long.TryParse(parts[1], out long expirationTicks))
                {
                    return (false, null, "Invalid code format");
                }

                var expirationTime = new DateTime(expirationTicks);
                if (DateTime.UtcNow > expirationTime)
                {
                    return (false, null, "This reset code has expired");
                }

                return (true, userId, "Valid code");
            }
            catch
            {
                return (false, null, "Invalid code");
            }
        }

        public void MarkCodeAsUsed(string code)
        {
            _usedCodes.TryAdd(code, true);

            // Cleanup expired codes from memory periodically
            if (_usedCodes.Count > 1000) // Arbitrary threshold
            {
                var keysToRemove = _usedCodes.Keys
                    .Where(k =>
                    {
                        try
                        {
                            var (isValid, _, _) = ValidateResetCode(k);
                            return !isValid; // Remove if code is invalid or expired
                        }
                        catch
                        {
                            return true; // Remove if any error occurs
                        }
                    })
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    _usedCodes.TryRemove(key, out _);
                }
            }
        }

        private static bool ByteArraysEqual(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }
    }
}