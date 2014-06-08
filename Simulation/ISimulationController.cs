using System;
using System.Collections.Generic;
using Sheet.Simulation.Core;
using Sheet.Simulation.Elements;

namespace Sheet.Simulation
{
    public interface ISimulationController
    {
        Compiler Compiler { get; }
        SimulationContext SimulationContext { get; set; }
        bool IsConsole { get; set; }

        void Run(List<Context> contexts, IEnumerable<Tag> tags);
        void Run(List<Context> contexts, IEnumerable<Tag> tags, int period, Action update);
        void Stop();
        void Reset(bool collect);
        void ResetTimerAndClock();
    }
}