using Sheet.Simulation.Core;

namespace Sheet.Simulation.Tests
{
    public interface ISolutionSerializer
    {
        Solution Serialize(IBlock root);
    }
}