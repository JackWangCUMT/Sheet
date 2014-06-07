using Sheet.Simulation.Core;

namespace Sheet.Simulation.Binary
{
    public interface IBinarySolutionWriter
    {
        void Save(string path, Solution solution);
    }
}
