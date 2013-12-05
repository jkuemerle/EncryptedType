using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EncryptedType;

using NUnit.Framework;

namespace EncryptedType.Tests
{
    [TestFixture]
    public class SeekTests
    {
        [EncryptedType]
        [SeekableType]
        public class EncTest
        {
            public string ID { get; set; }
            [EncryptedValue]
            [SeekableValue]
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
        public void TestSeek()
        {
            var n = new EncTest();
            var s = new CeloClavis.TestServer();
            ((IEncryptedType)n).KeyServer = s;
            ((IEncryptedType)n).EncryptionKeys = s.Map;
            //((IEncryptedType)n).Integrity = n.IntegrityValue;
            n.SSN = "111-11-1111";
            Assert.AreNotEqual("111-11-1111", n.SSN);
        }


    }
}
