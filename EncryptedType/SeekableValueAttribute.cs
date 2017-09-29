//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Security.Cryptography;

//using PostSharp;
//using PostSharp.Serialization;
//using PostSharp.Aspects;
//using PostSharp.Aspects.Dependencies;
//using PostSharp.Aspects.Advices;

//namespace EncryptedType
//{
//    [PSerializable]
//    [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, typeof(EncryptedTypeAttribute))]
//    [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, typeof(EncryptedTypeAttribute))]
//    [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, typeof(SeekableTypeAttribute))]
//    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
//    public class SeekableValueAttribute : LocationInterceptionAspect, IInstanceScopedAspect
//    {
//        private string propname;

//        public override void CompileTimeInitialize(PostSharp.Reflection.LocationInfo targetLocation, AspectInfo aspectInfo)
//        {
//            propname = targetLocation.Name;
//        }

//        public object CreateInstance(AdviceArgs adviceArgs) { return this.MemberwiseClone(); }

//        public void RuntimeInitializeInstance() { }

//        [ImportMember("HashedValues", IsRequired = true)]
//        public Property<IDictionary<string, string>> HashedValuesStore;

//        [ImportMember("HashValue", IsRequired = true, Order = ImportMemberOrder.AfterIntroductions)]
//        public Func<string, string> HashValue;
//        public override void OnSetValue(LocationInterceptionArgs args)
//        {
//            if (HashedValuesStore.Get().ContainsKey(propname))
//                HashedValuesStore.Get()[propname] = HashValue(args.Value.ToString());
//            else
//                HashedValuesStore.Get().Add(propname, HashValue(args.Value.ToString()));
//            args.ProceedSetValue();
//        }


//    }
//}
