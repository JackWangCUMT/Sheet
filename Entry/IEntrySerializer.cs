using Sheet.Entry.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Entry
{
    public interface IEntrySerializer
    {
        void CreateEmpty(string path);
        void Serialize(SolutionEntry solution, string path);
        SolutionEntry Deserialize(string path);
    }
}
