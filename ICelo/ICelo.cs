using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celo
{
    public interface ICelo
    {
        IDictionary<string, string> EncryptionKeys { get; set; }

        IKeyServer KeyServer { get; set; }

    }
}
