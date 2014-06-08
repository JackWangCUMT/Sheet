using Sheet.Simulation.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Simulation.Elements
{
    public class XnorGate : Element, IStateSimulation
    {
        public XnorGate() : base() { }
        public ISimulation Simulation { get; set; }
    }
}
