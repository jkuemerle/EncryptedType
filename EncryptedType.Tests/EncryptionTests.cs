using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

using EncryptedType;

using NUnit.Framework;


namespace EncryptedType.Tests
{
    [TestFixture]
    public class EncryptionTests
    {

        [EncryptedType]
        public class EncTest
        {
            public string ID { get; set; }
            [EncryptedValue]
            public string SSN { get; set; }

            public string IntegrityValue()
            {
                return this.ID;
            }

            public EncTest()
            {
                this.ID = Guid.NewGuid().ToString();
            }

        }

        [Test]
        public void TestAspect()
        {
            var n = new EncTest();
            var s = new CeloClavis.TestServer();
            ((IEncryptedType)n).KeyServer = s;
            ((IEncryptedType)n).EncryptionKeys = s.Map;
            ((IEncryptedType)n).Integrity("SSN", n.IntegrityValue);   
            n.SSN = "111-11-1111";
            Assert.AreNotEqual("111-11-1111", n.SSN);
        }

        [Test]
        public void TestDecryption()
        {
            var n = new EncTest();
            var s = new CeloClavis.TestServer();
            ((IEncryptedType)n).KeyServer = s;
            ((IEncryptedType)n).EncryptionKeys = s.Map;
            ((IEncryptedType)n).Integrity(() => n.SSN, n.IntegrityValue);
            n.SSN = "111-11-1111";
            Assert.AreEqual("111-11-1111", ((IEncryptedType)n).AsClear(() => n.SSN));
        }

        [Test]
        public void TestFluentKey()
        {
            var n = new EncTest();
            var s = new CeloClavis.TestServer();
            ((IEncryptedType)n).KeyServer(s);
            ((IEncryptedType)n).Key(() => n.SSN, "Key1");
            ((IEncryptedType)n).Integrity(() => n.SSN, n.IntegrityValue);
            n.SSN = "111-11-1111";
            Assert.AreEqual("111-11-1111", ((IEncryptedType)n).AsClear(() => n.SSN));

        }

        //[Test]
        //public void TestSharedKeyServer()
        //{
        //    var n = new EncTest();
        //    var o = new EncTest();
        //    var s = new CeloClavis.TestServer();
        //    ((IEncryptedType)n).SharedKeyServer = s;

        //}
    }
}
