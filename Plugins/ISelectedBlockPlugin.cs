using Sheet.Block.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Plugins
{
    public interface ISelectedBlockPlugin
    {
        string Name { get; }
        bool CanProcess(ISheet contentSheet, IBlock contentBlock, IBlock selectedBlock, SheetOptions options);
        void Process(ISheet contentSheet, IBlock contentBlock, IBlock selectedBlock, SheetOptions options);
    }
}
