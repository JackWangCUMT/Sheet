using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Simulation.Core
{
    public class TimerPulse : Element, ITimer, IStateSimulation
    {
        public TimerPulse() : base() { }
        public float Delay { get; set; }
        public string Unit { get; set; }
        public ISimulation Simulation { get; set; }
    }
}
