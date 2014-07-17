using Sheet.Simulation.Core;
using Sheet.Simulation.Elements;
using Sheet.Simulation.Simulations;
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

    public class Connections
    {
        #region Find Connections by Id

        private Dictionary<UInt32, List<Tuple<Pin, bool>>> MapPinsToWires(Element[] elements)
        {
            int lenght = elements.Length;
            var dict = new Dictionary<UInt32, List<Tuple<Pin, bool>>>();

            for (int i = 0; i < lenght; i++)
            {
                var element = elements[i];
                if (element is Wire)
                {
                    var wire = element as Wire;
                    var start = wire.Start;
                    var end = wire.End;
                    bool inverted = wire.InvertStart | wire.InvertEnd;

                    var startId = start.ElementId;
                    var endId = end.ElementId;

                    if (!dict.ContainsKey(startId))
                    {
                        dict.Add(startId, new List<Tuple<Pin, bool>>());
                    }

                    dict[startId].Add(new Tuple<Pin, bool>(start, inverted));
                    dict[startId].Add(new Tuple<Pin, bool>(end, inverted));

                    if (!dict.ContainsKey(endId))
                    {
                        dict.Add(endId, new List<Tuple<Pin, bool>>());
                    }

                    dict[endId].Add(new Tuple<Pin, bool>(start, inverted));
                    dict[endId].Add(new Tuple<Pin, bool>(end, inverted));
                }
            }

            return dict;
        }

        private void Find(Pin root, Pin pin, Dictionary<UInt32, Tuple<Pin, bool>> connections, Dictionary<UInt32, List<Tuple<Pin, bool>>> pinToWireDict, int level)
        {
            var connectedPins = pinToWireDict[pin.ElementId].Where(x => x.Item1 != pin && x.Item1 != root && connections.ContainsKey(x.Item1.ElementId) == false);

            foreach (var p in connectedPins)
            {
                connections.Add(p.Item1.ElementId, p);

                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print("{0}    Pin: {1} | Inverted: {2} | SimulationParent: {3} | Type: {4}",
                        new string(' ', level),
                        p.Item1.ElementId,
                        p.Item2,
                        p.Item1.SimulationParent.ElementId,
                        p.Item1.Type);
                }

                if (p.Item1.Type == PinType.Undefined && pinToWireDict.ContainsKey(pin.ElementId) == true)
                {
                    Find(root, p.Item1, connections, pinToWireDict, level + 4);
                }
            }
        }

        public void Find(Element[] elements)
        {
            if (SimulationSettings.EnableDebug)
            {
                Debug.Print("");
                Debug.Print("--- FindConnections(), elements.Count: {0}", elements.Count());
                Debug.Print("");
            }

            var pinToWireDict = MapPinsToWires(elements);

            var pins = elements.Where(x => x is IStateSimulation && x.Children != null)
                               .SelectMany(x => x.Children)
                               .Cast<Pin>()
                               .Where(p => (p.Type == PinType.Undefined || p.Type == PinType.Input) && pinToWireDict.ContainsKey(p.ElementId))
                               .ToArray();

            var lenght = pins.Length;

            for (int i = 0; i < lenght; i++)
            {
                var pin = pins[i];

                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print("Pin  {0} | SimulationParent: {1} | Type: {2}",
                        pin.ElementId,
                        (pin.SimulationParent != null) ? pin.SimulationParent.ElementId : UInt32.MaxValue,
                        pin.Type);
                }

                var connections = new Dictionary<UInt32, Tuple<Pin, bool>>();

                Find(pin, pin, connections, pinToWireDict, 0);

                if (connections.Count > 0)
                {
                    pin.Connections = connections.Values.ToArray();
                }
                else
                {
                    pin.Connections = null;
                }
            }

            if (SimulationSettings.EnableDebug)
            {
                Debug.Print("");
            }

            pinToWireDict = null;
            pins = null;
        }

        #endregion

        #region Reset Connections

        public void Reset(IEnumerable<Pin> pins)
        {
            foreach (var pin in pins)
            {
                if (pin.IsPinTypeUndefined)
                {
                    pin.Connections = null;
                    pin.Type = PinType.Undefined;
                }
                else
                {
                    pin.Connections = null;
                }
            }
        }

        #endregion
    }

    public interface ISimulationController
    {
        Compiler Compiler { get; }
        SimulationContext SimulationContext { get; set; }
        bool IsConsole { get; set; }

        void Run(List<Context> contexts, IEnumerable<Tag> tags);
        void Run(List<Context> contexts, IEnumerable<Tag> tags, int period, Action update);
        void Stop();
        void Reset(bool collect);
        void ResetTimerAndClock();
    }

    public class SimulationController : ISimulationController
    {
        #region Properties

        public Compiler Compiler { get; private set; }
        public SimulationContext SimulationContext { get; set; }
        public bool IsConsole { get; set; }

        #endregion

        #region Constructor

        public SimulationController()
        {
            Compiler = new Compiler();
        }

        #endregion

        #region Run

        private void Run(IEnumerable<Context> contexts, IEnumerable<Tag> tags, bool showInfo)
        {
            // print simulation info
            if (showInfo)
            {
                // get total number of elements for simulation
                var elements = contexts.SelectMany(x => x.Children).Concat(tags);

                Debug.Print("Simulation for {0} contexts, elements: {1}", contexts.Count(), elements.Count());
                Debug.Print("Debug Simulation Enabled: {0}", SimulationSettings.EnableDebug);
                Debug.Print("Have Cache: {0}", SimulationContext.Cache == null ? false : SimulationContext.Cache.HaveCache);
            }

            if (SimulationContext.Cache == null || SimulationContext.Cache.HaveCache == false)
            {
                // compile simulation for contexts
                SimulationContext.Cache = Compiler.Compile(contexts, tags, SimulationContext.SimulationClock);
            }

            if (SimulationContext.Cache != null || SimulationContext.Cache.HaveCache == true)
            {
                // run simulation for contexts
                Run(SimulationContext.Cache);
            }
        }

        private void Run(Action<object> action, object contexts, object tags, TimeSpan period)
        {
            SimulationContext.SimulationClock.Cycle = 0;
            SimulationContext.SimulationClock.Resolution = (int)period.TotalMilliseconds;

            SimulationContext.SimulationTimerSync = new object();

            var virtualTime = new TimeSpan(0);
            var realTime = System.Diagnostics.Stopwatch.StartNew();
            var dt = DateTime.Now;

            SimulationContext.SimulationTimer = new System.Threading.Timer(
                (s) =>
                {
                    lock (SimulationContext.SimulationTimerSync)
                    {
                        SimulationContext.SimulationClock.Cycle++;
                        virtualTime = virtualTime.Add(period);

                        var sw = System.Diagnostics.Stopwatch.StartNew();
                        action(s);
                        sw.Stop();

                        if (IsConsole)
                        {
                            Console.Title = string.Format("Cycle {0} | {1}ms | vt:{2} rt:{3} dt:{4} id:{5}",
                                SimulationContext.SimulationClock.Cycle,
                                sw.Elapsed.TotalMilliseconds,
                                virtualTime.TotalMilliseconds,
                                realTime.Elapsed.TotalMilliseconds,
                                DateTime.Now - dt,
                                System.Threading.Thread.CurrentThread.ManagedThreadId);
                        }

                        //if (SimulationSettings.EnableDebug)
                        //{
                        Debug.Print("Cycle {0} | {1}ms | vt:{2} rt:{3} dt:{4} id:{5}",
                            SimulationContext.SimulationClock.Cycle,
                            sw.Elapsed.TotalMilliseconds,
                            virtualTime.TotalMilliseconds,
                            realTime.Elapsed.TotalMilliseconds,
                            DateTime.Now - dt,
                            System.Threading.Thread.CurrentThread.ManagedThreadId);
                        //}
                    }
                },
                contexts,
                TimeSpan.FromMilliseconds(0),
                period);
        }

        private void RunWithLogEnabled(Action run, string message)
        {
            string logPath = string.Format("run-{0:yyyy-MM-dd_HH-mm-ss-fff}.log", DateTime.Now);
            var consoleOut = Console.Out;

            Debug.Print("{1}: {0}", message, logPath);
            try
            {
                using (var writer = new System.IO.StreamWriter(logPath))
                {
                    Console.SetOut(writer);
                    run();
                }
            }
            finally
            {
                Console.SetOut(consoleOut);
            }
            Debug.Print("Done {0}.", message);
        }

        public void Run(List<Context> contexts, IEnumerable<Tag> tags, int period, Action update)
        {
            ResetTimerAndClock();

            var action = new Action(() =>
            {
                Run(contexts, tags, false);
                Run((state) =>
                {
                    Run(state as List<Context>, tags, false);
                    update();
                }, contexts, tags, TimeSpan.FromMilliseconds(period));
            });

            if (SimulationSettings.EnableLog)
                RunWithLogEnabled(action, "Run");
            else
                action();
        }

        public void Run(List<Context> contexts, IEnumerable<Tag> tags)
        {
            ResetTimerAndClock();

            if (SimulationSettings.EnableLog)
                RunWithLogEnabled(() => Run(contexts, tags, true), "Run");
            else
                Run(contexts, tags, true);
        }

        public void Run(ISimulation[] simulations)
        {
            if (SimulationSettings.EnableDebug)
            {
                Debug.Print("");
            }

            if (simulations == null)
            {
                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print("--- warning: no ISimulation elements ---");
                    Debug.Print("");
                }

                return;
            }

            var lenght = simulations.Length;

            if (SimulationSettings.EnableDebug)
            {
                Debug.Print("");
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 0; i < lenght; i++)
            {
                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print("--- simulation: {0} | Type: {1} ---", simulations[i].Element.ElementId, simulations[i].GetType());
                    Debug.Print("");
                }

                simulations[i].Calculate();
            }

            sw.Stop();

            if (SimulationSettings.EnableDebug)
            {
                Debug.Print("Calculate() done in: {0}ms | {1} elements", sw.Elapsed.TotalMilliseconds, lenght);
            }
        }

        public void Run(SimulationCache cache)
        {
            if (cache == null || cache.HaveCache == false)
                return;

            Run(cache.Simulations);
        }

        #endregion

        #region Stop

        public void Stop()
        {
            if (SimulationContext != null &&
            SimulationContext.SimulationTimer != null)
            {
                SimulationContext.SimulationTimer.Dispose();
            }
        }

        #endregion

        #region Reset

        public void Reset(bool collect)
        {
            // reset simulation cache
            if (SimulationContext.Cache != null)
            {
                SimulationCache.Reset(SimulationContext.Cache);

                SimulationContext.Cache = null;
            }

            // collect memory
            if (collect)
            {
                System.GC.Collect();
            }
        }

        public void ResetTimerAndClock()
        {
            // stop simulation timer
            if (SimulationContext.SimulationTimer != null)
            {
                SimulationContext.SimulationTimer.Dispose();
            }

            // reset simulation clock
            SimulationContext.SimulationClock.Cycle = 0;
            SimulationContext.SimulationClock.Resolution = 0;
        }

        #endregion
    }
}
