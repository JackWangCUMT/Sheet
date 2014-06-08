using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Simulation.Core
{
    public interface IBoolState
    {
        bool? PreviousState { get; set; }
        bool? State { get; set; }
    }
}
