using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Block.Core
{
    public interface IArgbColor
    {
        byte Alpha { get; set; }
        byte Red { get; set; }
        byte Green { get; set; }
        byte Blue { get; set; }
    }
}
