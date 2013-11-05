using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celo
{
    public interface IKeyServer
    {
        string GetKey(string KeyName);

        IList<string> Keys {get;}
    }
}
