using Daedalin.Core.Enum;
using Daedalin.Core.OperationResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SymptomTracker.BLL
{
    internal class Cryptography
    {
        #region SerializeAndEncryptMessage
        public static async Task<string> SerializeAndEncryptMessage(object data)
        {
            return await Encrypt(JsonSerializer.Serialize(data));
        }
        #endregion

        #region Encrypt
        public static async Task<string> Encrypt(string data, bool IsFile = false)
        {
            var EncryptPassword = await SecureStorage.Default.GetAsync("EncryptPassword");
            if (EncryptPassword == null)
            {
                EncryptPassword = Guid.NewGuid().ToString().Replace("-", "");
                await SecureStorage.Default.SetAsync("EncryptPassword", EncryptPassword);
            }

            if (IsFile)
            {
                return EncryptFile(data, EncryptPassword);
            }
            else
            {
                var Base64 = Cryptography.Base64Encode(data);
                return EncryptString(Base64, EncryptPassword);
            }
        }
        #endregion

        #region DecryptAndDeserializeMessage
        public static async Task<OperatingResult<T>> DecryptAndDeserializeMessage<T>(string Titles)
        {
            var DataSting = await Decrypt(Titles);
            if (DataSting == null || !DataSting.Success)
            {
                return new OperatingResult<T>()
                {
                    ex = DataSting.ex,
                    Message = DataSting.Message,
                    Success = DataSting.Success,
                    Division = DataSting.Division,
                    MessageType = DataSting.MessageType,
                };
            }
            else if (string.IsNullOrEmpty(DataSting.Result))
                return OperatingResult<T>.OK(default);

            var Data = JsonSerializer.Deserialize<T>(DataSting.Result);

            return OperatingResult<T>.OK(Data);
        }
        #endregion

        #region Decrypt
        public static async Task<OperatingResult<string>> Decrypt(string Data, bool IsFile = false)
        {
            if (string.IsNullOrEmpty(Data))
                return OperatingResult<string>.OK(null);

            var EncryptPassword = await SecureStorage.Default.GetAsync("EncryptPassword");
            if (string.IsNullOrEmpty(EncryptPassword))
                return OperatingResult<string>.Fail("Es wurde Kein Key eingegeben.", eMessageType.Warning, nameof(DecryptAndDeserializeMessage));

            if (IsFile)
            {
                var outputFilePath = DecryptFile(Data, EncryptPassword);
                return OperatingResult<string>.OK(outputFilePath);
            }
            else
            {
                var Base64 = DecryptString(Data, EncryptPassword);
                var DataSting = Base64Decode(Base64);

                return OperatingResult<string>.OK(DataSting);
            }
        }
        #endregion

        #region Base64
        private static string Base64Encode(string str)
        {
            byte[] encbuff = Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(encbuff);
        }
        private static string Base64Decode(string str)
        {
            byte[] decbuff = Convert.FromBase64String(str);
            return Encoding.UTF8.GetString(decbuff);
        }
        #endregion

        #region EncryptFile
        private static string EncryptFile(string inputFilePath, string encryptionKey)
        {
            byte[] iv = new byte[16];
            var outputFilePath =  $"{inputFilePath}.encryption";
            var encryptionKeyBytes = Encoding.UTF8.GetBytes(encryptionKey);

            using (FileStream fsInput = new FileStream(inputFilePath, FileMode.Open))
            {
                using (FileStream fsOutput = new FileStream(outputFilePath, FileMode.Create))
                {
                    using (Aes aes = Aes.Create())
                    {
                        aes.Key = encryptionKeyBytes;
                        aes.IV = iv;

                        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                        using (CryptoStream cs = new CryptoStream(fsOutput, encryptor, CryptoStreamMode.Write))
                        {
                            fsInput.CopyTo(cs);
                        }
                    }
                }
            }
            return outputFilePath;
        }
        #endregion

        #region DecryptFile
        private static string DecryptFile(string inputFilePath, string encryptionKey)
        {
            byte[] iv = new byte[16];
            var outputFilePath = Path.Combine(FileSystem.CacheDirectory, Guid.NewGuid().ToString(), ".encryption");
            var encryptionKeyBytes = Encoding.UTF8.GetBytes(encryptionKey);

            using (FileStream fsInput = new FileStream(inputFilePath, FileMode.Open))
            {
                using (FileStream fsOutput = new FileStream(outputFilePath, FileMode.Create))
                {
                    using (Aes aes = Aes.Create())
                    {
                        aes.Key = encryptionKeyBytes;
                        aes.IV = iv;

                        // Perform decryption
                        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                        using (CryptoStream cs = new CryptoStream(fsOutput, decryptor, CryptoStreamMode.Write))
                        {
                            fsInput.CopyTo(cs);
                        }
                    }
                }
            }
            return outputFilePath;
        }
        #endregion

        #region EncryptString
        private static string EncryptString(string plainText, string encryptionKey)
        {
            var encryptionKeyBytes = Encoding.UTF8.GetBytes(encryptionKey);

            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = encryptionKeyBytes;
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new())
                {
                    using (CryptoStream cryptoStream = new((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }
        #endregion

        #region DecryptString
        private static string DecryptString(string cipherText, string encryptionKey)
        {
            var encryptionKeyBytes = Encoding.UTF8.GetBytes(encryptionKey);

            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = encryptionKeyBytes;
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new(buffer))
                {
                    using (CryptoStream cryptoStream = new((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
        #endregion
    }
}