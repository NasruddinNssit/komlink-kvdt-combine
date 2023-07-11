using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace kvdt_kiosk.Services.Komlink
{
    public class APISignatureServices
    {
        public string hashSecretKey = "b7edee98eb074d8eb67b8b20f5a3ab13";
        public string TVMKey = "9eksd92ks9378qwjs92ks92ls02ls02l";


        public string HMACSHA512(string text, string secretkey)
        {
            byte[] keyInBytes = Encoding.UTF8.GetBytes(secretkey);
            byte[] payloadInBytes = Encoding.UTF8.GetBytes(text);

            var md5 = new HMACSHA512(keyInBytes);
            byte[] hash = md5.ComputeHash(payloadInBytes);

            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }

        public string AESEncrypt(string toEncrypt, string key)
        {
            bool checkHashValue = true;

            if (checkHashValue == true)
            {
                toEncrypt = toEncrypt + "\r\n\r\n\r\n" + HMACSHA512(toEncrypt, hashSecretKey);
            }

            byte[] encrypted;
            byte[] IV;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.KeySize = 256;
                aesAlg.BlockSize = 128;

                aesAlg.Key = UTF8Encoding.UTF8.GetBytes(key);

                aesAlg.GenerateIV();
                IV = aesAlg.IV;

                aesAlg.Mode = CipherMode.CBC;

                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption. 
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(toEncrypt);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            var combinedIvCt = new byte[IV.Length + encrypted.Length];
            Array.Copy(IV, 0, combinedIvCt, 0, IV.Length);
            Array.Copy(encrypted, 0, combinedIvCt, IV.Length, encrypted.Length);

            // Return the encrypted bytes from the memory stream. 

            return Convert.ToBase64String(combinedIvCt);
        }


        public string GetSignature()
        {
            var ToIso8601DateTime = DateTime.UtcNow.ToUniversalTime();
            var iso8601DateTime = ToIso8601DateTime.ToString("yyyy-MM-ddTHH:mm:ss");

            //return signature
            var signature = AESEncrypt(iso8601DateTime, TVMKey);
            return signature;

        }
    }
}
