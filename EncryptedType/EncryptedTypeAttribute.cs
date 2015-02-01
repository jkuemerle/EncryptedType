using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

using PostSharp;
using PostSharp.Serialization;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Aspects.Dependencies;
using PostSharp.Extensibility;

namespace EncryptedType
{
    [PSerializable]
    [AttributeUsage(AttributeTargets.Class,Inherited=true,AllowMultiple=false)]
    [MulticastAttributeUsage(MulticastTargets.Class, Inheritance = MulticastInheritance.Multicast, AllowMultiple=false, PersistMetaData=true)]
    [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, typeof(EncryptedValueAttribute))]
    [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, typeof(CertificateEncryptedValueAttribute))]
    [IntroduceInterface(typeof(IEncryptedType), OverrideAction = InterfaceOverrideAction.Default)]
    public class EncryptedTypeAttribute : InstanceLevelAspect, IEncryptedType
    {
        protected static IKeyServer _sharedKeyServer;
        protected static IDictionary<string, string> _sharedKeys;
        protected static IDictionary<string,KeyInfo> _sharedKeyCache;
        protected static string _sharedKeyCacheLock = string.Empty;

        [IntroduceMember(IsVirtual = false, OverrideAction = MemberOverrideAction.OverrideOrFail, Visibility = PostSharp.Reflection.Visibility.Public)]
        public IKeyServer SharedKeyServer
        {
            get
            {
                return _sharedKeyServer;
            }
            set
            {
                _sharedKeyServer = value;
            }
        }


        [IntroduceMember(IsVirtual = false, OverrideAction = MemberOverrideAction.OverrideOrFail, Visibility = PostSharp.Reflection.Visibility.Public)]
        public IDictionary<string, KeyInfo> KeyCache {
            get { return _sharedKeyCache; }
            set {
                lock(_sharedKeyCacheLock) {
                    _sharedKeyCache = value;
                }
            }
        }

        [IntroduceMember(IsVirtual = false, OverrideAction = MemberOverrideAction.OverrideOrFail, Visibility = PostSharp.Reflection.Visibility.Public)]
        public IDictionary<string, string> EncryptedValues { set; get; }

        [IntroduceMember(IsVirtual = false, OverrideAction = MemberOverrideAction.OverrideOrFail, Visibility = PostSharp.Reflection.Visibility.Public)]
        public IDictionary<string, string> EncryptionKeys { set; get; }

        [IntroduceMember(IsVirtual=false,OverrideAction=MemberOverrideAction.OverrideOrFail, Visibility=PostSharp.Reflection.Visibility.Public)]
        public IKeyServer KeyServer { get; set; }

        [IntroduceMember(IsVirtual = true, OverrideAction = MemberOverrideAction.OverrideOrFail, Visibility = PostSharp.Reflection.Visibility.Public)]
        public IDictionary<string,Func<string>> Integrity { get; set; }


        [IntroduceMember(IsVirtual = false, OverrideAction = MemberOverrideAction.OverrideOrFail, Visibility = PostSharp.Reflection.Visibility.Public)]
        public IDictionary<string, string> GetEncryptedValues()
        {
            return this.EncryptedValues;
        }

        [IntroduceMember(IsVirtual = false, OverrideAction = MemberOverrideAction.OverrideOrFail, Visibility = PostSharp.Reflection.Visibility.Public)]
        public IDictionary<string, string> GetEncryptionKeys()
        {
            return this.EncryptionKeys;
        }

        public override void RuntimeInitializeInstance()
        {
            EncryptedValues = new ConcurrentDictionary<string, string>();
            EncryptionKeys = new ConcurrentDictionary<string, string>();
            base.RuntimeInitializeInstance();
        }

        [IntroduceMember(IsVirtual = false, OverrideAction = MemberOverrideAction.OverrideOrFail, Visibility = PostSharp.Reflection.Visibility.Public)]
        public object ClearText(string PropertyName)
        {
            if (EncryptionKeys.ContainsKey(PropertyName) && EncryptedValues.ContainsKey(PropertyName))
            {
                string keyName = EncryptionKeys[PropertyName];
                Func<string> IntegrityFunction = null;
                if (null != this.Integrity && this.Integrity.ContainsKey(PropertyName))
                    IntegrityFunction = this.Integrity[PropertyName];
                return Decrypt(EncryptedValues[PropertyName], keyName,IntegrityFunction);
            }
            return null;
        }

        [IntroduceMember(IsVirtual = false, OverrideAction = MemberOverrideAction.OverrideOrFail, Visibility = PostSharp.Reflection.Visibility.Public)]
        public string Encrypt(string Data, string KeyName, Func<string> IntegrityFunction)
        {
            if (null != IntegrityFunction)
                Data = AddHMAC(Data, IntegrityFunction);
            var val = System.Text.UnicodeEncoding.Unicode.GetBytes(Data);
            var symmetric = SymmetricMetaData.RandomIV();
            symmetric.Key = GetKeyInfo(KeyName, symmetric.IV, symmetric.Crypter);
            return Encrypt(val, symmetric);
        }

        [IntroduceMember(IsVirtual = false, OverrideAction = MemberOverrideAction.OverrideOrFail, Visibility = PostSharp.Reflection.Visibility.Public)]
        public string Encrypt(string val, string iv, string key, string secret, SymmetricAlgorithm crypter)
        {
            return Encrypt(Convert.FromBase64String(val), Convert.FromBase64String(iv), Convert.FromBase64String(key), Convert.FromBase64String(secret), crypter);
        }

        [IntroduceMember(IsVirtual = false, OverrideAction = MemberOverrideAction.OverrideOrFail, Visibility = PostSharp.Reflection.Visibility.Public)]
        public string Encrypt(byte[] val, SymmetricMetaData metadata)
        {
            return Encrypt(val, metadata.IV, metadata.Key.KeyBytes, metadata.Key.SecretBytes, metadata.Crypter);
        }

        [IntroduceMember(IsVirtual = false, OverrideAction = MemberOverrideAction.OverrideOrFail, Visibility = PostSharp.Reflection.Visibility.Public)]
        public string Encrypt(byte[] val, byte[] iv, byte[] key, byte[] secret, SymmetricAlgorithm crypter)
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
            return string.Format("{0}\0{1}\0{2}", Convert.ToBase64String(iv), Convert.ToBase64String(encrypted), ComputeHMAC(encrypted, secret));
        }

        private string AddHMAC(string Data, Func<string> Integrity)
        {
            var retVal = Data;
            retVal = string.Format("{0}\0{1}", Data, ComputeHMAC(Data, Integrity));
            return retVal;
        }

        private bool VerifyHMAC(string Data, Func<string> Integrity)
        {
            try
            {
                var values = Data.Split('\0');
                if (values.Length < 2)
                    return false;
                return values[1].ConstantTimeCompare(ComputeHMAC(values[0], Integrity));
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static string ComputeHMAC(string Data, Func<string> Integrity)
        {
            var hmac = new HMACSHA256() { Key = Encoding.Unicode.GetBytes(Integrity.Invoke()) };
            return Convert.ToBase64String(hmac.ComputeHash(Encoding.Unicode.GetBytes(Data)));
        }

        private static string ComputeHMAC(byte[] Data, byte[] Key)
        {
            if(null == Data || null == Key)
            {
                return null;
            }
            var hmac = new HMACSHA256() { Key = Key };
            return Convert.ToBase64String(hmac.ComputeHash(Data));
        }

        [IntroduceMember(IsVirtual = false, OverrideAction = MemberOverrideAction.OverrideOrFail, Visibility = PostSharp.Reflection.Visibility.Public)]
        public string Decrypt(string Data, string KeyName, Func<string> IntegrityFunction)
        {
            var vals = Data.Split('\0');
            var iv = Convert.FromBase64String(vals[0]);
            var encrypted = Convert.FromBase64String(vals[1]);
            var mac = vals[2];
            var crypter = new System.Security.Cryptography.RijndaelManaged();
            KeyInfo key = GetKeyInfo(KeyName, iv, crypter);
            if (vals.Length > 2)
            {
            }
            string retVal = Decrypt(encrypted, mac, iv, key.KeyBytes, key.SecretBytes, crypter);
            if (null != IntegrityFunction)
            {
                var values = retVal.Split('\0');
                if (values.Length < 2)
                    retVal = null;
                if (null != retVal && VerifyHMAC(retVal, IntegrityFunction))
                    retVal = values[0];
                else
                    retVal = null;
            }
            return retVal;
        }

        [IntroduceMember(IsVirtual = false, OverrideAction = MemberOverrideAction.OverrideOrFail, Visibility = PostSharp.Reflection.Visibility.Public)]
        public string Decrypt(string Data, string mac, SymmetricMetaData metadata)
        {
            return Decrypt(Convert.FromBase64String(Data), mac, metadata.IV, metadata.Key.KeyBytes, metadata.Key.SecretBytes, metadata.Crypter);
        }

        [IntroduceMember(IsVirtual = false, OverrideAction = MemberOverrideAction.OverrideOrFail, Visibility = PostSharp.Reflection.Visibility.Public)]
        public string Decrypt(byte[] encrypted, string mac, byte[] iv, byte[] key, byte[] secret, SymmetricAlgorithm crypter)
        {
            if (mac.ConstantTimeCompare(ComputeHMAC(encrypted, secret)))
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
                return Encoding.Unicode.GetString(decrypted, 0, decryptedByteCount);
            }
            return null;
        }

        public KeyInfo GetKeyInfo(string KeyName, byte[] IV, SymmetricAlgorithm Crypter)
        {
            if(null == this.KeyCache )
                lock(_sharedKeyCacheLock)
                {
                    if(null == this.KeyCache)
                        this.KeyCache = new ConcurrentDictionary<string, KeyInfo>();
                }
            if (KeyCache.ContainsKey(KeyName))
                return KeyCache[KeyName];
            string keyValue = null;
            KeyInfo retVal = new KeyInfo();
            if (null != this.SharedKeyServer)
                keyValue = this.SharedKeyServer.GetKey(KeyName);
            if (null == keyValue && null != this.KeyServer)
                keyValue = this.KeyServer.GetKey(KeyName);
            if(null != keyValue)
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
    }
}
