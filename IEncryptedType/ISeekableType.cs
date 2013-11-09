using System.Collections.Generic;

namespace EncryptedType
{
    public interface ISeekableType
    {
        IDictionary<string, string> HashedValues { set; get; }

        int Iterations { get; set; }

        string HashValue(string Value);

    }
}
