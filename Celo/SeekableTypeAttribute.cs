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
   public class SeekableTypeAttribute : InstanceLevelAspect
    {
        [IntroduceMember(IsVirtual = false, OverrideAction = MemberOverrideAction.OverrideOrFail, Visibility = PostSharp.Reflection.Visibility.Public)]
        public Dictionary<string, string> HashedValues { set; get; }

        public override void RuntimeInitialize(Type type)
        {
            HashedValues = new Dictionary<string, string>();
        }

    }
}
