using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Util.Core
{
    public interface IDatabaseController
    {
        string Name { get; set; }
        string[] Columns { get; set; }
        List<string[]> Data { get; set; }
        string[] Get(int index);
        bool Update(int index, string[] item);
        int Add(string[] item);
    }
}
