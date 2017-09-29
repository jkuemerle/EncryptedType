using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using EncryptedType;
using MSSQLServer;

namespace MSSQLServerTests
{
    [TestFixture]
    public class MSSQLTest
    {

        [Test]
        public void TestList()
        {
            IKeyServer ks = new MSSQLServer.MSSQLServer();
            var res = ks.Keys;
            Assert.IsNotNull(res);
            Assert.Greater(res.Count(), 0);
        }

        [Test]
        [TestCase(null,null)]
        [TestCase("Key1", "fjwflkwejfoijoijfweoihfweoihjh")]
        public void TestGet(string id, string expected)
        {
            IKeyServer ks = new MSSQLServer.MSSQLServer();
            var res = ks.GetKey(id);
            Assert.AreEqual(expected, res);
        }
    }
}
