//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Linq.Expressions;

//using EncryptedType;

//using NUnit.Framework;

//namespace EncryptedType.Tests
//{
//    [TestFixture]
//    public class CertificateEncryptionTest
//    {
//        [EncryptedType]
//        public class EncTest
//        {
//            public string ID { get; set; }
//            [CertificateEncryptedValue(CertificatePath = @"c:\temp\cert\joesoft.pfx")]
//            public string SSN { get; set; }

//            public string IntegrityValue()
//            {
//                return this.ID;
//            }

//            public EncTest()
//            {
//                this.ID = Guid.NewGuid().ToString();
//            }

//        }

//        [Test]
//        public void TestAspect()
//        {
//            var n = new EncTest();
//            n.SSN = "111-11-1111";
//            Assert.AreNotEqual("111-11-1111", n.SSN);
//        }

//    }
//}
