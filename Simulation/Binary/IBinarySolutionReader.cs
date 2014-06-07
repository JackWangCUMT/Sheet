using Sheet.Simulation.Core;

namespace Sheet.Simulation.Binary
{
    public interface IBinarySolutionReader
    {
        Solution Open(string path);
    }
}
