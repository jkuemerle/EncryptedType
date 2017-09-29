//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Security.Cryptography;
//using System.Security.Cryptography.X509Certificates;
//using System.IO;

//using PostSharp;
//using PostSharp.Serialization;
//using PostSharp.Aspects;
//using PostSharp.Aspects.Dependencies;
//using PostSharp.Aspects.Advices;


//namespace EncryptedType
//{
//    [PSerializable]
//    [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, typeof(EncryptedTypeAttribute))]
//    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
//    public class CertificateEncryptedValueAttribute : LocationInterceptionAspect, IInstanceScopedAspect
//    {
//        public string CertificateName { get; set; }
//        public string CertificatePath { get; set; }

//        private string propname;

//        public override void CompileTimeInitialize(PostSharp.Reflection.LocationInfo targetLocation, AspectInfo aspectInfo)
//        {
//            propname = targetLocation.Name;
//        }

//        [ImportMember("EncryptedValues", IsRequired = true)]
//        public Property<IDictionary<string, string>> EncryptedValuesStore;

//        [ImportMember("Encrypt", IsRequired = true, Order = ImportMemberOrder.AfterIntroductions)]
//        public Func<byte[], SymmetricMetaData, string> Encrypt;

//        //[ImportMember("Decrypt", IsRequired = true, Order = ImportMemberOrder.AfterIntroductions)]
//        //public Func<byte[], string, byte[], byte[], byte[], SymmetricAlgorithm, string> Decrypt;
//        //public string Decrypt(byte[] encrypted, string mac, byte[] iv, byte[] key, byte[] secret, SymmetricAlgorithm crypter)
        
//        public object CreateInstance(AdviceArgs adviceArgs) { return this.MemberwiseClone(); }

//        public void RuntimeInitializeInstance() { }

//        public override void OnSetValue(LocationInterceptionArgs args)
//        {
//            var symmetric = SymmetricMetaData.NewRandom();
//            var encrypted = string.Format("{0}\0{1}",EncryptSymmetricKey(symmetric.Key.KeyBytes),EncryptData(args.Value.ToString(),symmetric));
//            if (null != EncryptedValuesStore.Get())
//                if (!EncryptedValuesStore.Get().ContainsKey(propname))
//                    EncryptedValuesStore.Get().Add(propname, encrypted);
//                else
//                    EncryptedValuesStore.Get()[propname] = encrypted;
//        }

//        public override void OnGetValue(LocationInterceptionArgs args)
//        {
//            if (EncryptedValuesStore.Get().ContainsKey(propname))
//                args.Value = EncryptedValuesStore.Get()[propname];
//        }

//        private string EncryptData(string Data, SymmetricMetaData Symmetric)
//        {
//            return Encrypt(Encoding.Unicode.GetBytes(Data),Symmetric);
//        }

//        private string EncryptSymmetricKey(byte[] Data)
//        {
//            string retVal = null;
//            X509Certificate2 cert = null; 
//            if (!string.IsNullOrEmpty(CertificatePath))
//                cert = new X509Certificate2(CertificatePath);
//            RSACryptoServiceProvider rsa = cert.PublicKey.Key as RSACryptoServiceProvider;
//            retVal = Convert.ToBase64String(rsa.Encrypt(Data, false));
//            return retVal;
//        }

//        private byte[] DecryptSymmetricKey(string Data)
//        {
//            byte[] buffer = Convert.FromBase64String(Data);
//            X509Certificate2 cert = null;
//            if (!string.IsNullOrEmpty(CertificatePath))
//                cert = new X509Certificate2(CertificatePath);
//            RSACryptoServiceProvider rsa = cert.PublicKey.Key as RSACryptoServiceProvider;
//            return rsa.Decrypt(buffer, false);
//        }
//    }
//}
