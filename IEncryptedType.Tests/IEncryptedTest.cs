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
    }
}
