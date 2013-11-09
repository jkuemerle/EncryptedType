using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

using PostSharp;
using PostSharp.Serialization;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Aspects.Dependencies;

namespace EncryptedType
{
    [PSerializable]
    [AttributeUsage(AttributeTargets.Class)]
    [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, typeof(EncryptedTypeAttribute))]
    [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, typeof(EncryptedValueAttribute))]
    [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, typeof(SeekableValueAttribute))]
    [IntroduceInterface(typeof(ISeekableType), OverrideAction = InterfaceOverrideAction.Ignore)]
    public class SeekableTypeAttribute : InstanceLevelAspect, ISeekableType
    {
        protected int _iterations = 5000;

        [IntroduceMember(IsVirtual = false, OverrideAction = MemberOverrideAction.OverrideOrFail, Visibility = PostSharp.Reflection.Visibility.Public)]
        public IDictionary<string, string> HashedValues { set; get; }

        [IntroduceMember(IsVirtual = false, OverrideAction = MemberOverrideAction.OverrideOrFail, Visibility = PostSharp.Reflection.Visibility.Public)]
        public int Iterations {
            get {
                return _iterations;
            }
            set
            {
                _iterations = value;
            }
        }

        public override void RuntimeInitialize(Type type)
        {
            HashedValues = new Dictionary<string, string>();
        }

        [IntroduceMember(IsVirtual = false, OverrideAction = MemberOverrideAction.OverrideOrFail, Visibility = PostSharp.Reflection.Visibility.Public)]
        public string HashValue(string Value)
        {
            var hash = new SHA512Managed();
            byte[] result = hash.ComputeHash(Encoding.Unicode.GetBytes(Value));
            for (int i = 1; i <= _iterations; i++ )
            {
                result = hash.ComputeHash(result);
            }
            return Convert.ToBase64String(result);
        }


    }
}
