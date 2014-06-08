using Sheet.Simulation.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Simulation.Elements
{
    public class Pin : Element
    {
        public Pin() : base() { }
        public bool IsPinTypeUndefined { get; set; }
        public PinType Type { get; set; }
        public Tuple<Pin, bool>[] Connections { get; set; } // bool is flag for Inverted
    }
}
