using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CeloClavis
{
    public class TestServer : EncryptedType.IKeyServer
    {
        public string ID { get{ return "TestServer";} }

        public IDictionary<string, string> _keys = new Dictionary<string,string>();

        public TestServer()
        {
            var work = File.ReadAllLines(@"C:\temp\testKeys.txt");
            if(work.Length > 1)
            {
                _keys.Add("KEY1", work[0]);
                _keys.Add("KEY2", work[1]);
            }
        }
        public IList<string> Keys 
        {  get 
            {
                return new string[] { "Key1", "Key2" }.ToList();
            }
        }

        public string GetKey(string KeyName)
        {
            var name = KeyName.Trim().ToUpperInvariant();
            if (_keys.ContainsKey(name))
                return _keys[name];
            return null;
        }

        public IDictionary<string,string> Map
        {
            get
            {
                var retVal = new Dictionary<string,string>();
                retVal.Add("SSN","Key1");
                return retVal;
            }
        }
    }
}
