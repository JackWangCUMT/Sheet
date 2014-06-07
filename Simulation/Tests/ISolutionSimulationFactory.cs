using Sheet.Simulation.Core;

namespace Sheet.Simulation.Tests
{
    public interface ISolutionSimulationFactory
    {
        Clock Clock { get; }
        bool IsSimulationRunning { get; }
        SimulationFactory SimulationFactory { get; }
        Solution Solution { get; }

        void Start();
        void Stop();
    }
}