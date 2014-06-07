using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Simulation.Core
{
    public interface ISimulation
    {
        void Compile();
        void Calculate();
        void Reset();

        Element Element { get; set; }

        IClock Clock { get; set; }

        IBoolState State { get; set; }
        bool? InitialState { get; set; }
        Tuple<IBoolState, bool>[] StatesCache { get; set; }
        bool HaveCache { get; set; }

        Element[] DependsOn { get; set; }
    }
}
