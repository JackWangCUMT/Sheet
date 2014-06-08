using Sheet.Simulation.Elements;

namespace Sheet.Simulation.Binary
{
    public interface IBinarySolutionReader
    {
        Solution Open(string path);
    }
}
