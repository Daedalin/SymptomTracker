using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SymptomTracker.BLL
{
    internal class Encrypt
    {
        public static string Base64Encode(string str)
        {
            byte[] encbuff = System.Text.Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(encbuff);
        }
        public static string Base64Decode(string str)
        {
            byte[] decbuff = Convert.FromBase64String(str);
            return System.Text.Encoding.UTF8.GetString(decbuff);
        }


        /// <summary>
        /// Diese Methode verschlüsselt den mitgegebenen String
        /// </summary>
        /// <param name="plainMessage">Zu verschlüsselnder String</param>
        /// <param name="password">Passwort mit dem der String verschlüsselt werden soll</param>
        /// <returns>Den verschlüsselten String</returns>
        public static string EncryptMessage(string plainMessage, string password)
        {
            TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();
            des.IV = new byte[8];
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(password, new byte[0]);
            des.Key = pdb.CryptDeriveKey("RC2", "MD5", 128, new byte[8]);
            MemoryStream ms = new MemoryStream(plainMessage.Length * 2);
            CryptoStream encStream = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainMessage);
            encStream.Write(plainBytes, 0, plainBytes.Length);
            encStream.FlushFinalBlock();
            byte[] encryptedBytes = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(encryptedBytes, 0, (int)ms.Length);
            encStream.Close();
            return Convert.ToBase64String(encryptedBytes);
        }

        /// <summary>
        /// Diese Methode entschlüsslt den mitgegebenen String
        /// </summary>
        /// <param name="encryptedBase64">zu entschlüsselnder string</param>
        /// <param name="password">Passwort mit dem der String verschlüsselt wurde</param>
        /// <returns>Den entschlüsslten string</returns>
        public static string DecryptMessage(string encryptedBase64, string password)
        {
            TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();
            des.IV = new byte[8];
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(password, new byte[0]);
            des.Key = pdb.CryptDeriveKey("RC2", "MD5", 128, new byte[8]);
            byte[] encryptedBytes = Convert.FromBase64String(encryptedBase64);
            MemoryStream ms = new MemoryStream(encryptedBase64.Length);
            CryptoStream decStream = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            decStream.Write(encryptedBytes, 0, encryptedBytes.Length);
            decStream.FlushFinalBlock();
            byte[] plainBytes = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(plainBytes, 0, (int)ms.Length);
            decStream.Close();
            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}
