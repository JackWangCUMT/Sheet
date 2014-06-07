using Sheet.Simulation.Core;

namespace Sheet.Simulation.Tests
{
    public interface ISolutionRenamer
    {
        void Rename(Solution solution);
        void Rename(Project project);
        void Rename(Context context);
    }
}