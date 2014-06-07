using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Simulation.Core
{
    public class Tag : Element, IStateSimulation
    {
        public Tag() : base() { Properties = new Dictionary<string, TagProperty>(); }
        public IDictionary<string, TagProperty> Properties { get; set; }
        public ISimulation Simulation { get; set; }
    }
}
