using System.Collections.Generic;

namespace EncryptedType
{
    public interface IKeyServer
    {
        string ID { get; }

        string GetKey(string KeyName);

        IList<string> Keys {get;}

        IDictionary<string, string> Map { get; }
    }

    public struct Key
    {
        public string Name { get; set; }

        public string KeyValue { get; set; }
    }
}
