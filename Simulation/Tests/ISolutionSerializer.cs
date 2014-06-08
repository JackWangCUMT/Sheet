using Sheet.Block.Core;
using Sheet.Simulation.Elements;

namespace Sheet.Simulation.Tests
{
    public interface ISolutionSerializer
    {
        Solution Serialize(IBlock root);
    }
}