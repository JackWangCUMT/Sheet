using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Simulation.Core
{
    public class TopologicalSort<T> where T : class
    {
        #region Sort

        public IEnumerable<T> Sort(IEnumerable<T> source, Func<T, IEnumerable<T>> dependencies, bool ignoreDependencyCycles)
        {
            var sorted = new List<T>();
            var visited = new HashSet<T>();

            foreach (var item in source)
            {
                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print("+ {0} depends on:", (item as ISimulation).Element.Name);
                }

                Visit(item, visited, sorted, dependencies, ignoreDependencyCycles);

                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print("");
                }
            }

            return sorted;
        }

        #endregion

        #region Visit

        private void Visit(T item, HashSet<T> visited, List<T> sorted, Func<T, IEnumerable<T>> dependencies, bool ignoreDependencyCycles)
        {
            if (!visited.Contains(item))
            {
                visited.Add(item);

                var dependsOn = dependencies(item);

                if (dependsOn != null)
                {
                    foreach (var dep in dependsOn)
                    {
                        if (SimulationSettings.EnableDebug)
                        {
                            Debug.Print("|     {0}", (dep as ISimulation).Element.Name);
                        }

                        Visit(dep, visited, sorted, dependencies, ignoreDependencyCycles);
                    }

                    // add items with simulation dependencies
                    sorted.Add(item);
                }

                // add  items without simulation dependencies
                sorted.Add(item);
            }
            else if (!ignoreDependencyCycles && !sorted.Contains(item))
            {
                Debug.Print("Invalid dependency cycle: {0}", (item as Element).Name);
            }
        }

        #endregion
    }
}
