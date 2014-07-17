using Sheet.Block;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Simulation.Core
{
    public interface IClock
    {
        long Cycle { get; set; }
        int Resolution { get; set; }
    }

    public interface IBoolState
    {
        bool? PreviousState { get; set; }
        bool? State { get; set; }
    }

    public interface ISimulation
    {
        void Compile();
        void Calculate();
        void Reset();

        Element Element { get; set; }

        IClock Clock { get; set; }

        IBoolState State { get; set; }
        bool? InitialState { get; set; }
        Tuple<IBoolState, bool>[] StatesCache { get; set; }
        bool HaveCache { get; set; }

        Element[] DependsOn { get; set; }
    }

    public interface IStateSimulation
    {
        ISimulation Simulation { get; set; }
    }

    public interface ITimer
    {
        float Delay { get; set; }
        string Unit { get; set; }
    }

    public abstract class Element
    {
        public Element()
        {
            Children = new ObservableCollection<Element>();
            Parent = null;
        }
        public string Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public UInt32 ElementId { get; set; }
        public string Name { get; set; }
        public string FactoryName { get; set; }
        public Element Parent { get; set; }
        public ObservableCollection<Element> Children { get; set; }
        public Element SimulationParent { get; set; }
        public IBlock Block { get; set; }
    }

    public class Clock : IClock
    {
        #region Constructor

        public Clock()
            : base()
        {
        }

        public Clock(long cycle, int resolution)
            : this()
        {
            this.Cycle = cycle;
            this.Resolution = resolution;
        }

        #endregion

        #region Properties

        public long Cycle { get; set; }
        public int Resolution { get; set; }

        #endregion
    }

    public class BoolState : IBoolState, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void Notify(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region Properties

        public bool? previousState;
        public bool? PreviousState
        {
            get { return previousState; }
            set
            {
                if (value != previousState)
                {
                    previousState = value;
                    Notify("PreviousState");
                }
            }
        }

        public bool? state;
        public bool? State
        {
            get { return state; }
            set
            {
                if (value != state)
                {
                    state = value;
                    Notify("State");
                }
            }
        }

        #endregion
    }

    public static class SimulationSettings
    {
        public static bool EnableDebug { get; set; }
        public static bool EnableLog { get; set; }
    }

    public class SimulationContext
    {
        public System.Threading.Timer SimulationTimer { get; set; }
        public object SimulationTimerSync { get; set; }
        public IClock SimulationClock { get; set; }
        public SimulationCache Cache { get; set; }
    }

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
