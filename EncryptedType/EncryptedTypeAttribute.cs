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
    //[AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, typeof(CertificateEncryptedValueAttribute))]
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
        public IEnumerable<IKeyServer> KeyServers { get; set; }

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
                return EncryptedValues[PropertyName].Decrypt(KeyServers, IntegrityFunction);
            }
            return null;
        }

        [IntroduceMember(IsVirtual = false, OverrideAction = MemberOverrideAction.OverrideOrFail, Visibility = PostSharp.Reflection.Visibility.Public)]
        public string Encrypt(string Data, string KeyName, Func<string> IntegrityFunction)
        {
            return Data.Encrypt(KeyName, KeyServers, IntegrityFunction);
        }

    }
}
