using Sheet.Simulation.Core;

namespace Sheet.Simulation.Tests
{
    public interface ISolutionSimulationFactory
    {
        Clock Clock { get; }
        bool IsSimulationRunning { get; }
        SimulationController SimulationController { get; }
        Solution Solution { get; }

        void Start();
        void Stop();
    }
}