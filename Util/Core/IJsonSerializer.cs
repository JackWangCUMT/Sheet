using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Util.Core
{
    public interface IJsonSerializer
    {
        string Serialize(object value);
        T Deerialize<T>(string value);
    }
}
