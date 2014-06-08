using Sheet.Block.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Block.Model
{
    public class XArgbColor : IArgbColor
    {
        public byte Alpha { get; set; }
        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }
        public XArgbColor(byte alpha, byte red, byte green, byte blue)
        {
            Alpha = alpha;
            Red = red;
            Green = green;
            Blue = blue;
        }
    }
}
