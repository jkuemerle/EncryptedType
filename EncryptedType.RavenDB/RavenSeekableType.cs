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

namespace EncryptedType.RavenDB
{
    [PSerializable]
    [AttributeUsage(AttributeTargets.Class)]
    [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, typeof(RavenSeekableValueAttribute))]
    [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, typeof(RavenEncryptedTypeAttribute))]
    [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, typeof(RavenEncryptedValueAttribute))]
    [IntroduceInterface(typeof(ISeekableType), OverrideAction = InterfaceOverrideAction.Ignore)]
    public class RavenSeekableTypeAttribute : SeekableTypeAttribute
    {
        [Raven.Imports.Newtonsoft.Json.JsonIgnore]
        [IntroduceMember(IsVirtual = false, OverrideAction = MemberOverrideAction.OverrideOrFail, Visibility = PostSharp.Reflection.Visibility.Public)]
        [CopyCustomAttributes(typeof(Raven.Imports.Newtonsoft.Json.JsonIgnoreAttribute))]
        public int Iterations
        {
            get
            {
                return this._iterations;
            }
            set
            {
                _iterations = value;
            }
        }

    }
}
