using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace EncryptedType
{
    public static class EncryptedTypeExtensions
    {
        private static object __sharedKeyCacheLock = new object();
        private static IDictionary<string, KeyInfo> _sharedKeyCache;

        // Encryped Data Format: "<KeyID>~<CrypterID>~<HMACID>~<IV>~<Data>~<HMAC>"
        public static string Encrypt(this string Value, string KeyID, IEnumerable<IKeyServer> KeyServers, Func<string> Integrity)
        {
            return Encrypt(Value, KeyID, KeyServers, Integrity, Constants.HS256);
        }

        public static string Encrypt(this string Value, string KeyID, IEnumerable<IKeyServer> KeyServers, Func<string> Integrity, string HMACID)
        {
            return Encrypt(Value, KeyID, KeyServers, Integrity, HMACID, Constants.AES);
        }

        public static string Encrypt(this string Value, string KeyID, IEnumerable<IKeyServer> KeyServers, Func<string> Integrity, string HMACID, string CrypterID)
        {
            if(null == Value)
            {
                return null;
            }
            if(null != Integrity)
            {
                Value = AddHMAC(HMACID, Value, Integrity);
            }
            byte[] val = Encoding.UTF8.GetBytes(Value);
            SymmetricAlgorithm crypter = GetCrypter(CrypterID);
            var iv = new byte[crypter.BlockSize / 8].FillWithEntropy();
            KeyInfo key = GetKeyInfo(KeyServers, KeyID, iv, crypter);
            return string.Format("{0}~{1}~{2}",KeyID,CrypterID,Encrypt(HMACID, val, iv, key.KeyBytes, key.SecretBytes, crypter));
        }

        private static string AddHMAC(string HMACID,string Data, Func<string> Integrity)
        {
            var retVal = Data;
            retVal = string.Format("{0}\0{1}", Data, ComputeHMAC(HMACID,Data, Integrity));
            return retVal;
        }

        private static string Encrypt(string HMACID,byte[] val, byte[] iv, byte[] key, byte[] secret, SymmetricAlgorithm crypter)
        {
            byte[] encrypted;
            crypter.IV = iv;
            crypter.Key = key;
            crypter.Mode = CipherMode.CBC;
            using (var encrypter = crypter.CreateEncryptor())
            {
                using (var to = new MemoryStream())
                {
                    using (var writer = new CryptoStream(to, encrypter, CryptoStreamMode.Write))
                    {
                        writer.Write(val, 0, val.Length);
                        writer.FlushFinalBlock();
                        encrypted = to.ToArray();
                    }
                }
            }
            return string.Format("{0}~{1}~{2}~{3}", HMACID, Convert.ToBase64String(iv), Convert.ToBase64String(encrypted), ComputeHMAC(HMACID,encrypted, secret));
        }

        private static KeyInfo GetKeyInfo(IEnumerable<IKeyServer> KeyServers, string KeyName, byte[] IV, SymmetricAlgorithm Crypter)
        {
            if (null == _sharedKeyCache)
                lock (__sharedKeyCacheLock)
                {
                    if (null == _sharedKeyCache)
                        _sharedKeyCache = new ConcurrentDictionary<string, KeyInfo>();
                }
            if (_sharedKeyCache.ContainsKey(KeyName))
                return _sharedKeyCache[KeyName];
            string keyValue = null;
            KeyInfo retVal = new KeyInfo();
            if(null == KeyServers || KeyServers.Count() < 1)
            {
                return new KeyInfo();
            }
            foreach(var s in KeyServers)
            {
                keyValue = s.GetKey(KeyName);
                if(null != keyValue)
                {
                    break;
                }
            }
            if (null != keyValue)
            {
                retVal.KeyValue = keyValue;
                int keySize = Crypter.KeySize;
                byte[] workingBytes = new Rfc2898DeriveBytes(keyValue, IV).GetBytes(keySize / 4);
                retVal.KeyBytes = workingBytes.Take(keySize / 8).ToArray();
                retVal.SecretBytes = workingBytes.Skip(keySize / 8).Take(keySize / 8).ToArray();
                //if(!KeyCache.ContainsKey(KeyName))
                //    lock(_sharedKeyCacheLock)
                //    {
                //        if (!KeyCache.ContainsKey(KeyName))
                //            KeyCache.Add(KeyName, retVal);
                //    }
            }
            return retVal;
        }

        private static SymmetricAlgorithm GetCrypter(string CrypterID)
        {
            SymmetricAlgorithm crypter = null;
            switch (CrypterID.ToUpperInvariant())
            {
                case Constants.AES:
                    crypter = new AesCryptoServiceProvider();
                    break;
            }
            return crypter;
        }

        public static string Decrypt(this string Data, IEnumerable<IKeyServer> KeyServers, Func<string> IntegrityFunction)
        {
            var vals = Data.Split('~');
            if(vals.Length < 6)
            {
                // do something here to ensure constant time compare
                return null;
            }
            string keyID = vals[0];
            string crypterID = vals[1];
            string HMACID = vals[2];
            var iv = Convert.FromBase64String(vals[3]);
            var encrypted = Convert.FromBase64String(vals[4]);
            var mac = vals[5];
            SymmetricAlgorithm crypter = GetCrypter(crypterID);
            KeyInfo key = GetKeyInfo(KeyServers, keyID, iv, crypter);
            string retVal = Decrypt(HMACID,encrypted, mac, iv, key.KeyBytes, key.SecretBytes, crypter);
            if(retVal.IndexOf('\0') > 0 && null != IntegrityFunction)
            {
                // if there is integrity in the data and no integrity value we do not return data
                return null;
            }
            if (null != IntegrityFunction)
            {
                var values = retVal.Split('\0');
                if (values.Length < 2)
                {
                    retVal = null;
                }
                if(string.IsNullOrEmpty(values[0]) && string.IsNullOrEmpty(values[1]))
                {
                    return string.Empty;
                }
                if (null != retVal && VerifyHMAC(HMACID,values[0], values[1],IntegrityFunction))
                {
                    retVal = values[0];
                }
                else
                {
                    retVal = null;
                }
            }
            return retVal;
        }

        private static bool VerifyHMAC(string HMACID, string Data, string TestAgainst, Func<string> Integrity)
        {
            try
            {
                return TestAgainst.ConstantTimeCompare(ComputeHMAC(HMACID, Data, Integrity));
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static string Decrypt(string HMACID, byte[] encrypted, string mac, byte[] iv, byte[] key, byte[] secret, SymmetricAlgorithm crypter)
        {
            if (mac.ConstantTimeCompare(ComputeHMAC(HMACID,encrypted, secret)))
            {
                byte[] decrypted;
                int decryptedByteCount = 0;
                crypter.IV = iv;
                crypter.Key = key;
                crypter.Mode = CipherMode.CBC;
                using (var cipher = crypter.CreateDecryptor())
                {
                    using (var from = new MemoryStream(encrypted))
                    {
                        using (var reader = new CryptoStream(from, cipher, CryptoStreamMode.Read))
                        {
                            decrypted = new byte[encrypted.Length];
                            decryptedByteCount = reader.Read(decrypted, 0, decrypted.Length);
                        }
                    }
                }
                return Encoding.UTF8.GetString(decrypted, 0, decryptedByteCount);
            }
            return null;
        }

        private static string ComputeHMAC(string HMACID, string Data, Func<string> Integrity)
        {
            return ComputeHMAC(HMACID, Encoding.UTF8.GetBytes(Data), Encoding.UTF8.GetBytes(Integrity.Invoke()));
        }

        private static string ComputeHMAC(string HMACID, byte[] Data, byte[] Key)
        {
            if (null == Data || null == Key || Data.Length < 1 || Key.Length < 1)
            {
                return null;
            }
            HMAC hmac = null;
            switch (HMACID.ToUpperInvariant())
            {
                case Constants.HS256:
                    hmac = new HMACSHA256() { Key = Key };
                    break;
                default:
                    return null;
            }
            if (null != hmac)
            {
                return Convert.ToBase64String(hmac.ComputeHash(Data));
            }
            return null;
        }

    }
}

namespace System
{
    public static class SystemExtensions
    {
        public static bool ConstantTimeCompare(this string First, string Second)
        {
            bool result = true;
            if (First.Length != Second.Length)
            {
                result = false;
            }
            int max = First.Length > Second.Length ? Second.Length : First.Length;
            for (int i = 0; i < max; i++)
            {
                if (First[i] != Second[i])
                {
                    result = false;
                }
            }
            return result;
        }

        public static byte[] FillWithEntropy(this byte[] ToFill)
        {
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(ToFill);
            return ToFill;
        }

    }

}
