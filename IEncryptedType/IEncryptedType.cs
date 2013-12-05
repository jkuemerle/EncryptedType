using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq.Expressions;

namespace EncryptedType
{
    public interface IEncryptedType
    {
        IDictionary<string, string> EncryptionKeys { get; set; }

        IKeyServer KeyServer { get; set; }

        IDictionary<string, Func<string>> Integrity { get; set; }

        object ClearText(string PropertyName);

        IKeyServer SharedKeyServer { get; set; }

    }
}

namespace System
{
    public static class Extensions
    {
        public static object AsClear<T>(this T Item, Expression<Func<object>> Property) where T : EncryptedType.IEncryptedType
        {
            return ((EncryptedType.IEncryptedType)Item).ClearText(Property.PropertyName());
        }

        public static T Key<T>(this T Item, Expression<Func<object>> Property, string KeyName) where T: EncryptedType.IEncryptedType
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
        public static T KeyServer<T>(this T Item, EncryptedType.IKeyServer Server) where T : EncryptedType.IEncryptedType
        {
            ((EncryptedType.IEncryptedType)Item).KeyServer = Server;
            return Item;
        }

        public static string PropertyName(this Expression<Func<object>> Property)
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
