using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Simulation.Core
{
    public interface ITimer
    {
        float Delay { get; set; }
        string Unit { get; set; }
    }
}
