using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Simulation.Core
{
    public class SimulationCache
    {
        public bool HaveCache { get; set; }
        public ISimulation[] Simulations { get; set; }
        public IBoolState[] States { get; set; }
        public static void Reset(SimulationCache cache)
        {
            if (cache == null)
                return;

            if (cache.Simulations != null)
            {
                var lenght = cache.Simulations.Length;

                for (int i = 0; i < lenght; i++)
                {
                    var simulation = cache.Simulations[i];
                    simulation.Reset();

                    (simulation.Element as IStateSimulation).Simulation = null;
                }
            }

            cache.HaveCache = false;
            cache.Simulations = null;
            cache.States = null;
        }
    }
}
