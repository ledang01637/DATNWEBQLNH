using System.IO;
using System.Security.Cryptography;
using System.Text;
using System;
using DATN.Server.Data;
using Microsoft.Extensions.Configuration;
using DATN.Shared;

namespace DATN.Server.Hash
{
    public class FileEncryptionService
    {
        private readonly IConfiguration _configuration;
        private readonly string _encryptionKey;

        public FileEncryptionService(IConfiguration configuration)
        {
            _configuration = configuration;
            _encryptionKey = _configuration.GetSection("KeySecurity").Get<string>();
        }

        public string Encrypt(string plainText)
        {
            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(_encryptionKey);
            aes.IV = new byte[16];

            using MemoryStream ms = new();
            using CryptoStream cs = new(ms, aes.CreateEncryptor(aes.Key, aes.IV), CryptoStreamMode.Write);
            using (StreamWriter sw = new(cs))
            {
                sw.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        public string Decrypt(string encryptedText)
        {
            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(_encryptionKey);
            aes.IV = new byte[16];

            using MemoryStream ms = new(Convert.FromBase64String(encryptedText));
            using CryptoStream cs = new(ms, aes.CreateDecryptor(aes.Key, aes.IV), CryptoStreamMode.Read);
            using StreamReader sr = new(cs);
            {
                return sr.ReadToEnd();
            }
        }
    }
}
