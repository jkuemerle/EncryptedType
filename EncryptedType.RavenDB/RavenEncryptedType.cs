using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PostSharp;
using PostSharp.Serialization;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Aspects.Dependencies;
using Raven.Imports.Newtonsoft.Json;

using EncryptedType;
using EncryptedType.RavenDB;
using PostSharp.Extensibility;

namespace EncryptedType.RavenDB
{
    [PSerializable]
    [AttributeUsage(AttributeTargets.Class, Inherited=true)]
    [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, typeof(RavenEncryptedValueAttribute))]
    [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, typeof(RavenSeekableTypeAttribute))]
    [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, typeof(RavenSeekableValueAttribute))]
    [IntroduceInterface(typeof(IEncryptedType), OverrideAction = InterfaceOverrideAction.Ignore)]
    public class RavenEncryptedTypeAttribute : EncryptedTypeAttribute, IEncryptedType
    {
        [Raven.Imports.Newtonsoft.Json.JsonIgnore]
        [IntroduceMember(IsVirtual = false, OverrideAction = MemberOverrideAction.OverrideOrFail, Visibility = PostSharp.Reflection.Visibility.Public)]
        [CopyCustomAttributes(typeof(Raven.Imports.Newtonsoft.Json.JsonIgnoreAttribute))]
        public IDictionary<string,Func<string>> Integrity {
            get
            {
                return base.Integrity;
            }
            set { 
                base.Integrity = value; 
            }
        }

        [Raven.Imports.Newtonsoft.Json.JsonIgnore]
        [IntroduceMember(IsVirtual = false, OverrideAction = MemberOverrideAction.OverrideOrFail, Visibility = PostSharp.Reflection.Visibility.Public)]
        [CopyCustomAttributes(typeof(Raven.Imports.Newtonsoft.Json.JsonIgnoreAttribute))]
        public IKeyServer KeyServer
        {
            get
            {
                return base.KeyServer;
            }
            set
            {
                base.KeyServer = value;
            }
        }

        [Raven.Imports.Newtonsoft.Json.JsonIgnore]
        [IntroduceMember(IsVirtual = false, OverrideAction = MemberOverrideAction.OverrideOrFail, Visibility = PostSharp.Reflection.Visibility.Public)]
        [CopyCustomAttributes(typeof(Raven.Imports.Newtonsoft.Json.JsonIgnoreAttribute))]
        public IDictionary<string, KeyInfo> KeyCache
        {
            get { return _sharedKeyCache; }
            set
            {
                lock (_sharedKeyCacheLock)
                {
                    _sharedKeyCache = value;
                }
            }
        }

        [Raven.Imports.Newtonsoft.Json.JsonIgnore]
        [IntroduceMember(IsVirtual = false, OverrideAction = MemberOverrideAction.OverrideOrFail, Visibility = PostSharp.Reflection.Visibility.Public)]
        [CopyCustomAttributes(typeof(Raven.Imports.Newtonsoft.Json.JsonIgnoreAttribute))]
        public IKeyServer SharedKeyServer
        {
            get
            {
                return _sharedKeyServer;
            }
            set
            {
                lock (_sharedKeyServer)
                {
                    _sharedKeyServer = value;
                }
            }
        }

    }
}
