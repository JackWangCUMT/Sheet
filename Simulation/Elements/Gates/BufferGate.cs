using Sheet.Simulation.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Simulation.Elements
{
    public class BufferGate : Element, IStateSimulation
    {
        public BufferGate() : base() { }
        public ISimulation Simulation { get; set; }
    }
}
