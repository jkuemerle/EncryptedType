using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq.Expressions;
using System.Security.Cryptography;
using EncryptedType;

namespace EncryptedType
{
    public struct KeyInfo
    {
        public string KeyValue { get; set; }

        public byte[] KeyBytes { get; set; }

        public byte[] SecretBytes { get; set; }

        public KeyInfo(string Value)
            : this()
        {
            this.KeyValue = Value;
        }

        public KeyInfo(byte[] keyBytes, byte[] secretBytes) : this()
        {
            this.KeyBytes = keyBytes;
            this.SecretBytes = secretBytes;
        }
    }

    public class SymmetricMetaData
    {
        public KeyInfo Key { get; set; }

        public byte[] IV { get; set; }

        public SymmetricAlgorithm Crypter { get; set; }

        private SymmetricMetaData() { } 

        public static SymmetricMetaData NewRandom()
        {
            SymmetricMetaData retVal = new SymmetricMetaData();
            retVal.Crypter = GetCrypter();
            retVal.IV = new byte[retVal.Crypter.BlockSize / 8].FillWithEntropy();
            retVal.Key = new KeyInfo(new byte[retVal.Crypter.BlockSize / 8].FillWithEntropy(),
                new byte[retVal.Crypter.BlockSize / 8].FillWithEntropy());
            return retVal;
        }

        public static SymmetricMetaData RandomIV()
        {
            SymmetricMetaData retVal = new SymmetricMetaData();
            retVal.Crypter = GetCrypter();
            retVal.IV = new byte[retVal.Crypter.BlockSize / 8].FillWithEntropy();
            return retVal;
        }

        private static SymmetricAlgorithm GetCrypter()
        {
            var crypter = new System.Security.Cryptography.RijndaelManaged();
            crypter.Mode = CipherMode.CBC;
            return crypter;
        }

    }


    public interface IEncryptedType
    {
        IDictionary<string, KeyInfo> KeyCache { get; set; }

        IDictionary<string, string> EncryptionKeys { get; set; }

        IDictionary<string, Func<string>> Integrity { get; set; }

        object ClearText(string PropertyName);

        IEnumerable<IKeyServer> KeyServers { get; set; }

    }
}

namespace System
{
    public static class Extensions
    {

        public static object AsClear<TObject, TValue>(this TObject Item, Expression<Func< TValue>> Property) where TObject : EncryptedType.IEncryptedType
        {
            return ((EncryptedType.IEncryptedType)Item).ClearText(Property.PropertyName<TValue>());
        }

        public static object GetClearText<TObject, TValue>(this TObject Item, Expression<Func<TValue>> Property)
        {
            var obj = (EncryptedType.IEncryptedType) Item;
            if(null == obj )
            {
                return default(TObject);
            }
            return obj.AsClear(Property);
                //((IEncryptedType)n).AsClear(() => n.SSN);
        }

        public static object SetKeyServer<T>(this T Item, EncryptedType.IKeyServer KeyServer)
        {
            var obj = (EncryptedType.IEncryptedType)Item;
            if (null == obj)
            {
                return default(T);
            }
            obj.KeyServer(KeyServer);
            return obj;
        }

        public static TObject SetKey<TObject, TValue>(this TObject Item, Expression<Func<TValue>> Property, string KeyName)
        {
            var obj = (EncryptedType.IEncryptedType)Item;
            if(null == obj)
            {
                return default(TObject);
            }
            string propName = Property.PropertyName();
            if (null != obj.EncryptionKeys)
                if (obj.EncryptionKeys.ContainsKey(propName))
                    obj.EncryptionKeys[propName] = KeyName;
                else
                    obj.EncryptionKeys.Add(propName, KeyName);
            return (TObject) obj;
        }
        
        public static T Key<T>(this T Item, Expression<Func<object>> Property, string KeyName) where T : EncryptedType.IEncryptedType
        {
            string propName = Property.PropertyName();
            if (null != ((EncryptedType.IEncryptedType)Item).EncryptionKeys)
                if (((EncryptedType.IEncryptedType)Item).EncryptionKeys.ContainsKey(propName))
                    ((EncryptedType.IEncryptedType)Item).EncryptionKeys[propName] = KeyName;
                else
                    ((EncryptedType.IEncryptedType)Item).EncryptionKeys.Add(propName, KeyName);
            return Item;
        }
 
        public static T Integrity<T>(this T Item, string PropertyName, Func<string> Function) where T : EncryptedType.IEncryptedType
        {
            if(null != Item.Integrity)
            {
                if (Item.Integrity.ContainsKey(PropertyName))
                    Item.Integrity[PropertyName] = Function;
                else
                    Item.Integrity.Add(PropertyName, Function);
            }
            return Item;
        }

        public static T Integrity<T>(this T Item, Expression<Func<object>> Property, Func<string> Function) where T : EncryptedType.IEncryptedType
        {
            string PropertyName = Property.PropertyName();
            if (null != Item.Integrity)
            {
                if (Item.Integrity.ContainsKey(PropertyName))
                    Item.Integrity[PropertyName] = Function;
                else
                    Item.Integrity.Add(PropertyName, Function);
            }
            return Item;
        }

        public static TObject SetIntegrity<TObject, TValue>(this TObject Item, Expression<Func<TValue>> Property, Func<string> Function)
        {
            var obj = (EncryptedType.IEncryptedType)Item;
            if (null == obj)
            {
                return default(TObject);
            }
            string propName = Property.PropertyName();
            if (null != obj.Integrity)
            {
                if (obj.Integrity.ContainsKey(propName))
                    obj.Integrity[propName] = Function;
                else
                    obj.Integrity.Add(propName, Function);
            }
            return (TObject) obj;

        }

        public static T KeyServer<T>(this T Item, EncryptedType.IKeyServer Server) where T : EncryptedType.IEncryptedType
        {
            if (null == ((EncryptedType.IEncryptedType)Item).KeyServers)
            {
                ((EncryptedType.IEncryptedType)Item).KeyServers = new List<IKeyServer>() { Server };
            }
            else
            {
                var test = ((IList<IKeyServer>)((EncryptedType.IEncryptedType)Item).KeyServers);
                if (!test.Contains(Server))
                {
                    test.Add(Server);
                }
            }
            return Item;
        }

        public static string PropertyName<T>(this Expression<Func<T>> Property)
        {
            MemberExpression memberExpression = null;
            if (Property.Body.NodeType == ExpressionType.Convert)
            {
                memberExpression = ((UnaryExpression)Property.Body).Operand as MemberExpression;
            }
            else if (Property.Body.NodeType == ExpressionType.MemberAccess)
            {
                memberExpression = Property.Body as MemberExpression;
            }

            if (memberExpression == null)
            {
                throw new ArgumentException("Not a member access", "expression");
            }
            var propName = (memberExpression.Member as PropertyInfo).Name;
            return propName;
        }

    }
}
