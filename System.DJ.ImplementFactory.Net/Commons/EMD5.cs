using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace System.DJ.ImplementFactory.Commons
{
    public class EMD5
    {
        private const string m_Key = "12345abc";
        private const int _size = 8;

        /// <summary>
        /// 用于对称算法的初始化向量（默认值）。
        /// </summary>
        private static readonly byte[] rgbIV = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };

        /// <summary>
        /// Encrypts a string
        /// </summary>
        /// <param name="encryptString">The string to be encrypted</param>
        /// <param name="key">The encryption key, which is required to be 8 bits</param>
        /// <returns>Successful encryption returns the encrypted string, and fails to return the source string</returns>
        public static string EncryptDES(string encryptString, string key)
        {
            try
            {
                if (null == encryptString || null == key) throw new Exception("The string or key to be encrypted cannot be null");
                if (_size != key.Length) throw new Exception("The length of the string key must be equal to 8");
                byte[] rgbKey = Encoding.UTF8.GetBytes(key);

                byte[] inputByteArray = Encoding.UTF8.GetBytes(encryptString);
                DESCryptoServiceProvider dCSP = new DESCryptoServiceProvider();
                MemoryStream mStream = new MemoryStream();
                CryptoStream cStream = new CryptoStream(mStream, dCSP.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
                cStream.Write(inputByteArray, 0, inputByteArray.Length);
                cStream.FlushFinalBlock();
                return Convert.ToBase64String(mStream.ToArray());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Encrypts a string
        /// </summary>
        /// <param name="encryptString">The string to be encrypted</param>
        /// <returns>Successful encryption returns the encrypted string, and fails to return the source string</returns>
        public static string EncryptDES(string encryptString)
        {
            return EncryptDES(encryptString, m_Key);
        }

        /// <summary>
        /// Decrypt the string
        /// </summary>
        /// <param name="decryptString">The string to be decrypted</param>
        /// <param name="key">Decryption key, requires 8 bits</param>
        /// <returns>Return decrypt the string</returns>
        public static string DecryptDES(string decryptString, string key)
        {
            try
            {
                byte[] rgbKey = Encoding.UTF8.GetBytes(key);
                byte[] inputByteArray = Convert.FromBase64String(decryptString);
                DESCryptoServiceProvider DCSP = new DESCryptoServiceProvider();
                MemoryStream mStream = new MemoryStream();
                CryptoStream cStream = new CryptoStream(mStream, DCSP.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
                cStream.Write(inputByteArray, 0, inputByteArray.Length);
                cStream.FlushFinalBlock();
                return Encoding.UTF8.GetString(mStream.ToArray());
            }
            catch
            {
                return decryptString;
            }
        }

        /// <summary>
        /// Decrypt the string
        /// </summary>
        /// <param name="decryptString">The string to be decrypted</param>
        /// <returns>Return decrypt the string</returns>
        public static string DecryptDES(string decryptString)
        {
            return DecryptDES(decryptString, m_Key);
        }
    }
}
