using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PostSharp;
using PostSharp.Serialization;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Advices;

using EncryptedType;
using EncryptedType.RavenDB;

namespace EncryptedType.RavenDB
{
    [PSerializable]
    [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, typeof(RavenSeekableTypeAttribute))]
    [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, typeof(RavenEncryptedTypeAttribute))]
    [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, typeof(RavenEncryptedValueAttribute))]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    [CopyCustomAttributes]

    public class RavenSeekableValueAttribute : SeekableValueAttribute
    {
    }
}
