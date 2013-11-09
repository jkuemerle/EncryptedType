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

using Raven;
using Raven.Client.Document;

namespace RavenAspect
{
    [PSerializable]
    [AttributeUsage(AttributeTargets.Class)]
    [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, typeof(EncryptedValueAttribute))]
    public class EncryptedTypeAttribute : InstanceLevelAspect
    {
        [IntroduceMember(IsVirtual = false, OverrideAction = MemberOverrideAction.OverrideOrFail, Visibility = PostSharp.Reflection.Visibility.Public)]
        public IDictionary<string, string> EncryptedValues { set; get; }

        public override void RuntimeInitialize(Type type)
        {
            EncryptedValues = new Dictionary<string, string>();
        }

        [IntroduceMember(IsVirtual = false, OverrideAction = MemberOverrideAction.OverrideOrFail, Visibility = PostSharp.Reflection.Visibility.Public)]
        public IDictionary<string, string> GetEncryptedValues()
        {
            return this.EncryptedValues;
        }
    }

    [PSerializable]
    [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, typeof(EncryptedTypeAttribute))]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class EncryptedValueAttribute : LocationInterceptionAspect, IInstanceScopedAspect
    {
        private string propname;

        public override void CompileTimeInitialize(PostSharp.Reflection.LocationInfo targetLocation, AspectInfo aspectInfo)
        {
            propname = targetLocation.Name;
        }

        [ImportMember("GetEncryptedValues", IsRequired = true, Order = ImportMemberOrder.AfterIntroductions)]
        public Func<IDictionary<string, string>> GetEncryptedValues;

        public object CreateInstance(AdviceArgs adviceArgs) { return this.MemberwiseClone(); }

        public void RuntimeInitializeInstance() { }

        public override void OnSetValue(LocationInterceptionArgs args)
        {
            if (null != GetEncryptedValues())
                if (!GetEncryptedValues().ContainsKey(propname))
                    GetEncryptedValues().Add(propname, args.Value.ToString());
                else
                    GetEncryptedValues()[propname] = args.Value.ToString();
        }

        public override void OnGetValue(LocationInterceptionArgs args)
        {
            if (GetEncryptedValues().ContainsKey(propname))
                args.Value = GetEncryptedValues()[propname];
        }
    }

    [EncryptedType]
    public class EncTest
    {
        public string ID { get; set; }
        [EncryptedValue]
        public string SecureValue { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            string serverURL = "http://klaptop:1999";
            var item = new EncTest() { ID = Guid.NewGuid().ToString(), SecureValue = "Test" };
            using (var ds = new Raven.Client.Document.DocumentStore() { Url = serverURL, DefaultDatabase = "Test" })
            {
                ds.Initialize();
                var session = ds.OpenSession();
                session.Store(item, item.ID);
                session.SaveChanges();
                var test = session.Load<EncTest>(item.ID);
                if (item.ID == test.ID)
                    Console.WriteLine("Successfully saved!");
                else
                    Console.WriteLine("Did not save.");
            }
            Console.WriteLine("Execution complete.");
        }
    }
}
