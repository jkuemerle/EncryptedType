using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Celo;

using NUnit.Framework;

namespace Celo.Tests
{
    [TestFixture]
    public class TestCelo
    {

        [EncryptedType]
        public class EncTest
        {
            [EncryptedValue]
            public string SSN { get; set; }
        }

        [Test]
        public void TestAspect()
        {
            var n = new EncTest();
            ((ICelo)n).KeyServer = new CeloClavis.TestServer();
            ((ICelo)n).EncryptionKeys.Add("SSN", "Key1");
            n.SSN = "111-11-1111";
            Assert.AreEqual("111-11-1111", n.SSN);
        }

    }
}
