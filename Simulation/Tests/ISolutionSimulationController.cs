namespace Sheet.Simulation.Tests
{
    interface ISolutionSimulationController
    {
        void EnableDebug(bool enable);
        void EnableLog(bool enable);
        void Start();
        void Stop();
    }
}