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

namespace Celo
{
    [PSerializable]
    [AttributeUsage(AttributeTargets.Class)]
    [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, typeof(EncryptedValueAttribute))]
    [IntroduceInterface(typeof(ICelo), OverrideAction = InterfaceOverrideAction.Ignore)]
    public class EncryptedTypeAttribute : InstanceLevelAspect, ICelo
    {
        [IntroduceMember(IsVirtual = false, OverrideAction = MemberOverrideAction.OverrideOrFail, Visibility = PostSharp.Reflection.Visibility.Public)]
        public IDictionary<string, string> EncryptedValues { set; get; }

        [IntroduceMember(IsVirtual = false, OverrideAction = MemberOverrideAction.OverrideOrFail, Visibility = PostSharp.Reflection.Visibility.Public)]
        public IDictionary<string, string> EncryptionKeys { set; get; }

        [IntroduceMember(IsVirtual=false,OverrideAction=MemberOverrideAction.OverrideOrFail, Visibility=PostSharp.Reflection.Visibility.Public)]
        public IKeyServer KeyServer {get;set;}

        public override void RuntimeInitialize(Type type)
        {
            EncryptedValues = new Dictionary<string, string>();
            EncryptionKeys = new Dictionary<string, string>();
        }

    }
}
