using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using EncryptedType;
using MSSQLServer;

namespace IEncryptedTypeTests
{
    [TestFixture]
    public class IEncryptedTest
    {
        [Test]
        //[TestCase(null, "Key1", true)]
        [TestCase("", "Key1", false)]
        //[TestCase("foobar", "Key1", false)]
        public void TestEncrypt(string ToEncrypt,string KeyID, bool ExpectEmpty)
        {
            var ks = new MSSQLServer.MSSQLServer();
            var res = ToEncrypt.Encrypt(KeyID, new List<IKeyServer>() { ks }, () => { return "123456"; });
            if(ExpectEmpty)
            {
                Assert.IsNull(res);
            }
            else
            {
                Assert.IsNotNull(res);
                Assert.IsNotEmpty(res);
            }
        }

        [Test]
        [TestCase(null, "Key1", true)]
        [TestCase("", "Key1", false)]
        [TestCase("foobar", "Key1", false)]
        public void TestDecrypt(string ToEncrypt,string KeyID, bool ExpectEmpty)
        {
            var ks = new MSSQLServer.MSSQLServer();
            var integ = new Func<string>(() => { return "123456"; });
            var servers = new List<IKeyServer>() { ks };
            var enc = ToEncrypt.Encrypt(KeyID, servers, integ);
            if(ExpectEmpty)
            {
                Assert.IsNull(enc);
            }
            else
            {
                var dec = enc.Decrypt(servers, integ);
                Assert.AreEqual(ToEncrypt, dec);
            }
        }

        [Test]
        public void TestTest()
        {
            var ks = new MSSQLServer.MSSQLServer();
            var integ = new Func<string>(() => { return "123456"; });
            var servers = new List<IKeyServer>() { ks };
            var enc = "foo".Encrypt("Key2", servers, integ);
            Console.WriteLine(enc);
            var dec = enc.Decrypt(servers, integ);
        }

        [Test]
        public void TestDec()
        {
            var ks = new MSSQLServer.MSSQLServer();
            var integ = new Func<string>(() => { return "123456"; });
            var servers = new List<IKeyServer>() { ks };
            var enc = "Key2~AES~HS256~H669DtnhM7uEyjnF/HszYQ==~pMTKFKk5nT5K8XH2PxAuMlBumE56qTpxmbac2sp1Z5wcj60HhDMVmKC2RKumGzh5aCi7mTXmSF5NjFGvXfwVFw==~WTP35Q09qw8U6/aYq1/z3xR7X7Bv8yxRIi2k4fgPf/M=";
            var dec = enc.Decrypt(servers, integ);
            Console.WriteLine(dec);
        }
    }
}
