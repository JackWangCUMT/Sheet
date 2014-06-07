using Sheet.Simulation.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Simulation
{
    public class Compiler
    {
        #region StateSimulationDict

        public Dictionary<Type, Func<Element, ISimulation>> StateSimulationDict = new Dictionary<Type, Func<Element, ISimulation>>()
        {
            // Tag
            {
                typeof(Tag),
                (element) =>
                {
                    var stateSimulation = element as IStateSimulation;
                    stateSimulation.Simulation = new TagSimulation();
                    stateSimulation.Simulation.Element = element;
                    return stateSimulation.Simulation;
                }
            },
            // AndGate
            {
                typeof(AndGate),
                (element) =>
                {
                    var stateSimulation = element as IStateSimulation;
                    stateSimulation.Simulation = new AndGateSimulation();
                    stateSimulation.Simulation.Element = element;
                    return stateSimulation.Simulation;
                }
            },
            // OrGate
            {
                typeof(OrGate),
                (element) =>
                {
                    var stateSimulation = element as IStateSimulation;
                    stateSimulation.Simulation = new OrGateSimulation();
                    stateSimulation.Simulation.Element = element;
                    return stateSimulation.Simulation;
                }
            },
            // TimerOn
            {
                typeof(TimerOn),
                (element) =>
                {
                    var stateSimulation = element as IStateSimulation;
                    stateSimulation.Simulation = new TimerOnSimulation();
                    stateSimulation.Simulation.Element = element;
                    return stateSimulation.Simulation;
                }
            },
            // TimerOff
            {
                typeof(TimerOff),
                (element) =>
                {
                    var stateSimulation = element as IStateSimulation;
                    stateSimulation.Simulation = new TimerOffSimulation();
                    stateSimulation.Simulation.Element = element;
                    return stateSimulation.Simulation;
                }
            },
            // TimerPulse
            {
                typeof(TimerPulse),
                (element) =>
                {
                    var stateSimulation = element as IStateSimulation;
                    stateSimulation.Simulation = new TimerPulseSimulation();
                    stateSimulation.Simulation.Element = element;
                    return stateSimulation.Simulation;
                }
            }
        };

        #endregion

        #region Compile

        private void ProcessInput(Pin input, string level)
        {
            var connections = input.Connections;
            var lenght = connections.Length;

            for (int i = 0; i < lenght; i++)
            {
                var connection = connections[i];

                bool isUndefined = connection.Item1.Type == PinType.Undefined;

                if (!(connection.Item1.SimulationParent is Context) && isUndefined)
                {
                    if (!(connection.Item1.SimulationParent is Tag))
                    {
                        var simulation = connection.Item1.SimulationParent as IStateSimulation;

                        connection.Item1.Type = PinType.Output;

                        if (SimulationSettings.EnableDebug)
                        {
                            Debug.Print("{0}{1} -> {2}", level, connection.Item1.ElementId, connection.Item1.Type);
                        }
                    }
                    else
                    {
                        if (SimulationSettings.EnableDebug)
                        {
                            Debug.Print("{0}(*) {1} -> {2}", level, connection.Item1.ElementId, connection.Item1.Type);
                        }
                    }

                    if (connection.Item1.SimulationParent != null && isUndefined)
                    {
                        ProcessOutput(connection.Item1, string.Concat(level, "    "));
                    }
                }
            }
        }

        private void ProcessOutput(Pin output, string level)
        {
            var pins = output.SimulationParent.Children.Where(p => p != output).Cast<Pin>();

            foreach (var pin in pins)
            {
                bool isUndefined = pin.Type == PinType.Undefined;

                if (!(pin.SimulationParent is Context) && !(pin.SimulationParent is Tag) && isUndefined)
                {
                    var simulation = pin.SimulationParent as IStateSimulation;

                    pin.Type = PinType.Input;

                    if (SimulationSettings.EnableDebug)
                    {
                        Debug.Print("{0}{1} -> {2}", level, pin.ElementId, pin.Type);
                    }
                }

                if (pin.Connections != null && pin.Connections.Length > 0 && isUndefined)
                {
                    ProcessInput(pin, level);
                }
            }
        }

        private void FindPinTypes(IEnumerable<Element> elements)
        {
            // find input connections
            var connections = elements.Where(x => x.Children != null)
                                      .SelectMany(x => x.Children)
                                      .Cast<Pin>()
                                      .Where(p => p.Connections != null && p.Type == PinType.Input && p.Connections.Length > 0 && p.Connections.Any(i => i.Item1.Type == PinType.Undefined))
                                      .ToArray();

            var lenght = connections.Length;

            if (lenght == 0)
                return;

            // process all input connections
            for (int i = 0; i < lenght; i++)
            {
                var connection = connections[i];
                var simulation = connection.SimulationParent as IStateSimulation;

                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print("+ {0} -> {1}", connection.ElementId, connection.Type);
                }

                ProcessInput(connection, "  ");

                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print("");
                }
            }
        }

        private void InitializeStates(List<ISimulation> simulations)
        {
            var lenght = simulations.Count;

            for (int i = 0; i < lenght; i++)
            {
                var state = new BoolState();
                var simulation = simulations[i];

                state.State = simulation.InitialState;

                simulation.State = state;
            }
        }

        private void GenerateCompileCache(List<ISimulation> simulations, IClock clock)
        {
            if (simulations == null)
            {
                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print("--- warning: no ISimulation elements ---");
                    Debug.Print("");
                }

                return;
            }

            var lenght = simulations.Count;

            for (int i = 0; i < lenght; i++)
            {
                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print("--- compilation: {0} | Type: {1} ---", simulations[i].Element.ElementId, simulations[i].GetType());
                    Debug.Print("");
                }

                simulations[i].Compile();

                simulations[i].Clock = clock;

                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print("");
                }
            }
        }

        private SimulationCache Compile(Element[] elements, IClock clock)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var cache = new SimulationCache();
            var connections = new Connections();

            // -- step 1: reset pin connections ---
            var pins = elements.Where(x => x is Pin).Cast<Pin>();

            connections.Reset(pins);

            // -- step 2: initialize IStateSimulation simulation
            var simulations = new List<ISimulation>();

            var lenght = elements.Length;
            for (int i = 0; i < lenght; i++)
            {
                var element = elements[i];
                if (element is IStateSimulation)
                {
                    var simulation = StateSimulationDict[element.GetType()](element);
                    simulations.Add(simulation);
                }
            }

            // -- step 3: update pin connections ---
            connections.Find(elements);

            if (SimulationSettings.EnableDebug)
            {
                Debug.Print("--- elements with input connected ---");
                Debug.Print("");
            }

            // -- step 4: get ordered elements for simulation ---
            FindPinTypes(elements);

            // -- step 5: initialize ISimulation states
            InitializeStates(simulations);

            // -- step 6: complile each simulation ---
            GenerateCompileCache(simulations, clock);

            // -- step 7: sort simulations using dependencies ---

            if (SimulationSettings.EnableDebug)
            {
                Debug.Print("-- dependencies ---");
                Debug.Print("");
            }

            var ts = new TopologicalSort<ISimulation>();
            var sortedSimulations = ts.Sort(simulations, x =>
            {
                if (x.DependsOn == null)
                    return null;
                else
                    return x.DependsOn.Cast<IStateSimulation>().Select(y => y.Simulation);
            }, true);

            if (SimulationSettings.EnableDebug)
            {
                Debug.Print("-- sorted dependencies ---");
                Debug.Print("");

                foreach (var simulation in sortedSimulations)
                {
                    Debug.Print("{0}", simulation.Element.ElementId);
                }

                Debug.Print("");
            }

            // -- step 8: cache sorted elements
            if (sortedSimulations != null)
            {
                cache.Simulations = sortedSimulations.ToArray();
                cache.HaveCache = true;
            }

            // Connections are not used after compilation is done
            foreach (var pin in pins)
                pin.Connections = null;

            // DependsOn are not used after compilation is done
            foreach (var simulation in simulations)
                simulation.DependsOn = null;

            pins = null;
            simulations = null;
            sortedSimulations = null;

            sw.Stop();

            Debug.Print("Compile() done in: {0}ms", sw.Elapsed.TotalMilliseconds);

            return cache;
        }

        public SimulationCache Compile(IEnumerable<Context> contexts, IEnumerable<Tag> tags, IClock clock)
        {
            var elements = contexts.SelectMany(x => x.Children).Concat(tags).ToArray();

            // compile elements
            var cache = Compile(elements, clock);

            // collect unused memory
            System.GC.Collect();

            return cache;
        }

        public SimulationCache Compile(Context context, IEnumerable<Tag> tags, IClock clock)
        {
            var elements = context.Children.Concat(tags).ToArray();

            // compile elements
            var cache = Compile(elements, clock);

            // collect unused memory
            System.GC.Collect();

            return cache;
        }

        #endregion
    }
}
