using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulation.Core;

namespace Simulation.Core
{
    #region Simulation.Core

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
        public string Name{ get; set; }
        public string FactoryName { get; set; }
        public Element Parent { get; set; }
        public ObservableCollection<Element> Children { get; set; }
        public Element SimulationParent { get; set; }
    }

    public interface ITimer
    {
        float Delay { get; set; }
        string Unit { get; set; }
    }

    public class Property
    {
        public Property() : base() { }
        public Property(object data) : this() { Data = data; }
        public object Data { get; set; }
    }

    public enum PinType
    {
        Undefined,
        Input,
        Output
    }

    public class Tag : Element, IStateSimulation
    {
        public Tag() : base() { Properties = new Dictionary<string, Property>(); }
        public IDictionary<string, Property> Properties { get; set; }
        public ISimulation Simulation { get; set; }
    }

    public class Pin : Element
    {
        public Pin() : base() { }
        public bool IsPinTypeUndefined { get; set; }
        public PinType Type { get; set; }
        public Tuple<Pin, bool>[] Connections { get; set; } // bool is flag for Inverted
    }

    public class Signal : Element
    {
        public Signal() : base() { }
        public Tag Tag { get; set; }
    }

    public class Wire : Element
    {
        public Wire() : base() 
        {
            InvertStart = false;
            InvertEnd = false;
        }
        private Pin start;
        public Pin Start
        {
            get { return start; }
            set
            {
                if (value != start)
                {
                    if (start != null)
                        Children.Remove(start);

                    start = value;

                    if (start != null)
                        Children.Add(start);
                }
            }
        }
        private Pin end;
        public Pin End
        {
            get { return end; }
            set
            {
                if (value != end)
                {
                    if (end != null)
                        Children.Remove(end);

                    end = value;

                    if (end != null)
                        Children.Add(end);
                }
            }
        }
        public bool InvertStart { get; set; }
        public bool InvertEnd { get; set; }
    }

    public class AndGate : Element, IStateSimulation
    {
        public AndGate() : base() { }
        public ISimulation Simulation { get; set; }
    }

    public class OrGate : Element, IStateSimulation
    {
        public OrGate() : base() { }
        public ISimulation Simulation { get; set; }
    }

    public class NotGate : Element, IStateSimulation
    {
        public NotGate() : base() { }
        public ISimulation Simulation { get; set; }
    }

    public class BufferGate : Element, IStateSimulation
    {
        public BufferGate() : base() { }
        public ISimulation Simulation { get; set; }
    }

    public class NandGate : Element, IStateSimulation
    {
        public NandGate() : base() { }
        public ISimulation Simulation { get; set; }
    }

    public class NorGate : Element, IStateSimulation
    {
        public NorGate() : base() { }
        public ISimulation Simulation { get; set; }
    }

    public class XorGate : Element, IStateSimulation
    {
        public XorGate() : base() { }
        public ISimulation Simulation { get; set; }
    }

    public class XnorGate : Element, IStateSimulation
    {
        public XnorGate() : base() { }
        public ISimulation Simulation { get; set; }
    }

    public class MemoryResetPriority : Element, IStateSimulation
    {
        public MemoryResetPriority() : base() { }
        public ISimulation Simulation { get; set; }
    }

    public class MemorySetPriority : Element, IStateSimulation
    {
        public MemorySetPriority() : base() { }
        public ISimulation Simulation { get; set; }
    }

    public class TimerOn : Element, ITimer, IStateSimulation
    {
        public TimerOn() : base() { }
        public float Delay { get; set; }
        public string Unit { get; set; }
        public ISimulation Simulation { get; set; }
    }

    public class TimerOff : Element, ITimer, IStateSimulation
    {
        public TimerOff() : base() { }
        public float Delay { get; set; }
        public string Unit { get; set; }
        public ISimulation Simulation { get; set; }
    }

    public class TimerPulse : Element, ITimer, IStateSimulation
    {
        public TimerPulse() : base() { }
        public float Delay { get; set; }
        public string Unit { get; set; }
        public ISimulation Simulation { get; set; }
    }

    public class Context : Element
    {
        public Context() : base() { }
    }

    public class Project : Element
    {
        public Project() : base() { }
    }

    public class Solution : Element
    {
        public Solution() : base()  { Tags = new ObservableCollection<Tag>(); }
        public ObservableCollection<Tag> Tags { get; set; }
        public Tag DefaultTag { get; set; }
    }

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

    public class SimulationContext
    {
        public System.Threading.Timer SimulationTimer { get; set; }
        public object SimulationTimerSync { get; set; }
        public IClock SimulationClock { get; set; }
        public SimulationCache Cache { get; set; }
    }

    public class TagSimulation : ISimulation
    {
        #region Constructor

        public TagSimulation()
            : base()
        {
            this.InitialState = false;
        }

        #endregion

        #region Properties

        public Element Element { get; set; }

        #endregion

        #region ISimulation

        public IClock Clock { get; set; }

        public IBoolState State { get; set; }

        public bool? InitialState { get; set; }
        public Tuple<IBoolState, bool>[] StatesCache { get; set; }
        public bool HaveCache { get; set; }

        public Element[] DependsOn { get; set; }

        public void Compile()
        {
            if (HaveCache)
                Reset();

            var tag = Element as Tag;

            // TODO: Tag can only have one input
            Pin input = tag.Children.Cast<Pin>().Where(x => x.Type == PinType.Input && x.Connections != null && x.Connections.Length > 0).FirstOrDefault();
            //IEnumerable<Pin> outputs = tag.Children.Cast<Pin>().Where(x => x.Type == PinType.Output);

            if (input == null || input.Connections == null || input.Connections.Length <= 0)
            {
                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print("No Valid Input/Connections for Id: {0} | State: {1}", Element.ElementId, State.State);
                }

                return;
            }

            // get all connected inputs with possible state
            var connections = input.Connections.Where(x => x.Item1.Type == PinType.Output);

            // set ISimulation dependencies (used for topological sort)
            DependsOn = connections.Where(x => x.Item1 != null).Select(y => y.Item1.SimulationParent).Take(1).ToArray();

            if (SimulationSettings.EnableDebug)
            {
                foreach (var connection in connections)
                {
                    Debug.Print("Pin: {0} | Inverted: {1} | SimulationParent: {2} | Type: {3}",
                    connection.Item1.ElementId,
                    connection.Item2,
                    (connection.Item1.SimulationParent != null) ? connection.Item1.SimulationParent.ElementId : UInt32.MaxValue,
                    connection.Item1.Type);
                }
            }

            // get all connected inputs with state
            var states = connections.Select(x => new Tuple<IBoolState, bool>((x.Item1.SimulationParent as IStateSimulation).Simulation.State, x.Item2)).ToArray();

            if (states.Length == 1)
            {
                StatesCache = states;
                HaveCache = true;
            }
            else
            {
                // invalidate state
                State = null;

                StatesCache = null;
                HaveCache = false;
            }
        }

        public void Calculate()
        {
            if (HaveCache)
            {
                // calculate new state
                var first = StatesCache[0];

                State.State = first.Item2 ? !(first.Item1.State) : first.Item1.State;

                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print("Id: {0} | State: {1}", Element.ElementId, State.State);
                    Debug.Print("");
                }
            }
        }

        public void Reset()
        {
            HaveCache = false;
            StatesCache = null;
            State = null;
            Clock = null;
        }

        #endregion
    }

    public class AndGateSimulation : ISimulation
    {
        #region Constructor

        public AndGateSimulation()
            : base()
        {
            this.InitialState = null;
        }

        #endregion

        #region ISimulation

        public Element Element { get; set; }

        public IClock Clock { get; set; }

        public IBoolState State { get; set; }
        public bool? InitialState { get; set; }
        public Tuple<IBoolState, bool>[] StatesCache { get; set; }
        public bool HaveCache { get; set; }

        public Element[] DependsOn { get; set; }

        public void Compile()
        {
            if (HaveCache)
                Reset();

            // get all connected inputs with possible state
            var connections = Element.Children.Cast<Pin>()
                                              .Where(pin => pin.Connections != null && pin.Type == PinType.Input)
                                              .SelectMany(pin => pin.Connections)
                                              .Where(x => x.Item1.Type == PinType.Output);

            // set ISimulation dependencies (used for topological sort)
            DependsOn = connections.Select(y => y.Item1.SimulationParent).ToArray();

            if (SimulationSettings.EnableDebug)
            {
                foreach (var connection in connections)
                {
                    Debug.Print("Pin: {0} | Inverted: {1} | SimulationParent: {2} | Type: {3}",
                    connection.Item1.ElementId,
                    connection.Item2,
                    (connection.Item1.SimulationParent != null) ? connection.Item1.SimulationParent.ElementId : UInt32.MaxValue,
                    connection.Item1.Type);
                }
            }

            // get all connected inputs with state, where Tuple<IBoolState,bool> is IBoolState and Inverted
            var states = connections.Select(x => new Tuple<IBoolState, bool>((x.Item1.SimulationParent as IStateSimulation).Simulation.State, x.Item2)).ToArray();

            if (states.Length > 0)
            {
                StatesCache = states;
                HaveCache = true;
            }
            else
            {
                // invalidate state
                State = null;

                StatesCache = null;
                HaveCache = false;
            }
        }

        public void Calculate()
        {
            if (HaveCache)
            {
                // calculate new state
                State.State = CalculateState(StatesCache);

                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print("Id: {0} | State: {1}", Element.ElementId, State.State);
                    Debug.Print("");
                }
            }
        }

        private static bool? CalculateState(Tuple<IBoolState, bool>[] states)
        {
            int lenght = states.Length;
            if (lenght == 1)
                return null;

            bool? result = null;
            for (int i = 0; i < lenght; i++)
            {
                var item = states[i];
                var state = item.Item1.State;
                var isInverted = item.Item2;

                if (i == 0)
                    result = isInverted ? !(state) : state;
                else
                    result &= isInverted ? !(state) : state;
            }

            return result;
        }

        public void Reset()
        {
            HaveCache = false;
            StatesCache = null;
            State = null;
            Clock = null;
        }

        #endregion
    }

    public class OrGateSimulation : ISimulation
    {
        #region Constructor

        public OrGateSimulation()
            : base()
        {
            this.InitialState = null;
        }

        #endregion

        #region ISimulation

        public Element Element { get; set; }

        public IClock Clock { get; set; }

        public IBoolState State { get; set; }
        public bool? InitialState { get; set; }
        public Tuple<IBoolState, bool>[] StatesCache { get; set; }
        public bool HaveCache { get; set; }

        public Element[] DependsOn { get; set; }

        public void Compile()
        {
            if (HaveCache)
                Reset();

            // get all connected inputs with possible state
            var connections = Element.Children.Cast<Pin>()
                                              .Where(pin => pin.Connections != null && pin.Type == PinType.Input)
                                              .SelectMany(pin => pin.Connections)
                                              .Where(x => x.Item1.Type == PinType.Output);

            // set ISimulation dependencies (used for topological sort)
            DependsOn = connections.Select(y => y.Item1.SimulationParent).ToArray();

            if (SimulationSettings.EnableDebug)
            {
                foreach (var connection in connections)
                {
                    Debug.Print("Pin: {0} | Inverted: {1} | SimulationParent: {2} | Type: {3}",
                    connection.Item1.ElementId,
                    connection.Item2,
                    (connection.Item1.SimulationParent != null) ? connection.Item1.SimulationParent.ElementId : UInt32.MaxValue,
                    connection.Item1.Type);
                }
            }

            // get all connected inputs with state, where Tuple<IBoolState,bool> is IBoolState and Inverted
            var states = connections.Select(x => new Tuple<IBoolState, bool>((x.Item1.SimulationParent as IStateSimulation).Simulation.State, x.Item2)).ToArray();

            if (states.Length > 0)
            {
                StatesCache = states;
                HaveCache = true;
            }
            else
            {
                // invalidate state
                State = null;

                StatesCache = null;
                HaveCache = false;
            }
        }

        public void Calculate()
        {
            if (HaveCache)
            {
                // calculate new state
                State.State = CalculateState(StatesCache);

                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print("Id: {0} | State: {1}", Element.ElementId, State.State);
                    Debug.Print("");
                }
            }
        }

        private static bool? CalculateState(Tuple<IBoolState, bool>[] states)
        {
            int lenght = states.Length;
            if (lenght == 1)
                return null;

            bool? result = null;
            for (int i = 0; i < lenght; i++)
            {
                var item = states[i];
                var state = item.Item1.State;
                var isInverted = item.Item2;

                if (i == 0)
                    result = isInverted ? !(state) : state;
                else
                    result |= isInverted ? !(state) : state;
            }

            return result;
        }

        public void Reset()
        {
            HaveCache = false;
            StatesCache = null;
            State = null;
            Clock = null;
        }

        #endregion
    }

    public class NotGateSimulation : ISimulation
    {
        #region Constructor

        public NotGateSimulation()
            : base()
        {
            this.InitialState = null;
        }

        #endregion

        #region ISimulation

        public Element Element { get; set; }

        public IClock Clock { get; set; }

        public IBoolState State { get; set; }
        public bool? InitialState { get; set; }
        public Tuple<IBoolState, bool>[] StatesCache { get; set; }
        public bool HaveCache { get; set; }

        public Element[] DependsOn { get; set; }

        public void Compile()
        {
            throw new NotImplementedException();
        }

        public void Calculate()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class BufferGateSimulation : ISimulation
    {
        #region Constructor

        public BufferGateSimulation()
            : base()
        {
            this.InitialState = null;
        }

        #endregion

        #region ISimulation

        public Element Element { get; set; }

        public IClock Clock { get; set; }

        public IBoolState State { get; set; }
        public bool? InitialState { get; set; }
        public Tuple<IBoolState, bool>[] StatesCache { get; set; }
        public bool HaveCache { get; set; }

        public Element[] DependsOn { get; set; }

        public void Compile()
        {
            throw new NotImplementedException();
        }

        public void Calculate()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class NandGateSimulation : ISimulation
    {
        #region Constructor

        public NandGateSimulation()
            : base()
        {
            this.InitialState = null;
        }

        #endregion

        #region ISimulation

        public Element Element { get; set; }

        public IClock Clock { get; set; }

        public IBoolState State { get; set; }
        public bool? InitialState { get; set; }
        public Tuple<IBoolState, bool>[] StatesCache { get; set; }
        public bool HaveCache { get; set; }

        public Element[] DependsOn { get; set; }

        public void Compile()
        {
            throw new NotImplementedException();
        }

        public void Calculate()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class NorGateSimulation : ISimulation
    {
        #region Constructor

        public NorGateSimulation()
            : base()
        {
            this.InitialState = null;
        }

        #endregion

        #region ISimulation

        public Element Element { get; set; }

        public IClock Clock { get; set; }

        public IBoolState State { get; set; }
        public bool? InitialState { get; set; }
        public Tuple<IBoolState, bool>[] StatesCache { get; set; }
        public bool HaveCache { get; set; }

        public Element[] DependsOn { get; set; }

        public void Compile()
        {
            throw new NotImplementedException();
        }

        public void Calculate()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class XorGateSimulation : ISimulation
    {
        #region Constructor

        public XorGateSimulation()
            : base()
        {
            this.InitialState = null;
        }

        #endregion

        #region ISimulation

        public Element Element { get; set; }

        public IClock Clock { get; set; }

        public IBoolState State { get; set; }
        public bool? InitialState { get; set; }
        public Tuple<IBoolState, bool>[] StatesCache { get; set; }
        public bool HaveCache { get; set; }

        public Element[] DependsOn { get; set; }

        public void Compile()
        {
            throw new NotImplementedException();
        }

        public void Calculate()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class XnorGateSimulation : ISimulation
    {
        #region Constructor

        public XnorGateSimulation()
            : base()
        {
            this.InitialState = null;
        }

        #endregion

        #region ISimulation

        public Element Element { get; set; }

        public IClock Clock { get; set; }

        public IBoolState State { get; set; }
        public bool? InitialState { get; set; }
        public Tuple<IBoolState, bool>[] StatesCache { get; set; }
        public bool HaveCache { get; set; }

        public Element[] DependsOn { get; set; }

        public void Compile()
        {
            throw new NotImplementedException();
        }

        public void Calculate()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class MemoryResetPrioritySimulation : ISimulation
    {
        #region Constructor

        public MemoryResetPrioritySimulation()
            : base()
        {
            this.InitialState = null;
        }

        #endregion

        #region ISimulation

        public Element Element { get; set; }

        public IClock Clock { get; set; }

        public IBoolState State { get; set; }
        public bool? InitialState { get; set; }
        public Tuple<IBoolState, bool>[] StatesCache { get; set; }
        public bool HaveCache { get; set; }

        public Element[] DependsOn { get; set; }

        public void Compile()
        {
            throw new NotImplementedException();
        }

        public void Calculate()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class MemorySetPrioritySimulation : ISimulation
    {
        #region Constructor

        public MemorySetPrioritySimulation()
            : base()
        {
            this.InitialState = null;
        }

        #endregion

        #region ISimulation

        public Element Element { get; set; }

        public IClock Clock { get; set; }

        public IBoolState State { get; set; }
        public bool? InitialState { get; set; }
        public Tuple<IBoolState, bool>[] StatesCache { get; set; }
        public bool HaveCache { get; set; }

        public Element[] DependsOn { get; set; }

        public void Compile()
        {
            throw new NotImplementedException();
        }

        public void Calculate()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class TimerOnSimulation : ISimulation
    {
        #region Constructor

        public TimerOnSimulation()
            : base()
        {
            this.InitialState = false;
        }

        #endregion

        #region ISimulation

        public Element Element { get; set; }

        public IClock Clock { get; set; }

        public IBoolState State { get; set; }
        public bool? InitialState { get; set; }
        public Tuple<IBoolState, bool>[] StatesCache { get; set; }
        public bool HaveCache { get; set; }

        public Element[] DependsOn { get; set; }

        public void Compile()
        {
            if (HaveCache)
                Reset();

            // only one input is allowed for timer
            var inputs = Element.Children.Cast<Pin>().Where(pin => pin.Connections != null && pin.Type == PinType.Input);

            if (inputs == null || inputs.Count() != 1)
            {
                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print("No Valid Input for Id: {0} | State: {1}", Element.ElementId, State.State);
                }

                return;
            }

            // get all connected inputs with possible state
            var connections = inputs.First().Connections.Where(x => x.Item1.Type == PinType.Output);

            // set ISimulation dependencies (used for topological sort)
            DependsOn = connections.Where(x => x.Item1 != null).Select(y => y.Item1.SimulationParent).Take(1).ToArray();

            if (SimulationSettings.EnableDebug)
            {
                foreach (var connection in connections)
                {
                    Debug.Print("Pin: {0} | Inverted: {1} | SimulationParent: {2} | Type: {3}",
                    connection.Item1.ElementId,
                    connection.Item2,
                    (connection.Item1.SimulationParent != null) ? connection.Item1.SimulationParent.ElementId : UInt32.MaxValue,
                    connection.Item1.Type);
                }
            }

            // get all connected inputs with state
            var states = connections.Select(x => new Tuple<IBoolState, bool>((x.Item1.SimulationParent as IStateSimulation).Simulation.State, x.Item2)).ToArray();

            if (states.Length == 1)
            {
                StatesCache = states;
                HaveCache = true;
            }
            else
            {
                // invalidate state
                State = null;

                StatesCache = null;
                HaveCache = false;
            }
        }

        private bool IsEnabled;
        private long EndCycle;

        public void Calculate()
        {
            if (HaveCache)
            {
                // calculate new state
                var first = StatesCache[0];
                bool? enableState = first.Item2 ? !(first.Item1.State) : first.Item1.State;

                switch (enableState)
                {
                    case true:
                        {
                            if (IsEnabled)
                            {
                                if (Clock.Cycle >= EndCycle && State.State != true)
                                {
                                    State.State = true;
                                }
                            }
                            else
                            {
                                // Delay -> in seconds
                                // Clock.Cycle
                                // Clock.Resolution -> in milsisecond
                                long cyclesDelay = (long)((Element as ITimer).Delay * 1000) / Clock.Resolution;
                                EndCycle = Clock.Cycle + cyclesDelay;

                                IsEnabled = true;

                                if (Clock.Cycle >= EndCycle)
                                {
                                    State.State = true;
                                }
                            }
                        }
                        break;
                    case false:
                        {
                            IsEnabled = false;
                            State.State = false;
                        }
                        break;
                    case null:
                        {
                            IsEnabled = false;
                            State.State = null;
                        }
                        break;
                }

                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print("Id: {0} | State: {1}", Element.ElementId, State.State);
                    Debug.Print("");
                }
            }
        }

        public void Reset()
        {
            HaveCache = false;
            StatesCache = null;
            State = null;
            Clock = null;
        }

        #endregion
    }

    public class TimerOffSimulation : ISimulation
    {
        #region Constructor

        public TimerOffSimulation()
            : base()
        {
            this.InitialState = false;
        }

        #endregion

        #region ISimulation

        public Element Element { get; set; }

        public IClock Clock { get; set; }

        public IBoolState State { get; set; }
        public bool? InitialState { get; set; }
        public Tuple<IBoolState, bool>[] StatesCache { get; set; }
        public bool HaveCache { get; set; }

        public Element[] DependsOn { get; set; }

        public void Compile()
        {
            if (HaveCache)
                Reset();

            // only one input is allowed for timer
            var inputs = Element.Children.Cast<Pin>().Where(pin => pin.Connections != null && pin.Type == PinType.Input);

            if (inputs == null || inputs.Count() != 1)
            {
                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print("No Valid Input for Id: {0} | State: {1}", Element.ElementId, State.State);
                }

                return;
            }

            // get all connected inputs with possible state
            var connections = inputs.First().Connections.Where(x => x.Item1.Type == PinType.Output);

            // set ISimulation dependencies (used for topological sort)
            DependsOn = connections.Where(x => x.Item1 != null).Select(y => y.Item1.SimulationParent).Take(1).ToArray();

            if (SimulationSettings.EnableDebug)
            {
                foreach (var connection in connections)
                {
                    Debug.Print("Pin: {0} | Inverted: {1} | SimulationParent: {2} | Type: {3}",
                    connection.Item1.ElementId,
                    connection.Item2,
                    (connection.Item1.SimulationParent != null) ? connection.Item1.SimulationParent.ElementId : UInt32.MaxValue,
                    connection.Item1.Type);
                }
            }

            // get all connected inputs with state
            var states = connections.Select(x => new Tuple<IBoolState, bool>((x.Item1.SimulationParent as IStateSimulation).Simulation.State, x.Item2)).ToArray();

            if (states.Length == 1)
            {
                StatesCache = states;
                HaveCache = true;
            }
            else
            {
                // invalidate state
                State = null;

                StatesCache = null;
                HaveCache = false;
            }
        }

        private bool IsEnabled;
        private bool IsLowEnabled;
        private long EndCycle;

        public void Calculate()
        {
            if (HaveCache)
            {
                // calculate new state
                var first = StatesCache[0];
                bool? enableState = first.Item2 ? !(first.Item1.State) : first.Item1.State;

                switch (enableState)
                {
                    case true:
                        {
                            if (IsEnabled == false && IsLowEnabled == false)
                            {
                                State.State = true;
                                IsEnabled = true;
                                IsLowEnabled = false;
                            }
                            else if (IsEnabled == true && IsLowEnabled == true && State.State != false)
                            {
                                if (Clock.Cycle >= EndCycle)
                                {
                                    State.State = false;
                                    IsEnabled = false;
                                    IsLowEnabled = false;
                                    break;
                                }
                            }
                        }
                        break;
                    case false:
                        {
                            if (IsEnabled == true && IsLowEnabled == false)
                            {
                                // Delay -> in seconds
                                // Clock.Cycle
                                // Clock.Resolution -> in milsisecond
                                long cyclesDelay = (long)((Element as ITimer).Delay * 1000) / Clock.Resolution;
                                EndCycle = Clock.Cycle + cyclesDelay;

                                IsLowEnabled = true;
                                break;
                            }
                            else if (IsEnabled == true && IsLowEnabled == true && State.State != false)
                            {
                                if (Clock.Cycle >= EndCycle)
                                {
                                    State.State = false;
                                    IsEnabled = false;
                                    IsLowEnabled = false;
                                    break;
                                }
                            }
                        }
                        break;
                    case null:
                        {
                            IsEnabled = false;
                            IsLowEnabled = false;
                            State.State = null;
                        }
                        break;
                }

                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print("Id: {0} | State: {1}", Element.ElementId, State.State);
                    Debug.Print("");
                }
            }
        }

        public void Reset()
        {
            HaveCache = false;
            StatesCache = null;
            State = null;
            Clock = null;
        }

        #endregion
    }

    public class TimerPulseSimulation : ISimulation
    {
        #region Constructor

        public TimerPulseSimulation()
            : base()
        {
            this.InitialState = false;
        }

        #endregion

        #region ISimulation

        public Element Element { get; set; }

        public IClock Clock { get; set; }

        public IBoolState State { get; set; }
        public bool? InitialState { get; set; }
        public Tuple<IBoolState, bool>[] StatesCache { get; set; }
        public bool HaveCache { get; set; }

        public Element[] DependsOn { get; set; }

        public void Compile()
        {
            if (HaveCache)
                Reset();

            // only one input is allowed for timer
            var inputs = Element.Children.Cast<Pin>().Where(pin => pin.Connections != null && pin.Type == PinType.Input);

            if (inputs == null || inputs.Count() != 1)
            {
                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print("No Valid Input for Id: {0} | State: {1}", Element.ElementId, State.State);
                }

                return;
            }

            // get all connected inputs with possible state
            var connections = inputs.First().Connections.Where(x => x.Item1.Type == PinType.Output);

            // set ISimulation dependencies (used for topological sort)
            DependsOn = connections.Where(x => x.Item1 != null).Select(y => y.Item1.SimulationParent).Take(1).ToArray();

            if (SimulationSettings.EnableDebug)
            {
                foreach (var connection in connections)
                {
                    Debug.Print("Pin: {0} | Inverted: {1} | SimulationParent: {2} | Type: {3}",
                    connection.Item1.ElementId,
                    connection.Item2,
                    (connection.Item1.SimulationParent != null) ? connection.Item1.SimulationParent.ElementId : UInt32.MaxValue,
                    connection.Item1.Type);
                }
            }

            // get all connected inputs with state
            var states = connections.Select(x => new Tuple<IBoolState, bool>((x.Item1.SimulationParent as IStateSimulation).Simulation.State, x.Item2)).ToArray();

            if (states.Length == 1)
            {
                StatesCache = states;
                HaveCache = true;
            }
            else
            {
                // invalidate state
                State = null;

                StatesCache = null;
                HaveCache = false;
            }
        }

        private bool IsEnabled;
        private bool IsReset;
        private long EndCycle;

        public void Calculate()
        {
            if (HaveCache)
            {
                // calculate new state
                var first = StatesCache[0];
                bool? enableState = first.Item2 ? !(first.Item1.State) : first.Item1.State;

                switch (enableState)
                {
                    case true:
                        {
                            if (IsEnabled)
                            {
                                if (Clock.Cycle >= EndCycle && State.State != false)
                                {
                                    IsEnabled = false;
                                    State.State = false;
                                    break;
                                }
                            }
                            else
                            {
                                if (IsReset == true)
                                {
                                    // Delay -> in seconds
                                    // Clock.Cycle
                                    // Clock.Resolution -> in milsisecond
                                    long cyclesDelay = (long)((Element as ITimer).Delay * 1000) / Clock.Resolution;
                                    EndCycle = Clock.Cycle + cyclesDelay;

                                    IsReset = false;

                                    if (Clock.Cycle >= EndCycle)
                                    {
                                        IsEnabled = false;
                                        State.State = false;
                                    }
                                    else
                                    {
                                        IsEnabled = true;
                                        State.State = true;
                                    }
                                }

                                break;
                            }
                        }
                        break;
                    case false:
                        {
                            IsReset = true;

                            if (IsEnabled)
                            {
                                if (Clock.Cycle >= EndCycle && State.State != false)
                                {
                                    State.State = false;
                                    IsEnabled = false;
                                    break;
                                }
                            }
                        }
                        break;
                    case null:
                        {
                            IsReset = true;
                            IsEnabled = false;
                            State.State = null;
                        }
                        break;
                }

                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print("Id: {0} | State: {1}", Element.ElementId, State.State);
                    Debug.Print("");
                }
            }
        }

        public void Reset()
        {
            HaveCache = false;
            StatesCache = null;
            State = null;
            Clock = null;
        }

        #endregion
    }

    public class Clock : IClock
    {
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

        public long Cycle { get; set; }
        public int Resolution { get; set; }
    }

    public class BoolState : IBoolState, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        public virtual void Notify(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        
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
    }

    public static class SimulationSettings
    {
        public static bool EnableDebug { get; set; }
        public static bool EnableLog { get; set; }
    }

    public static class Simulation
    {
        #region Connections by Id

        private static void FindPinConnections(Pin root,
            Pin pin,
            Dictionary<UInt32, Tuple<Pin, bool>> connections,
            Dictionary<UInt32, List<Tuple<Pin, bool>>> pinToWireDict,
            int level)
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
                    FindPinConnections(root, p.Item1, connections, pinToWireDict, level + 4);
                }
            }
        }

        private static Dictionary<UInt32, List<Tuple<Pin, bool>>> PinToWireConections(this Element[] elements)
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
                        dict.Add(startId, new List<Tuple<Pin, bool>>());

                    dict[startId].Add(new Tuple<Pin, bool>(start, inverted));
                    dict[startId].Add(new Tuple<Pin, bool>(end, inverted));

                    if (!dict.ContainsKey(endId))
                        dict.Add(endId, new List<Tuple<Pin, bool>>());

                    dict[endId].Add(new Tuple<Pin, bool>(start, inverted));
                    dict[endId].Add(new Tuple<Pin, bool>(end, inverted));
                }
            }

            return dict;
        }

        private static void FindConnections(Element[] elements)
        {
            if (SimulationSettings.EnableDebug)
            {
                Debug.Print("");
                Debug.Print("--- FindConnections(), elements.Count: {0}", elements.Count());
                Debug.Print("");
            }

            var pinToWireDict = elements.PinToWireConections();

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

                FindPinConnections(pin, pin, connections, pinToWireDict, 0);

                if (connections.Count > 0)
                    pin.Connections = connections.Values.ToArray();
                else
                    pin.Connections = null;
            }

            if (SimulationSettings.EnableDebug)
            {
                Debug.Print("");
            }

            pinToWireDict = null;
            pins = null;
        }

        public static void ResetConnections(IEnumerable<Pin> pins)
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

        #region State Simulation Dictionary

        public static Dictionary<Type, Func<Element, ISimulation>> StateSimulationDict =
            new Dictionary<Type, Func<Element, ISimulation>>()
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

        #region Simulation

        private static void ProcessInput(Pin input, string level)
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

        private static void ProcessOutput(Pin output, string level)
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

        private static void FindPinTypes(IEnumerable<Element> elements)
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

        private static void InitializeStates(List<ISimulation> simulations)
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

        private static void GenerateCompileCache(List<ISimulation> simulations, IClock clock)
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

        private static void Calculate(ISimulation[] simulations)
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

        private static SimulationCache Compile(Element[] elements, IClock clock)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var cache = new SimulationCache();

            // -- step 1: reset pin connections ---
            var pins = elements.Where(x => x is Pin).Cast<Pin>();

            Simulation.ResetConnections(pins);

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
            Simulation.FindConnections(elements);

            if (SimulationSettings.EnableDebug)
            {
                Debug.Print("--- elements with input connected ---");
                Debug.Print("");
            }

            // -- step 4: get ordered elements for simulation ---
            Simulation.FindPinTypes(elements);

            // -- step 5: initialize ISimulation states
            Simulation.InitializeStates(simulations);

            // -- step 6: complile each simulation ---
            Simulation.GenerateCompileCache(simulations, clock);

            // -- step 7: sort simulations using dependencies ---

            if (SimulationSettings.EnableDebug)
            {
                Debug.Print("-- dependencies ---");
                Debug.Print("");
            }

            var sortedSimulations = simulations.TopologicalSort(x =>
            {
                if (x.DependsOn == null)
                    return null;
                else
                    return x.DependsOn.Cast<IStateSimulation>().Select(y => y.Simulation);
            });

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

        #endregion

        #region Run

        public static SimulationCache Compile(IEnumerable<Context> contexts, IEnumerable<Tag> tags, IClock clock)
        {
            var elements = contexts.SelectMany(x => x.Children).Concat(tags).ToArray();

            // compile elements
            var cache = Simulation.Compile(elements, clock);

            // collect unused memory
            System.GC.Collect();

            return cache;
        }

        public static SimulationCache Compile(Context context, IEnumerable<Tag> tags, IClock clock)
        {
            var elements = context.Children.Concat(tags).ToArray();

            // compile elements
            var cache = Simulation.Compile(elements, clock);

            // collect unused memory
            System.GC.Collect();

            return cache;
        }

        public static void Run(SimulationCache cache)
        {
            if (cache == null || cache.HaveCache == false)
                return;

            Simulation.Calculate(cache.Simulations);
        }

        #endregion

        #region Topological Sort

        private static IEnumerable<T> TopologicalSort<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> dependencies)
        {
            var sorted = new List<T>();
            var visited = new HashSet<T>();

            foreach (var item in source)
            {
                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print("+ {0} depends on:", (item as ISimulation).Element.Name);
                }

                Visit(item, visited, sorted, dependencies);

                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print("");
                }
            }

            return sorted;
        }

        private static void Visit<T>(T item, HashSet<T> visited, List<T> sorted, Func<T, IEnumerable<T>> dependencies)
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

                        Visit(dep, visited, sorted, dependencies);
                    }

                    // add items with simulation dependencies
                    sorted.Add(item);
                }

                // add  items without simulation dependencies
                sorted.Add(item);
            }
            //else if (!sorted.Contains(item))
            //{
            //    Debug.Print("Invalid dependency cycle: {0}", (item as Element).Name);
            //}
        }

        #endregion
    }

    public static class SimulationFactory
    {
        #region Properties

        public static SimulationContext CurrentSimulationContext { get; set; }
        public static bool IsConsole { get; set; }

        #endregion

        #region Reset

        public static void Reset(bool collect)
        {
            // reset simulation cache
            if (CurrentSimulationContext.Cache != null)
            {
                SimulationCache.Reset(CurrentSimulationContext.Cache);

                CurrentSimulationContext.Cache = null;
            }

            // collect memory
            if (collect)
            {
                System.GC.Collect();
            }
        }

        #endregion

        #region Simulation

        private static void Run(IEnumerable<Context> contexts, IEnumerable<Tag> tags, bool showInfo)
        {
            // print simulation info
            if (showInfo)
            {
                // get total number of elements for simulation
                var elements = contexts.SelectMany(x => x.Children).Concat(tags);

                Debug.Print("Simulation for {0} contexts, elements: {1}", contexts.Count(), elements.Count());
                Debug.Print("Debug Simulation Enabled: {0}", SimulationSettings.EnableDebug);
                Debug.Print("Have Cache: {0}", CurrentSimulationContext.Cache == null ? false : CurrentSimulationContext.Cache.HaveCache);
            }

            if (CurrentSimulationContext.Cache == null || CurrentSimulationContext.Cache.HaveCache == false)
            {
                // compile simulation for contexts
                CurrentSimulationContext.Cache = Simulation.Compile(contexts, tags, CurrentSimulationContext.SimulationClock);
            }

            if (CurrentSimulationContext.Cache != null || CurrentSimulationContext.Cache.HaveCache == true)
            {
                // run simulation for contexts
                Simulation.Run(CurrentSimulationContext.Cache);
            }
        }

        private static void Run(Action<object> action, object contexts, object tags, TimeSpan period)
        {
            CurrentSimulationContext.SimulationClock.Cycle = 0;
            CurrentSimulationContext.SimulationClock.Resolution = (int)period.TotalMilliseconds;

            CurrentSimulationContext.SimulationTimerSync = new object();

            var virtualTime = new TimeSpan(0);
            var realTime = System.Diagnostics.Stopwatch.StartNew();
            var dt = DateTime.Now;

            CurrentSimulationContext.SimulationTimer = new System.Threading.Timer(
                (s) =>
                {
                    lock (CurrentSimulationContext.SimulationTimerSync)
                    {
                        CurrentSimulationContext.SimulationClock.Cycle++;
                        virtualTime = virtualTime.Add(period);

                        var sw = System.Diagnostics.Stopwatch.StartNew();
                        action(s);
                        sw.Stop();

                        if (IsConsole)
                        {
                            Console.Title = string.Format("Cycle {0} | {1}ms | vt:{2} rt:{3} dt:{4} id:{5}",
                                CurrentSimulationContext.SimulationClock.Cycle,
                                sw.Elapsed.TotalMilliseconds,
                                virtualTime.TotalMilliseconds,
                                realTime.Elapsed.TotalMilliseconds,
                                DateTime.Now - dt,
                                System.Threading.Thread.CurrentThread.ManagedThreadId);
                        }

                        /*
                        if (Settings.EnableDebug)
                        {
                            Debug.Print("Cycle {0} | {1}ms | vt:{2} rt:{3} dt:{4} id:{5}",
                                SimulationClock.Cycle,
                                sw.Elapsed.TotalMilliseconds,
                                virtualTime.TotalMilliseconds,
                                realTime.Elapsed.TotalMilliseconds,
                                DateTime.Now - dt,
                                System.Threading.Thread.CurrentThread.ManagedThreadId);
                        }
                        */

                        Debug.Print("Cycle {0} | {1}ms | vt:{2} rt:{3} dt:{4} id:{5}",
                            CurrentSimulationContext.SimulationClock.Cycle,
                            sw.Elapsed.TotalMilliseconds,
                            virtualTime.TotalMilliseconds,
                            realTime.Elapsed.TotalMilliseconds,
                            DateTime.Now - dt,
                            System.Threading.Thread.CurrentThread.ManagedThreadId);
                    }
                },
                contexts,
                TimeSpan.FromMilliseconds(0),
                period);
        }

        private static void LogRun(Action run, string message)
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

        public static void Run(List<Context> contexts, IEnumerable<Tag> tags, int period, Action update)
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
                LogRun(action, "Run");
            else
                action();
        }

        public static void Run(List<Context> contexts, IEnumerable<Tag> tags)
        {
            ResetTimerAndClock();

            if (SimulationSettings.EnableLog)
                LogRun(() => Run(contexts, tags, true), "Run");
            else
                Run(contexts, tags, true);
        }

        public static void Stop()
        {
            if (CurrentSimulationContext != null &&
            CurrentSimulationContext.SimulationTimer != null)
            {
                CurrentSimulationContext.SimulationTimer.Dispose();
            }
        }

        public static void ResetTimerAndClock()
        {
            // stop simulation timer
            if (CurrentSimulationContext.SimulationTimer != null)
            {
                CurrentSimulationContext.SimulationTimer.Dispose();
            }

            // reset simulation clock
            CurrentSimulationContext.SimulationClock.Cycle = 0;
            CurrentSimulationContext.SimulationClock.Resolution = 0;
        }

        #endregion
    }

    #endregion
}

namespace Simulation.Tests
{
    using Sheet;

    #region Tests

    public class TestFactory
    {
        public Tag CreateSignalTag(string designation, string description, string signal, string condition)
        {
            var tag = new Tag() { Id = Guid.NewGuid().ToString() };
            tag.Properties.Add("Designation", new Property(designation));
            tag.Properties.Add("Description", new Property(description));
            tag.Properties.Add("Signal", new Property(signal));
            tag.Properties.Add("Condition", new Property(condition));
            return tag;
        }

        public Wire CreateWire(Context context, Pin start, Pin end)
        {
            if (context == null)
                throw new ArgumentException("context");

            var element = new Wire()
            {
                Start = start,
                End = end,
                X = 0,
                Y = 0,
                Id = Guid.NewGuid().ToString(),
                Parent = context
            };

            context.Children.Add(element);

            return element;
        }

        public Pin CreatePin(Context context,
            double x,
            double y,
            Element parent,
            string name = "",
            string factoryName = "",
            PinType type = PinType.Undefined,
            bool isPinTypeUndefined = true)
        {
            if (context == null)
                throw new ArgumentException("context");

            var element = new Pin()
            {
                Name = name,
                FactoryName = factoryName,
                X = x,
                Y = y,
                Id = Guid.NewGuid().ToString(),
                Parent = parent,
                Type = type,
                IsPinTypeUndefined = isPinTypeUndefined
            };

            if (parent != null && !(parent is Context))
            {
                parent.Children.Add(element);
            }

            context.Children.Add(element);

            return element;
        }

        public Signal CreateSignal(Context context, double x, double y, string name = "")
        {
            if (context == null)
                throw new ArgumentException("context");

            var element = new Signal()
            {
                Name = name,
                Id = Guid.NewGuid().ToString(),
                X = x,
                Y = y,
                Tag = null,
                Parent = context
            };

            context.Children.Add(element);

            // left, Input: Children[0]
            CreatePin(context, x, y + 15, element, "I", "I", PinType.Input, false);

            // right, Output: Children[1]
            CreatePin(context, x + 285, y + 15, element, "O", "O", PinType.Output, false);

            return element;
        }

        public AndGate CreateAndGate(Context context, double x, double y, string name = "")
        {
            if (context == null)
                throw new ArgumentException("context");

            var element = new AndGate()
            {
                Name = name,
                Id = Guid.NewGuid().ToString(),
                X = x,
                Y = y,
                Parent = context
            };

            context.Children.Add(element);

            // left
            CreatePin(context, x, y + 15, element, "L", "L");
            // right
            CreatePin(context, x + 30, y + 15, element, "R", "R");
            // top
            CreatePin(context, x + 15, y, element, "T", "T");
            // bottom
            CreatePin(context, x + 15, y + 30, element, "B", "B");

            return element;
        }

        public OrGate CreateOrGate(Context context, double x, double y, string name = "")
        {
            if (context == null)
                throw new ArgumentException("context");

            var element = new OrGate()
            {
                Name = name,
                Id = Guid.NewGuid().ToString(),
                X = x,
                Y = y,
                Parent = context
            };

            context.Children.Add(element);

            // left
            CreatePin(context, x, y + 15, element, "L", "L");
            // right
            CreatePin(context, x + 30, y + 15, element, "R", "R");
            // top
            CreatePin(context, x + 15, y, element, "T", "T");
            // bottom
            CreatePin(context, x + 15, y + 30, element, "B", "B");

            return element;
        }

        public NotGate CreateNotGate(Context context, double x, double y, string name = "")
        {
            if (context == null)
                throw new ArgumentException("context");

            var element = new NotGate()
            {
                Name = name,
                Id = Guid.NewGuid().ToString(),
                X = x,
                Y = y,
                Parent = context
            };

            context.Children.Add(element);

            // left
            CreatePin(context, x, y + 15, element, "L", "L");
            // right
            CreatePin(context, x + 40, y + 15, element, "R", "R");
            // top
            CreatePin(context, x + 15, y, element, "T", "T");
            // bottom
            CreatePin(context, x + 15, y + 30, element, "B", "B");

            return element;
        }

        public BufferGate CreateBufferGate(Context context, double x, double y, string name = "")
        {
            if (context == null)
                throw new ArgumentException("context");

            var element = new BufferGate()
            {
                Name = name,
                Id = Guid.NewGuid().ToString(),
                X = x,
                Y = y,
                Parent = context
            };

            context.Children.Add(element);

            // left
            CreatePin(context, x, y + 15, element, "L", "L");
            // right
            CreatePin(context, x + 30, y + 15, element, "R", "R");
            // top
            CreatePin(context, x + 15, y, element, "T", "T");
            // bottom
            CreatePin(context, x + 15, y + 30, element, "B", "B");

            return element;
        }

        public NandGate CreateNandGate(Context context, double x, double y, string name = "")
        {
            if (context == null)
                throw new ArgumentException("context");

            var element = new NandGate()
            {
                Name = name,
                Id = Guid.NewGuid().ToString(),
                X = x,
                Y = y,
                Parent = context
            };

            context.Children.Add(element);

            // left
            CreatePin(context, x, y + 15, element, "L", "L");
            // right
            CreatePin(context, x + 40, y + 15, element, "R", "R");
            // top
            CreatePin(context, x + 15, y, element, "T", "T");
            // bottom
            CreatePin(context, x + 15, y + 30, element, "B", "B");

            return element;
        }

        public NorGate CreateNorGate(Context context, double x, double y, string name = "")
        {
            if (context == null)
                throw new ArgumentException("context");

            var element = new NorGate()
            {
                Name = name,
                Id = Guid.NewGuid().ToString(),
                X = x,
                Y = y,
                Parent = context
            };

            context.Children.Add(element);

            // left
            CreatePin(context, x, y + 15, element, "L", "L");
            // right
            CreatePin(context, x + 40, y + 15, element, "R", "R");
            // top
            CreatePin(context, x + 15, y, element, "T", "T");
            // bottom
            CreatePin(context, x + 15, y + 30, element, "B", "B");

            return element;
        }

        public XorGate CreateXorGate(Context context, double x, double y, string name = "")
        {
            if (context == null)
                throw new ArgumentException("context");

            var element = new XorGate()
            {
                Name = name,
                Id = Guid.NewGuid().ToString(),
                X = x,
                Y = y,
                Parent = context
            };

            context.Children.Add(element);

            // left
            CreatePin(context, x, y + 15, element, "L", "L");
            // right
            CreatePin(context, x + 30, y + 15, element, "R", "R");
            // top
            CreatePin(context, x + 15, y, element, "T", "T");
            // bottom
            CreatePin(context, x + 15, y + 30, element, "B", "B");

            return element;
        }

        public XnorGate CreateXnorGate(Context context, double x, double y, string name = "")
        {
            if (context == null)
                throw new ArgumentException("context");

            var element = new XnorGate()
            {
                Name = name,
                Id = Guid.NewGuid().ToString(),
                X = x,
                Y = y,
                Parent = context
            };

            context.Children.Add(element);

            // left
            CreatePin(context, x, y + 15, element, "L", "L");
            // right
            CreatePin(context, x + 40, y + 15, element, "R", "R");
            // top
            CreatePin(context, x + 15, y, element, "T", "T");
            // bottom
            CreatePin(context, x + 15, y + 30, element, "B", "B");

            return element;
        }

        public MemorySetPriority CreateMemorySetPriority(Context context, double x, double y, string name = "")
        {
            if (context == null)
                throw new ArgumentException("context");

            var element = new MemorySetPriority()
            {
                Name = name,
                Id = Guid.NewGuid().ToString(),
                X = x,
                Y = y,
                Parent = context
            };

            context.Children.Add(element);

            // S
            CreatePin(context, x + 15, y, element, "S", "S");
            // R
            CreatePin(context, x + 45, y, element, "R", "R");
            // Q
            CreatePin(context, x + 15, y + 30, element, "Q", "Q");
            // Q'
            CreatePin(context, x + 45, y + 30, element, "NQ", "NQ");

            return element;
        }

        public MemoryResetPriority CreateMemoryResetPriority(Context context, double x, double y, string name = "")
        {
            if (context == null)
                throw new ArgumentException("context");

            var element = new MemoryResetPriority()
            {
                Name = name,
                Id = Guid.NewGuid().ToString(),
                X = x,
                Y = y,
                Parent = context
            };

            context.Children.Add(element);

            // S
            CreatePin(context, x + 15, y, element, "S", "S");
            // R
            CreatePin(context, x + 45, y, element, "R", "R");
            // Q
            CreatePin(context, x + 15, y + 30, element, "Q", "Q");
            // Q'
            CreatePin(context, x + 45, y + 30, element, "NQ", "NQ");

            return element;
        }

        public TimerPulse CreateTimerPulse(Context context, double x, double y, string name = "")
        {
            if (context == null)
                throw new ArgumentException("context");

            var element = new TimerPulse()
            {
                Name = name,
                Id = Guid.NewGuid().ToString(),
                X = x,
                Y = y,
                Parent = context
            };

            context.Children.Add(element);

            // left
            CreatePin(context, x, y + 15, element, "L", "L");
            // right
            CreatePin(context, x + 30, y + 15, element, "R", "R");
            // top
            CreatePin(context, x + 15, y, element, "T", "T");
            // bottom
            CreatePin(context, x + 15, y + 30, element, "B", "B");

            return element;
        }

        public TimerOn CreateTimerOn(Context context, double x, double y, string name = "")
        {
            if (context == null)
                throw new ArgumentException("context");

            var element = new TimerOn()
            {
                Name = name,
                Id = Guid.NewGuid().ToString(),
                X = x,
                Y = y,
                Parent = context
            };

            context.Children.Add(element);

            // left
            CreatePin(context, x, y + 15, element, "L", "L");
            // right
            CreatePin(context, x + 30, y + 15, element, "R", "R");
            // top
            CreatePin(context, x + 15, y, element, "T", "T");
            // bottom
            CreatePin(context, x + 15, y + 30, element, "B", "B");

            return element;
        }

        public TimerOff CreateTimerOff(Context context, double x, double y, string name = "")
        {
            if (context == null)
                throw new ArgumentException("context");

            var element = new TimerOff()
            {
                Name = name,
                Id = Guid.NewGuid().ToString(),
                X = x,
                Y = y,
                Parent = context
            };

            context.Children.Add(element);

            // left
            CreatePin(context, x, y + 15, element, "L", "L");
            // right
            CreatePin(context, x + 30, y + 15, element, "R", "R");
            // top
            CreatePin(context, x + 15, y, element, "T", "T");
            // bottom
            CreatePin(context, x + 15, y + 30, element, "B", "B");

            return element;
        }

        public Element CreateElementFromType(Context context, string type, double x, double y)
        {
            if (context == null)
                throw new ArgumentException("context");

            if (string.IsNullOrEmpty(type))
                throw new ArgumentException("type");

            Element element = null;

            switch (type)
            {
                case "Signal":
                    element = CreateSignal(context, x, y);
                    break;
                case "AndGate":
                    element = CreateAndGate(context, x, y);
                    break;
                case "OrGate":
                    element = CreateOrGate(context, x, y);
                    break;
                case "NotGate":
                    element = CreateNotGate(context, x, y);
                    break;
                case "BufferGate":
                    element = CreateBufferGate(context, x, y);
                    break;
                case "NandGate":
                    element = CreateNandGate(context, x, y);
                    break;
                case "NorGate":
                    element = CreateNorGate(context, x, y);
                    break;
                case "XorGate":
                    element = CreateXorGate(context, x, y);
                    break;
                case "XnorGate":
                    element = CreateXnorGate(context, x, y);
                    break;
                case "MemorySetPriority":
                    element = CreateMemorySetPriority(context, x, y);
                    break;
                case "MemoryResetPriority":
                    element = CreateMemoryResetPriority(context, x, y);
                    break;
                case "TimerPulse":
                    element = CreateTimerPulse(context, x, y);
                    break;
                case "TimerOn":
                    element = CreateTimerOn(context, x, y);
                    break;
                case "TimerOff":
                    element = CreateTimerOff(context, x, y);
                    break;
                default:
                    throw new ArgumentException("type");
            };

            return element;
        }
    }

    public class TestSimulation
    {
        #region Fields

        private Solution _solution;
        private Clock _clock;
        private int _periodInMillisencods;
        private bool _isSimulationRunning = false;

        #endregion

        #region Properties

        public Solution Solution { get { return _solution; } }
        public bool IsSimulationRunning { get { return _isSimulationRunning; } }

        #endregion

        #region Constructor

        public TestSimulation(Solution solution, int periodInMillisencods = 100)
        {
            _solution = solution;
            _periodInMillisencods = periodInMillisencods;
            Initialize();
        }

        #endregion

        #region Tags

        private void ResetTags()
        {
            if (_solution == null || _solution.Tags == null)
                return;

            // clear Tags children
            foreach (var tag in _solution.Tags)
            {
                foreach (var pin in tag.Children.Cast<Pin>())
                {
                    pin.SimulationParent = null;
                }

                if (tag.Children != null)
                    tag.Children.Clear();
            }
        }

        private void MapSignalToTag(Signal signal, Tag tag)
        {
            if (signal == null || signal.Children == null || tag == null || tag.Children == null)
                return;

            foreach (var pin in signal.Children.Cast<Pin>())
            {
                tag.Children.Add(pin);

                // set simulation parent to Tag
                pin.SimulationParent = tag;
            }
        }

        private void MapSignalsToTag(List<Signal> signals)
        {
            if (signals == null)
                return;

            // map each Signal children to Tag
            foreach (var signal in signals)
            {
                var tag = signal.Tag;

                MapSignalToTag(signal, tag);
            }
        }

        public void MapTags(List<Context> contexts)
        {
            var signals = contexts.SelectMany(x => x.Children).Where(y => y is Signal).Cast<Signal>().ToList();

            ResetTags();

            MapSignalsToTag(signals);
        }

        private void DebugPrintTagMap()
        {
            // print debug
            foreach (var tag in _solution.Tags)
            {
                Debug.Print("{0}", tag.Properties["Designation"].Data);

                foreach (var pin in tag.Children.Cast<Pin>())
                {
                    Debug.Print("    -> pin type: {0}, parent: {1}, id: {2}", pin.Type, pin.Parent.Name, pin.Id);
                }
            }
        }

        #endregion

        #region Simulation

        private void Initialize()
        {
            _clock = new Clock();

            // create simulation context
            SimulationFactory.CurrentSimulationContext = new SimulationContext()
            {
                Cache = null,
                SimulationClock = _clock
            };

            SimulationFactory.IsConsole = false;

            // disable Console debug output
            SimulationSettings.EnableDebug = false;

            // disable Log for Run()
            SimulationSettings.EnableLog = false;
        }

        public void Start()
        {
            // simulation is already running
            if (SimulationFactory.CurrentSimulationContext.SimulationTimer != null)
                return;

            if (_solution == null)
                return;

            var projects = _solution.Children.Cast<Project>();
            if (projects == null)
                return;

            var contexts = projects.SelectMany(x => x.Children).Cast<Context>().ToList();
            if (contexts == null)
                return;

            // map tags
            MapTags(contexts);

            if (SimulationSettings.EnableDebug)
            {
                DebugPrintTagMap();
            }

            // set elements Id
            var elements = contexts.SelectMany(x => x.Children).Concat(_solution.Tags);
            UInt32 elementId = 0;

            foreach (var element in elements)
            {
                element.ElementId = elementId;
                elementId++;

                // set simulation parent
                if (element.SimulationParent == null)
                {
                    element.SimulationParent = element.Parent;
                }
            }

            // simulation period in ms
            int period = _periodInMillisencods;

            // reset previous simulation cache
            SimulationFactory.Reset(true);

            // run simulation
            SimulationFactory.Run(contexts, _solution.Tags, period, () => { });
            _isSimulationRunning = true;

            // reset simulation parent
            foreach (var element in elements)
            {
                element.SimulationParent = null;
            }
        }

        public void Stop()
        {
            if (SimulationFactory.CurrentSimulationContext.SimulationTimer != null)
            {
                // stop simulation
                SimulationFactory.Stop();

                _isSimulationRunning = false;

                // reset previous simulation cache
                SimulationFactory.Reset(true);

                ResetTags();
            }
        }

        #endregion
    }

    public class TestRenamer
    {
        public void AutoRename(Element element)
        {
            AutoRenameSelector(element);
        }

        public void AutoRenameSelector(Element element)
        {
            if (element is Context)
            {
                AutoRenameElements(element as Context);
            }
            else if (element is Project)
            {
                AutoRenameElements(element as Project);
            }
            else if (element is Solution)
            {
                AutoRenameElements(element as Solution);
            }
            else
            {
                throw new Exception("Not supported Type for rename.");
            }
        }

        private Dictionary<string, int> GetLogicModelCounters()
        {
            // element counters based on type
            var types = System.Reflection.Assembly.GetAssembly(typeof(Solution))
                                                  .GetTypes()
                                                  .Where(x => x.IsClass && x.Namespace == "Simulation.Core")
                                                  .Select(y => y.ToString().Split('.').Last());

            // counters: key = element Type, value = element counter
            var counters = new Dictionary<string, int>();
            foreach (var type in types)
                counters.Add(type, 0);

            return counters;
        }

        private Dictionary<string, string> ShortElementNames = new Dictionary<string, string>()
        {
            // Solution
            { "Solution", "sln" },
		    { "Project", "prj" },
            { "Context", "ctx" },
            // Basic
		    { "Wire", "w" },
		    { "Pin", "p" },
		    { "Signal", "s" },
            // Gates
            { "BufferGate", "bg" },
            { "NotGate", "ng" },
            { "OrGate", "og" },
            { "NorGate", "nog" },
		    { "AndGate", "ag" },
		    { "NandGate", "nag" },
		    { "XorGate", "xog" },
		    { "XnorGate", "xnog" },
            // Timers
		    { "TimerOff", "toff" },
		    { "TimerOn", "ton" },
		    { "TimerPulse", "tp" },
            // Memory
		    { "MemoryResetPriority", "mr" },
		    { "MemorySetPriority", "ms" }
        };

        private void AutoRenameElements(Solution solution)
        {
            // counters: key = element Type, value = element counter
            Dictionary<string, int> counters = GetLogicModelCounters();

            // dict: key = element Id, value = generated name for simulation
            Dictionary<string, string> ids = new Dictionary<string, string>();

            // rename solution
            string solution_name = string.Format("{0}{1}",
                ShortElementNames["Solution"],
                ++counters["Solution"]);

            solution.Name = solution_name;
            ids.Add(solution.Id, solution_name);

            //Debug.Print("Solution: {0} : {1}", solution.Name, solution.Id);

            // get all projects
            var projects = solution.Children.Cast<Project>();

            foreach (var project in projects)
            {
                // rename project
                string project_name = string.Format("{0}{1}",
                    ShortElementNames["Project"],
                    ++counters["Project"]);

                project.Name = project_name;
                ids.Add(project.Id, project_name);

                //Debug.Print("Project: {0} : {1}", project.Name, project.Id);

                // rename contexts
                var contexts = project.Children.Cast<Context>();

                AutoRenameElements(contexts, counters, ids);
            }
        }

        private void AutoRenameElements(Project project)
        {
            // counters: key = element Type, value = element counter
            Dictionary<string, int> counters = GetLogicModelCounters();

            // dict: key = element Id, value = generated name for simulation
            Dictionary<string, string> ids = new Dictionary<string, string>();

            // rename project
            string project_name = string.Format("{0}{1}",
                ShortElementNames["Project"],
                ++counters["Project"]);

            project.Name = project_name;
            ids.Add(project.Id, project_name);

            //Debug.Print("Project: {0} : {1}", project.Name, project.Id);

            // rename contexts
            var contexts = project.Children.Cast<Context>();

            AutoRenameElements(contexts, counters, ids);
        }

        private void AutoRenameElements(Context context)
        {
            // counters: key = element Type, value = element counter
            Dictionary<string, int> counters = GetLogicModelCounters();

            // dict: key = element Id, value = generated name for simulation
            Dictionary<string, string> ids = new Dictionary<string, string>();

            AutoRenameContext(context, counters, ids);
        }

        private void AutoRenameElements(IEnumerable<Context> contexts, Dictionary<string, int> counters, Dictionary<string, string> ids)
        {
            foreach (var context in contexts)
            {
                AutoRenameContext(context, counters, ids);
            }
        }

        public void AutoRenameContext(Context context, Dictionary<string, int> counters, Dictionary<string, string> ids)
        {
            string context_name = string.Format("{0}{1}",
                ShortElementNames["Context"],
                ++counters["Context"]);

            context.Name = context_name;
            ids.Add(context.Id, context_name);

            //Debug.Print("Context: {0} : {1}", context.Name, context.Id);

            foreach (var child in context.Children)
            {
                string type = child.GetType().ToString().Split('.').Last();

                // element is Pin with parent Element uses FactoryName
                if (child is Pin && child.Parent != null && !(child.Parent is Context))
                {
                    string child_name = string.Format("{0}_{1}",
                        child.FactoryName.ToLower(),
                        child.Parent.Name);

                    child.Name = child_name;
                    ids.Add(child.Id, child_name);

                    //Debug.Print("{2}: {0} : {1}", child.Name, child.Id, type);
                }
                // standalone element
                else
                {
                    string child_name = string.Format("{0}{1}",
                        ShortElementNames[type],
                        ++counters[type]);

                    child.Name = child_name;
                    ids.Add(child.Id, child_name);

                    //Debug.Print("{2}: {0} : {1}", child.Name, child.Id, type);
                }
            }
        }
    }

    public class TestDemoSolution
    {
        private TestSimulation _simulation = null;
        private Solution _solution = null;

        public TestDemoSolution(Solution solution, int period)
        {
            _solution = solution;
            new TestRenamer().AutoRename(_solution);
            _simulation = new TestSimulation(_solution, period);
        }

        public static Solution CreateDemoSolution()
        {
            var _factory = new TestFactory();
        
            // create solution
            var solution = new Solution() { Id = Guid.NewGuid().ToString(), Name = "solution", DefaultTag = _factory.CreateSignalTag("tag", "", "", "") };

            var project = new Project() { Id = Guid.NewGuid().ToString(), Name = "project", Parent = solution };
            solution.Children.Add(project);

            var context = new Context() { Id = Guid.NewGuid().ToString(), Name = "context", Parent = project };
            project.Children.Add(context);

            // create tags
            var tag1 = _factory.CreateSignalTag("tag1", "", "", "");
            var tag2 = _factory.CreateSignalTag("tag2", "", "", "");
            var tag3 = _factory.CreateSignalTag("tag3", "", "", "");
            solution.Tags.Add(tag1);
            solution.Tags.Add(tag2);
            solution.Tags.Add(tag3);

            // context children
            var s1 = _factory.CreateSignal(context, 0, 0);
            var s2 = _factory.CreateSignal(context, 0, 0);
            var s3 = _factory.CreateSignal(context, 0, 0);
            var ag1 = _factory.CreateAndGate(context, 0, 0);
            var p1 = _factory.CreatePin(context, 0, 0, context);

            var o_s1 = s1.Children.Single((e) => e.FactoryName == "O") as Pin;
            var o_s2 = s2.Children.Single((e) => e.FactoryName == "O") as Pin;
            var t_ag1 = ag1.Children.Single((e) => e.FactoryName == "T") as Pin;
            var l_ag1 = ag1.Children.Single((e) => e.FactoryName == "L") as Pin;
            var r_ag1 = ag1.Children.Single((e) => e.FactoryName == "R") as Pin;
            var i_s3 = s1.Children.Single((e) => e.FactoryName == "I") as Pin;

            var w1 = _factory.CreateWire(context, o_s1, p1);
            var w2 = _factory.CreateWire(context, p1, t_ag1);
            var w3 = _factory.CreateWire(context, o_s2, l_ag1);
            var w4 = _factory.CreateWire(context, r_ag1, i_s3);

            // associate tags
            s1.Tag = tag1;
            s2.Tag = tag2;
            s3.Tag = tag3;

            return solution;
        }

        public void EnableSimulationDebug(bool enable)
        {
            SimulationSettings.EnableDebug = enable;
        }

        public void EnableSimulationLog(bool enable)
        {
            SimulationSettings.EnableLog = enable;
        }

        public void StartSimulation()
        {
            _simulation.Start();
        }

        public void StopSimulation()
        {
            _simulation.Stop();
        }
    }

    public class TestSerializer 
    {
        private TestFactory _factory = new TestFactory();
        private ObservableCollection<Tag> tags = null;
        private Dictionary<int, Pin> map = null;

        private bool Compare(string strA, string strB)
        {
            return string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase) == 0;
        }

        private int SetId(IBlock parent, int nextId)
        {
            if (parent.Points != null)
            {
                foreach (var point in parent.Points)
                {
                    point.Id = nextId++;
                }
            }

            if (parent.Lines != null)
            {
                foreach (var line in parent.Lines)
                {
                    line.Id = nextId++;
                }
            }

            if (parent.Blocks != null)
            {
                foreach (var block in parent.Blocks)
                {
                    block.Id = nextId++;
                    nextId = SetId(block, nextId);
                } 
            }

            return nextId;
        }

        private Tag CreateSignalTag(string designation, string description, string signal, string condition)
        {
            var tag = new Tag() { Id = Guid.NewGuid().ToString() };
            tag.Properties.Add("Designation", new Property(designation));
            tag.Properties.Add("Description", new Property(description));
            tag.Properties.Add("Signal", new Property(signal));
            tag.Properties.Add("Condition", new Property(condition));
            return tag;
        }

        private Wire CreateWire(Context context, Pin start, Pin end)
        {
            var element = new Wire()
            {
                Start = start,
                End = end,
                X = 0,
                Y = 0,
                Id = Guid.NewGuid().ToString(),
                Parent = context
            };

            context.Children.Add(element);

            return element;
        }

        private Pin CreatePin(Context context, double x, double y, Element parent, string name = "", string factoryName = "", PinType type = PinType.Undefined, bool pinTypeUndefined = true)
        {
            var element = new Pin()
            {
                Name = name,
                FactoryName = factoryName,
                X = x,
                Y = y,
                Id = Guid.NewGuid().ToString(),
                Parent = parent,
                Type = type,
                IsPinTypeUndefined = pinTypeUndefined
            };

            if (parent != null && !(parent is Context))
            {
                parent.Children.Add(element);
            }

            context.Children.Add(element);

            return element;
        }

        private Signal CreateSignal(Context context, IBlock block)
        {
            var tag = _factory.CreateSignalTag("tag" + block.Id.ToString(), "", "", "");
            tags.Add(tag);

            var element = new Signal()
            {
                Name = block.Name.ToLower() + block.Id.ToString(),
                Id = Guid.NewGuid().ToString(),
                X = block.X,
                Y = block.Y,
                Tag = tag,
                Parent = context
            };

            context.Children.Add(element);

            var input = block.Points[0];
            var output = block.Points[1];
            
            var ipin = CreatePin(context, input.X, input.Y, element, "I", "I", PinType.Input, false);
            var opin = CreatePin(context, output.X, output.Y, element, "O", "O", PinType.Output, false);

            map.Add(input.Id, ipin);
            map.Add(output.Id, opin);
  
            return element;
        }

        private AndGate CreateAndGate(Context context, IBlock block)
        {
            var element = new AndGate()
            {
                Name = block.Name.ToLower() + block.Id.ToString(),
                Id = Guid.NewGuid().ToString(),
                X = block.X,
                Y = block.Y,
                Parent = context
            };

            context.Children.Add(element);

            var left = block.Points[0];
            var right = block.Points[1];
            var top = block.Points[2];
            var bottom = block.Points[3];

            var lpin = CreatePin(context, left.X, left.Y, element, "L", "L");
            var rpin = CreatePin(context, right.X, right.Y, element, "R", "R");
            var tpin = CreatePin(context, top.X, top.Y, element, "T", "T");
            var bpin = CreatePin(context, bottom.X, bottom.Y, element, "B", "B");

            map.Add(left.Id, lpin);
            map.Add(right.Id, rpin);
            map.Add(top.Id, tpin);
            map.Add(bottom.Id, bpin);
            
            return element;
        }

        private OrGate CreateOrGate(Context context, IBlock block)
        {
            var element = new OrGate()
            {
                Name = block.Name.ToLower() + block.Id.ToString(),
                Id = Guid.NewGuid().ToString(),
                X = block.X,
                Y = block.Y,
                Parent = context
            };

            context.Children.Add(element);

            var left = block.Points[0];
            var right = block.Points[1];
            var top = block.Points[2];
            var bottom = block.Points[3];

            var lpin = CreatePin(context, left.X, left.Y, element, "L", "L");
            var rpin = CreatePin(context, right.X, right.Y, element, "R", "R");
            var tpin = CreatePin(context, top.X, top.Y, element, "T", "T");
            var bpin = CreatePin(context, bottom.X, bottom.Y, element, "B", "B");

            map.Add(left.Id, lpin);
            map.Add(right.Id, rpin);
            map.Add(top.Id, tpin);
            map.Add(bottom.Id, bpin);
            
            return element;
        }

        private TimerOn CreateTimerOn(Context context, IBlock block)
        {
            var element = new TimerOn()
            {
                Name = block.Name.ToLower() + block.Id.ToString(),
                Id = Guid.NewGuid().ToString(),
                X = block.X,
                Y = block.Y,
                Parent = context,
                Delay = 1.0f
            };

            context.Children.Add(element);

            var left = block.Points[0];
            var right = block.Points[1];
            var top = block.Points[2];
            var bottom = block.Points[3];

            var lpin = CreatePin(context, left.X, left.Y, element, "L", "L");
            var rpin = CreatePin(context, right.X, right.Y, element, "R", "R");
            var tpin = CreatePin(context, top.X, top.Y, element, "T", "T");
            var bpin = CreatePin(context, bottom.X, bottom.Y, element, "B", "B");

            map.Add(left.Id, lpin);
            map.Add(right.Id, rpin);
            map.Add(top.Id, tpin);
            map.Add(bottom.Id, bpin);
            
            return element;
        }

        private TimerOff CreateTimerOff(Context context, IBlock block)
        {
            var element = new TimerOff()
            {
                Name = block.Name.ToLower() + block.Id.ToString(),
                Id = Guid.NewGuid().ToString(),
                X = block.X,
                Y = block.Y,
                Parent = context,
                Delay = 1.0f
            };

            context.Children.Add(element);

            var left = block.Points[0];
            var right = block.Points[1];
            var top = block.Points[2];
            var bottom = block.Points[3];

            var lpin = CreatePin(context, left.X, left.Y, element, "L", "L");
            var rpin = CreatePin(context, right.X, right.Y, element, "R", "R");
            var tpin = CreatePin(context, top.X, top.Y, element, "T", "T");
            var bpin = CreatePin(context, bottom.X, bottom.Y, element, "B", "B");

            map.Add(left.Id, lpin);
            map.Add(right.Id, rpin);
            map.Add(top.Id, tpin);
            map.Add(bottom.Id, bpin);
            
            return element;
        }

        private TimerPulse CreateTimerPulse(Context context, IBlock block)
        {
            var element = new TimerPulse()
            {
                Name = block.Name.ToLower() + block.Id.ToString(),
                Id = Guid.NewGuid().ToString(),
                X = block.X,
                Y = block.Y,
                Parent = context,
                Delay = 1.0f
            };

            context.Children.Add(element);

            var left = block.Points[0];
            var right = block.Points[1];
            var top = block.Points[2];
            var bottom = block.Points[3];

            var lpin = CreatePin(context, left.X, left.Y, element, "L", "L");
            var rpin = CreatePin(context, right.X, right.Y, element, "R", "R");
            var tpin = CreatePin(context, top.X, top.Y, element, "T", "T");
            var bpin = CreatePin(context, bottom.X, bottom.Y, element, "B", "B");

            map.Add(left.Id, lpin);
            map.Add(right.Id, rpin);
            map.Add(top.Id, tpin);
            map.Add(bottom.Id, bpin);
            
            return element;
        }

        private Pin Serialize(Context context, IPoint point)
        { 
            var pin = CreatePin(context, point.X, point.Y, context, "pin" + point.Id.ToString());
            map.Add(point.Id, pin);
            return pin;
        }

        private Wire Serialize(Context context, ILine line)
        {
            Pin start = map[line.Start.Id];
            Pin end = map[line.End.Id];
            var wire = CreateWire(context, start, end);
            return wire;
        }

        private Element Serialize(Context context, IBlock block)
        {
            if (Compare(block.Name, "SIGNAL"))
            {
                return CreateSignal(context, block);
            }
            else if (Compare(block.Name, "AND"))
            {
                return CreateAndGate(context, block);
            }
            else if (Compare(block.Name, "OR"))
            {
                return CreateOrGate(context, block);
            }
            else if (Compare(block.Name, "TIMER-ON"))
            {
                return CreateTimerOn(context, block);
            }
            else if (Compare(block.Name, "TIMER-OFF"))
            {
                return CreateTimerOff(context, block);
            }
            else if (Compare(block.Name, "TIMER-PULSE"))
            {
                return CreateTimerPulse(context, block);
            }
            else
            {
                throw new Exception("Unsupported block Name");
            }
        }

        private void SerializerContents(Context context, IBlock root)
        {
            SetId(root, 1);

            if (root.Blocks != null)
            {
                foreach (var block in root.Blocks)
                {
                    Serialize(context, block);
                }
            }
            
            if (root.Points != null)
            {
                foreach (var point in root.Points)
                {
                    Serialize(context, point);
                }
            }

            if (root.Lines != null)
            {
                foreach (var line in root.Lines)
                {
                    Serialize(context, line);
                }
            }
        }
        
        public Solution Serialize(IBlock root)
        {
            var solution = new Solution() { Id = Guid.NewGuid().ToString(), Name = "solution", DefaultTag = null };

            var project = new Project() { Id = Guid.NewGuid().ToString(), Name = "project", Parent = solution };
            solution.Children.Add(project);

            var context = new Context() { Id = Guid.NewGuid().ToString(), Name = "context", Parent = project };
            project.Children.Add(context);

            map = new Dictionary<int, Pin>();
            tags = new ObservableCollection<Tag>();

            SerializerContents(context, root);

            solution.Tags = tags;

            return solution;
        }
    }

    #endregion
}

namespace Simulation.Binary
{
    using System.IO;
    using System.IO.Compression;
    using System.Runtime.InteropServices;

    #region BinaryFileFormat

    // command Id: sizeof(UInt16)
    // command data: sizeof(command)
    // ...
    // elements counter: sizeof(UInt32)
    //
    // where:
    // Id   Command
    // ---------------
    // 0    Solution
    // 1    Project
    // 2    Context
    // 3    Pin
    // 4    Signal
    // 5    AndGate
    // 6    OrGate
    // 7    TimerOn
    // 8    TimerOff
    // 9    TimerPulse
    // 10   Connect

    // Solution command:
    // UInt32 Id;

    // Project command:
    // UInt32 Id;

    // Context command:
    // UInt32 Id;

    // Pin command:
    // UInt32 Id;

    // Signal command:
    // UInt32 Id;
    // UInt32 InputPinId;
    // UInt32 OutputPinId;

    // AndGate command:
    // UInt32 Id;
    // UInt32 LeftPinId;
    // UInt32 RightPinId;
    // UInt32 TopPinId;
    // UInt32 BottomPinId;

    // OrGate command:
    // UInt32 Id;
    // UInt32 LeftPinId;
    // UInt32 RightPinId;
    // UInt32 TopPinId;
    // UInt32 BottomPinId;

    // TimerOn command:
    // UInt32 Id;
    // UInt32 LeftPinId;
    // UInt32 RightPinId;
    // UInt32 TopPinId;
    // UInt32 BottomPinId;
    // Single Delay;

    // TimerOff command:
    // UInt32 Id;
    // UInt32 LeftPinId;
    // UInt32 RightPinId;
    // UInt32 TopPinId;
    // UInt32 BottomPinId;
    // Single Delay;

    // TimerPulse command:
    // UInt32 Id;
    // UInt32 LeftPinId;
    // UInt32 RightPinId;
    // UInt32 TopPinId;
    // UInt32 BottomPinId;
    // Single Delay;

    // Connect command:
    // UInt32 Id;
    // UInt32 SrcPinId;
    // UInt32 DstPinId;
    // Byte InvertStart;
    // Byte InvertEnd;

    #endregion

    #region BinaryFileReader

    public class BinaryFileReader
    {
        #region Fields

        private static int sizeOfCommandId = Marshal.SizeOf(typeof(UInt16));
        private static int sizeOfTotalElements = Marshal.SizeOf(typeof(UInt32));
        private static Element[] Elements;
        public static Solution CurrentSolution;
        public static Project CurrentProject;
        public static Context CurrentContext;

        #endregion

        #region Compression

        public void CompressFile(string sourcePath, string destinationPath)
        {
            using (var inputStream = File.OpenRead(sourcePath))
            {
                using (var outputStream = File.Create(destinationPath))
                {
                    using (var compressedStream = new GZipStream(outputStream, CompressionMode.Compress))
                    {
                        inputStream.CopyTo(compressedStream);
                    }
                }
            }
        }

        public void DecompressFile(string sourcePath, string destinationPath)
        {
            using (var inputStream = File.OpenRead(sourcePath))
            {
                using (var outputStream = File.Create(destinationPath))
                {
                    using (var deCompressedStream = new GZipStream(outputStream, CompressionMode.Decompress))
                    {
                        inputStream.CopyTo(deCompressedStream);
                    }
                }
            }
        }

        #endregion

        #region Open

        public void OpenCompressed(string path)
        {
            using (var fileStream = File.OpenRead(path))
            {
                using (var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        gzipStream.CopyTo(memoryStream);
                        ReadData(memoryStream);
                    }
                }
            }
        }

        public void OpenUncompressed(string path)
        {
            using (var fileStream = File.OpenRead(path))
            {
                using (var memoryStream = new MemoryStream())
                {
                    fileStream.CopyTo(memoryStream);
                    ReadData(memoryStream);
                }
            }
        }

        private void ReadData(MemoryStream memoryStream)
        {
            using (var reader = new BinaryReader(memoryStream))
            {
                UInt32 totalElements = 0;
                UInt16 commandId = 0;

                long size = memoryStream.Length;
                long dataSize = size - sizeOfTotalElements;

                // get element counter
                memoryStream.Seek(size - sizeOfTotalElements, SeekOrigin.Begin);

                totalElements = reader.ReadUInt32();

                memoryStream.Seek(0, SeekOrigin.Begin);

                // allocate elements array
                Elements = new Element[totalElements];

                while (memoryStream.Position < dataSize)
                {
                    commandId = reader.ReadUInt16();

                    switch (commandId)
                    {
                        // Solution
                        case 0:
                            {
                                UInt32 Id = reader.ReadUInt32();

                                AddSolution(ref Id);
                            }
                            break;
                        // Project
                        case 1:
                            {
                                UInt32 Id = reader.ReadUInt32();

                                AddProject(ref Id);
                            }
                            break;
                        // Context
                        case 2:
                            {
                                UInt32 Id = reader.ReadUInt32();

                                AddContext(ref Id);
                            }
                            break;
                        // Pin
                        case 3:
                            {
                                UInt32 Id = reader.ReadUInt32();

                                AddPin(ref Id);
                            }
                            break;
                        // Signal
                        case 4:
                            {
                                UInt32 Id = reader.ReadUInt32();
                                UInt32 InputPinId = reader.ReadUInt32();
                                UInt32 OutputPinId = reader.ReadUInt32();

                                AddSignal(ref Id, ref InputPinId, ref OutputPinId);
                            }
                            break;
                        // AndGate
                        case 5:
                            {
                                UInt32 Id = reader.ReadUInt32();
                                UInt32 LeftPinId = reader.ReadUInt32();
                                UInt32 RightPinId = reader.ReadUInt32();
                                UInt32 TopPinId = reader.ReadUInt32();
                                UInt32 BottomPinId = reader.ReadUInt32();

                                AddAndGate(ref Id, ref LeftPinId, ref RightPinId, ref TopPinId, ref BottomPinId);
                            }
                            break;
                        // OrGate
                        case 6:
                            {
                                UInt32 Id = reader.ReadUInt32();
                                UInt32 LeftPinId = reader.ReadUInt32();
                                UInt32 RightPinId = reader.ReadUInt32();
                                UInt32 TopPinId = reader.ReadUInt32();
                                UInt32 BottomPinId = reader.ReadUInt32();

                                AddOrGate(ref Id, ref LeftPinId, ref RightPinId, ref TopPinId, ref BottomPinId);
                            }
                            break;
                        // TimerOn
                        case 7:
                            {
                                UInt32 Id = reader.ReadUInt32();
                                UInt32 LeftPinId = reader.ReadUInt32();
                                UInt32 RightPinId = reader.ReadUInt32();
                                UInt32 TopPinId = reader.ReadUInt32();
                                UInt32 BottomPinId = reader.ReadUInt32();
                                Single Delay = reader.ReadSingle();

                                AddTimerOn(ref Id, ref LeftPinId, ref RightPinId, ref TopPinId, ref BottomPinId, ref Delay);
                            }
                            break;
                        // TimerOff
                        case 8:
                            {
                                UInt32 Id = reader.ReadUInt32();
                                UInt32 LeftPinId = reader.ReadUInt32();
                                UInt32 RightPinId = reader.ReadUInt32();
                                UInt32 TopPinId = reader.ReadUInt32();
                                UInt32 BottomPinId = reader.ReadUInt32();
                                Single Delay = reader.ReadSingle();

                                AddTimerOff(ref Id, ref LeftPinId, ref RightPinId, ref TopPinId, ref BottomPinId, ref Delay);
                            }
                            break;
                        // TimerPulse
                        case 9:
                            {
                                UInt32 Id = reader.ReadUInt32();
                                UInt32 LeftPinId = reader.ReadUInt32();
                                UInt32 RightPinId = reader.ReadUInt32();
                                UInt32 TopPinId = reader.ReadUInt32();
                                UInt32 BottomPinId = reader.ReadUInt32();
                                Single Delay = reader.ReadSingle();

                                AddTimerPulse(ref Id, ref LeftPinId, ref RightPinId, ref TopPinId, ref BottomPinId, ref Delay);
                            }
                            break;
                        // Connect
                        case 10:
                            {
                                UInt32 Id = reader.ReadUInt32();
                                UInt32 SrcPinId = reader.ReadUInt32();
                                UInt32 DstPinId = reader.ReadUInt32();
                                Byte InvertStart = reader.ReadByte();
                                Byte InvertEnd = reader.ReadByte();

                                AddWire(ref Id, ref SrcPinId, ref DstPinId, ref InvertStart, ref InvertEnd);
                            }
                            break;
                    }
                }

                // reset elements cache array
                for (UInt32 i = 0; i < totalElements; i++)
                {
                    Elements[i] = null;
                }

                Elements = null;
            }
        }

        #endregion

        #region Add

        private void AddWire(ref UInt32 Id, ref UInt32 SrcPinId, ref UInt32 DstPinId, ref Byte InvertStart, ref Byte InvertEnd)
        {
            var context = CurrentContext;
            var children = context.Children;

            var p_src = Elements[SrcPinId];
            var p_dst = Elements[DstPinId];

            if (p_src != null && p_dst != null && p_src is Pin && p_dst is Pin)
            {
                var wire = new Wire(); //WIRE

                wire.ElementId = Id;

                wire.Start = p_src as Pin;
                wire.End = p_dst as Pin;

                wire.InvertStart = InvertStart == 0x01;
                wire.InvertEnd = InvertEnd == 0x01;

                wire.Parent = context;

                children.Add(wire);

                Elements[Id] = wire;
            }
        }

        private void AddTimerPulse(ref UInt32 Id, ref UInt32 LeftPinId, ref UInt32 RightPinId, ref UInt32 TopPinId, ref UInt32 BottomPinId, ref Single Delay)
        {
            var context = CurrentContext;
            var children = context.Children;

            var tp = new TimerPulse() { Delay = Delay }; //TP

            var p_top = new Pin() { Type = PinType.Undefined, IsPinTypeUndefined = true }; //TOP
            var p_bottom = new Pin() { Type = PinType.Undefined, IsPinTypeUndefined = true }; //BOTTOM
            var p_left = new Pin() { Type = PinType.Undefined, IsPinTypeUndefined = true }; //LEFT
            var p_right = new Pin() { Type = PinType.Undefined, IsPinTypeUndefined = true }; //RIGHT

            tp.ElementId = Id;
            p_top.ElementId = TopPinId;
            p_bottom.ElementId = BottomPinId;
            p_left.ElementId = LeftPinId;
            p_right.ElementId = RightPinId;

            tp.Children.Add(p_top);
            tp.Children.Add(p_bottom);
            tp.Children.Add(p_left);
            tp.Children.Add(p_right);

            tp.Parent = context;

            p_top.Parent = tp;
            p_bottom.Parent = tp;
            p_left.Parent = tp;
            p_right.Parent = tp;

            children.Add(tp);
            children.Add(p_top);
            children.Add(p_bottom);
            children.Add(p_left);
            children.Add(p_right);

            Elements[Id] = tp;
            Elements[TopPinId] = p_top;
            Elements[BottomPinId] = p_bottom;
            Elements[LeftPinId] = p_left;
            Elements[RightPinId] = p_right;
        }

        private void AddTimerOff(ref UInt32 Id, ref UInt32 LeftPinId, ref UInt32 RightPinId, ref UInt32 TopPinId, ref UInt32 BottomPinId, ref Single Delay)
        {
            var context = CurrentContext;
            var children = context.Children;

            var toff = new TimerOff() { Delay = Delay }; //TOFF

            var p_top = new Pin() { Type = PinType.Undefined, IsPinTypeUndefined = true }; //TOP
            var p_bottom = new Pin() { Type = PinType.Undefined, IsPinTypeUndefined = true }; //BOTTOM
            var p_left = new Pin() { Type = PinType.Undefined, IsPinTypeUndefined = true }; //LEFT
            var p_right = new Pin() { Type = PinType.Undefined, IsPinTypeUndefined = true }; //RIGHT

            toff.ElementId = Id;
            p_top.ElementId = TopPinId;
            p_bottom.ElementId = BottomPinId;
            p_left.ElementId = LeftPinId;
            p_right.ElementId = RightPinId;

            toff.Children.Add(p_top);
            toff.Children.Add(p_bottom);
            toff.Children.Add(p_left);
            toff.Children.Add(p_right);

            toff.Parent = context;

            p_top.Parent = toff;
            p_bottom.Parent = toff;
            p_left.Parent = toff;
            p_right.Parent = toff;

            children.Add(toff);
            children.Add(p_top);
            children.Add(p_bottom);
            children.Add(p_left);
            children.Add(p_right);

            Elements[Id] = toff;
            Elements[TopPinId] = p_top;
            Elements[BottomPinId] = p_bottom;
            Elements[LeftPinId] = p_left;
            Elements[RightPinId] = p_right;
        }

        private void AddTimerOn(ref UInt32 Id, ref UInt32 LeftPinId, ref UInt32 RightPinId, ref UInt32 TopPinId, ref UInt32 BottomPinId, ref Single Delay)
        {
            var context = CurrentContext;
            var children = context.Children;

            var ton = new TimerOn() { Delay = Delay }; //TON

            var p_top = new Pin() { Type = PinType.Undefined, IsPinTypeUndefined = true }; //TOP
            var p_bottom = new Pin() { Type = PinType.Undefined, IsPinTypeUndefined = true }; //BOTTOM
            var p_left = new Pin() { Type = PinType.Undefined, IsPinTypeUndefined = true }; //LEFT
            var p_right = new Pin() { Type = PinType.Undefined, IsPinTypeUndefined = true }; //RIGHT

            ton.ElementId = Id;
            p_top.ElementId = TopPinId;
            p_bottom.ElementId = BottomPinId;
            p_left.ElementId = LeftPinId;
            p_right.ElementId = RightPinId;

            ton.Children.Add(p_top);
            ton.Children.Add(p_bottom);
            ton.Children.Add(p_left);
            ton.Children.Add(p_right);

            ton.Parent = context;

            p_top.Parent = ton;
            p_bottom.Parent = ton;
            p_left.Parent = ton;
            p_right.Parent = ton;

            children.Add(ton);
            children.Add(p_top);
            children.Add(p_bottom);
            children.Add(p_left);
            children.Add(p_right);

            Elements[Id] = ton;
            Elements[TopPinId] = p_top;
            Elements[BottomPinId] = p_bottom;
            Elements[LeftPinId] = p_left;
            Elements[RightPinId] = p_right;
        }

        private void AddOrGate(ref UInt32 Id, ref UInt32 LeftPinId, ref UInt32 RightPinId, ref UInt32 TopPinId, ref UInt32 BottomPinId)
        {
            var context = CurrentContext;
            var children = context.Children;

            var og = new OrGate(); //ORGATE

            var p_top = new Pin() { Type = PinType.Undefined, IsPinTypeUndefined = true }; //TOP
            var p_bottom = new Pin() { Type = PinType.Undefined, IsPinTypeUndefined = true }; //BOTTOM
            var p_left = new Pin() { Type = PinType.Undefined, IsPinTypeUndefined = true }; //LEFT
            var p_right = new Pin() { Type = PinType.Undefined, IsPinTypeUndefined = true }; //RIGHT

            og.ElementId = Id;
            p_top.ElementId = TopPinId;
            p_bottom.ElementId = BottomPinId;
            p_left.ElementId = LeftPinId;
            p_right.ElementId = RightPinId;

            og.Children.Add(p_top);
            og.Children.Add(p_bottom);
            og.Children.Add(p_left);
            og.Children.Add(p_right);

            og.Parent = context;

            p_top.Parent = og;
            p_bottom.Parent = og;
            p_left.Parent = og;
            p_right.Parent = og;

            children.Add(og);
            children.Add(p_top);
            children.Add(p_bottom);
            children.Add(p_left);
            children.Add(p_right);

            Elements[Id] = og;
            Elements[TopPinId] = p_top;
            Elements[BottomPinId] = p_bottom;
            Elements[LeftPinId] = p_left;
            Elements[RightPinId] = p_right;
        }

        private void AddAndGate(ref UInt32 Id, ref UInt32 LeftPinId, ref UInt32 RightPinId, ref UInt32 TopPinId, ref UInt32 BottomPinId)
        {
            var context = CurrentContext;
            var children = context.Children;

            var ag = new AndGate(); //ANDGATE

            var p_top = new Pin() { Type = PinType.Undefined, IsPinTypeUndefined = true }; //TOP
            var p_bottom = new Pin() { Type = PinType.Undefined, IsPinTypeUndefined = true }; //BOTTOM
            var p_left = new Pin() { Type = PinType.Undefined, IsPinTypeUndefined = true }; //LEFT
            var p_right = new Pin() { Type = PinType.Undefined, IsPinTypeUndefined = true }; //RIGHT

            ag.ElementId = Id;
            p_top.ElementId = TopPinId;
            p_bottom.ElementId = BottomPinId;
            p_left.ElementId = LeftPinId;
            p_right.ElementId = RightPinId;

            ag.Children.Add(p_top);
            ag.Children.Add(p_bottom);
            ag.Children.Add(p_left);
            ag.Children.Add(p_right);

            ag.Parent = context;

            p_top.Parent = ag;
            p_bottom.Parent = ag;
            p_left.Parent = ag;
            p_right.Parent = ag;

            children.Add(ag);
            children.Add(p_top);
            children.Add(p_bottom);
            children.Add(p_left);
            children.Add(p_right);

            Elements[Id] = ag;
            Elements[TopPinId] = p_top;
            Elements[BottomPinId] = p_bottom;
            Elements[LeftPinId] = p_left;
            Elements[RightPinId] = p_right;
        }

        private void AddSignal(ref UInt32 Id, ref UInt32 InputPinId, ref UInt32 OutputPinId)
        {
            var context = CurrentContext;
            var children = context.Children;

            var signal = new Signal(); //SIGNAL

            var p_input = new Pin() { Type = PinType.Input, IsPinTypeUndefined = false }; //INPUT
            var p_output = new Pin() { Type = PinType.Output, IsPinTypeUndefined = false }; //OUTPUT   

            signal.ElementId = Id;
            p_input.ElementId = InputPinId;
            p_output.ElementId = OutputPinId;

            signal.Children.Add(p_input);
            signal.Children.Add(p_output);

            signal.Children.Add(p_input);
            signal.Children.Add(p_output);

            signal.Parent = context;

            p_input.Parent = signal;
            p_output.Parent = signal;

            children.Add(signal);
            children.Add(p_input);
            children.Add(p_output);

            Elements[Id] = signal;
            Elements[InputPinId] = p_input;
            Elements[OutputPinId] = p_output;
        }

        private void AddPin(ref UInt32 Id)
        {
            var context = CurrentContext;
            var children = context.Children;

            var p = new Pin() { Type = PinType.Undefined, IsPinTypeUndefined = true }; //PIN

            p.ElementId = Id;

            p.Parent = context;

            children.Add(p);

            Elements[Id] = p;
        }

        private void AddContext(ref UInt32 Id)
        {
            var context = new Context();
            Elements[Id] = context;

            CurrentProject.Children.Add(context);
            CurrentContext = context;
            //CurrentSolution.CurrentContext = context;
        }

        private void AddProject(ref UInt32 Id)
        {
            var project = new Project();
            Elements[Id] = project;

            CurrentSolution.Children.Add(project);
            CurrentProject = project;
            //CurrentSolution.CurrentProject = project;
        }

        private void AddSolution(ref UInt32 Id)
        {
            var solution = new Solution();
            Elements[Id] = solution;

            CurrentSolution = solution;
        }

        #endregion
    }

    #endregion

    #region BinaryFileWriter

    public class BinaryFileWriter
    {
        #region Fields

        // dict: key = element Id, value = generated element id for simulation
        private Dictionary<string, UInt32> ids;

        // generated element id
        private UInt32 elementId;

        // command id
        private UInt16 commandId;

        // bool true Byte value
        private const Byte trueByte = 0x01;

        // bool false Byte value
        private const Byte falseByte = 0x00;

        #endregion

        #region Save

        public void Save(Solution solution, string path)
        {
            using (var stream = new FileStream(path, FileMode.Create))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    ids = new Dictionary<string, UInt32>();
                    elementId = 0;

                    // write solution
                    WriteSolution(solution, writer);

                    var projects = solution.Children.Cast<Project>();

                    // write projects
                    foreach (var project in projects)
                    {
                        WriteProjectAndChildren(project, writer);
                    }

                    // total elements counter
                    writer.Write(elementId);
                }
            }
        }

        public void Save(Project project, string path)
        {
            using (var stream = new FileStream(path, FileMode.Create))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    ids = new Dictionary<string, UInt32>();
                    elementId = 0;

                    // write dummy solution
                    WriteDummySolution(writer);

                    // write project
                    WriteProjectAndChildren(project, writer);

                    // total elements counter
                    writer.Write(elementId);
                }
            }
        }

        public void Save(Context context, string path)
        {
            using (var stream = new FileStream(path, FileMode.Create))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    ids = new Dictionary<string, UInt32>();
                    elementId = 0;

                    // write dummy solution
                    WriteDummySolution(writer);

                    // write dummy project
                    WriteDummyProject(writer);

                    // write context
                    WriteContextAndChildren(context, writer);

                    // total elements counter
                    writer.Write(elementId);
                }
            }
        }

        #endregion

        #region Write

        private void WriteProjectAndChildren(Project project, BinaryWriter writer)
        {
            // write project
            WriteProject(project, writer);

            var contexts = project.Children.Cast<Context>();

            // write contexts
            foreach (var context in contexts)
            {
                WriteContextAndChildren(context, writer);
            }
        }

        private void WriteContextAndChildren(Context context, BinaryWriter writer)
        {
            // write context
            WriteContext(context, writer);

            // write child
            foreach (var child in context.Children)
            {
                WriteElement(child, writer);
            }

            // write connections
            WriteConnections(context, writer);
        }

        private void WriteElement(Element element, BinaryWriter writer)
        {
            string type = element.GetType().ToString().Split('.').Last();
            switch (type)
            {
                case "Signal":
                    WriteSignal(element, writer);
                    break;
                case "Pin":
                    WritePin(element, writer);
                    break;
                case "AndGate":
                    WriteAndGate(element, writer);
                    break;
                case "OrGate":
                    WriteOrGate(element, writer);
                    break;
                case "TimerOn":
                    WriteTimerOn(element, writer);
                    break;
                case "TimerOff":
                    WriteTimerOff(element, writer);
                    break;
                case "TimerPulse":
                    WriteTimerPulse(element, writer);
                    break;
            }
        }

        private void WriteConnections(Context context, BinaryWriter writer)
        {
            // write connections
            var wires = context.Children.Where(x => x is Wire).Cast<Wire>();

            foreach (var wire in wires)
            {
                WriteWire(wire, writer);
            }
        }

        private void WriteDummySolution(BinaryWriter writer)
        {
            commandId = 0;
            writer.Write(commandId);
            writer.Write(elementId);
            elementId++;
        }

        private void WriteDummyProject(BinaryWriter writer)
        {
            commandId = 1;
            writer.Write(commandId);
            writer.Write(elementId);
            elementId++;
        }

        private void WriteSolution(Solution solution, BinaryWriter writer)
        {
            commandId = 0;
            writer.Write(commandId);
            writer.Write(elementId);
            ids.Add(solution.Id, elementId);
            elementId++;
        }

        private void WriteProject(Project project, BinaryWriter writer)
        {
            commandId = 1;
            writer.Write(commandId);
            writer.Write(elementId);
            ids.Add(project.Id, elementId);
            elementId++;
        }

        private void WriteContext(Context context, BinaryWriter writer)
        {
            commandId = 2;
            writer.Write(commandId);
            writer.Write(elementId);
            ids.Add(context.Id, elementId);
            elementId++;
        }

        private void WritePin(Element element, BinaryWriter writer)
        {
            if (element.Parent == null || element.Parent is Context)
            {
                // pin id
                commandId = 3;
                writer.Write(commandId);
                writer.Write(elementId);
                ids.Add(element.Id, elementId);
                elementId++;
            }
        }

        private void WriteSignal(Element element, BinaryWriter writer)
        {
            // signal id
            commandId = 4;
            writer.Write(commandId);
            writer.Write(elementId);
            ids.Add(element.Id, elementId);
            elementId++;

            // input pin id
            var i = element.Children.Single(x => x.FactoryName == "I");
            i.ElementId = elementId;
            writer.Write(elementId);
            ids.Add(i.Id, elementId);
            elementId++;

            // output pin id
            var o = element.Children.Single(x => x.FactoryName == "O");
            o.ElementId = elementId;
            writer.Write(elementId);
            ids.Add(o.Id, elementId);
            elementId++;
        }

        private void WriteAndGate(Element element, BinaryWriter writer)
        {
            // andgate id
            commandId = 5;
            writer.Write(commandId);
            writer.Write(elementId);
            ids.Add(element.Id, elementId);
            elementId++;

            // left pin id
            var left = element.Children.Single(x => x.FactoryName == "L");
            left.ElementId = elementId;
            writer.Write(elementId);
            ids.Add(left.Id, elementId);
            elementId++;

            // right pin id
            var right = element.Children.Single(x => x.FactoryName == "R");
            right.ElementId = elementId;
            writer.Write(elementId);
            ids.Add(right.Id, elementId);
            elementId++;

            // top pin id
            var top = element.Children.Single(x => x.FactoryName == "T");
            top.ElementId = elementId;
            writer.Write(elementId);
            ids.Add(top.Id, elementId);
            elementId++;

            // bottom pin id
            var bottom = element.Children.Single(x => x.FactoryName == "B");
            bottom.ElementId = elementId;
            writer.Write(elementId);
            ids.Add(bottom.Id, elementId);
            elementId++;
        }

        private void WriteOrGate(Element element, BinaryWriter writer)
        {
            // orgate id
            commandId = 6;
            writer.Write(commandId);
            writer.Write(elementId);
            ids.Add(element.Id, elementId);
            elementId++;

            // left pin id
            var left = element.Children.Single(x => x.FactoryName == "L");
            left.ElementId = elementId;
            writer.Write(elementId);
            ids.Add(left.Id, elementId);
            elementId++;

            // right pin id
            var right = element.Children.Single(x => x.FactoryName == "R");
            right.ElementId = elementId;
            writer.Write(elementId);
            ids.Add(right.Id, elementId);
            elementId++;

            // top pin id
            var top = element.Children.Single(x => x.FactoryName == "T");
            top.ElementId = elementId;
            writer.Write(elementId);
            ids.Add(top.Id, elementId);
            elementId++;

            // bottom pin id
            var bottom = element.Children.Single(x => x.FactoryName == "B");
            bottom.ElementId = elementId;
            writer.Write(elementId);
            ids.Add(bottom.Id, elementId);
            elementId++;
        }

        private void WriteTimerOn(Element element, BinaryWriter writer)
        {
            // timeron id
            commandId = 7;
            writer.Write(commandId);
            writer.Write(elementId);
            ids.Add(element.Id, elementId);
            elementId++;

            // left pin id
            var left = element.Children.Single(x => x.FactoryName == "L");
            left.ElementId = elementId;
            writer.Write(elementId);
            ids.Add(left.Id, elementId);
            elementId++;

            // right pin id
            var right = element.Children.Single(x => x.FactoryName == "R");
            right.ElementId = elementId;
            writer.Write(elementId);
            ids.Add(right.Id, elementId);
            elementId++;

            // top pin id
            var top = element.Children.Single(x => x.FactoryName == "T");
            top.ElementId = elementId;
            writer.Write(elementId);
            ids.Add(top.Id, elementId);
            elementId++;

            // bottom pin id
            var bottom = element.Children.Single(x => x.FactoryName == "B");
            bottom.ElementId = elementId;
            writer.Write(elementId);
            ids.Add(bottom.Id, elementId);
            elementId++;

            // delay
            writer.Write((element as ITimer).Delay);
        }

        private void WriteTimerOff(Element element, BinaryWriter writer)
        {
            // timeroff id
            commandId = 8;
            writer.Write(commandId);
            writer.Write(elementId);
            ids.Add(element.Id, elementId);
            elementId++;

            // left pin id
            var left = element.Children.Single(x => x.FactoryName == "L");
            left.ElementId = elementId;
            writer.Write(elementId);
            ids.Add(left.Id, elementId);
            elementId++;

            // right pin id
            var right = element.Children.Single(x => x.FactoryName == "R");
            right.ElementId = elementId;
            writer.Write(elementId);
            ids.Add(right.Id, elementId);
            elementId++;

            // top pin id
            var top = element.Children.Single(x => x.FactoryName == "T");
            top.ElementId = elementId;
            writer.Write(elementId);
            ids.Add(top.Id, elementId);
            elementId++;

            // bottom pin id
            var bottom = element.Children.Single(x => x.FactoryName == "B");
            bottom.ElementId = elementId;
            writer.Write(elementId);
            ids.Add(bottom.Id, elementId);
            elementId++;

            // delay
            writer.Write((element as ITimer).Delay);
        }

        private void WriteTimerPulse(Element element, BinaryWriter writer)
        {
            // timerpulse id
            commandId = 9;
            writer.Write(commandId);
            writer.Write(elementId);
            ids.Add(element.Id, elementId);
            elementId++;

            // left pin id
            var left = element.Children.Single(x => x.FactoryName == "L");
            left.ElementId = elementId;
            writer.Write(elementId);
            ids.Add(left.Id, elementId);
            elementId++;

            // right pin id
            var right = element.Children.Single(x => x.FactoryName == "R");
            right.ElementId = elementId;
            writer.Write(elementId);
            ids.Add(right.Id, elementId);
            elementId++;

            // top pin id
            var top = element.Children.Single(x => x.FactoryName == "T");
            top.ElementId = elementId;
            writer.Write(elementId);
            ids.Add(top.Id, elementId);
            elementId++;

            // bottom pin id
            var bottom = element.Children.Single(x => x.FactoryName == "B");
            bottom.ElementId = elementId;
            writer.Write(elementId);
            ids.Add(bottom.Id, elementId);
            elementId++;

            // delay
            writer.Write((element as ITimer).Delay);
        }

        private void WriteWire(Wire wire, BinaryWriter writer)
        {
            UInt32 srcPinId;
            UInt32 dstPinId;
            Byte invertStart;
            Byte invertEnd;

            srcPinId = ids[wire.Start.Id];
            dstPinId = ids[wire.End.Id];

            invertStart = wire.InvertStart ? trueByte : falseByte;
            invertEnd = wire.InvertEnd ? trueByte : falseByte;

            // connect id
            commandId = 10;
            writer.Write(commandId);
            writer.Write(elementId);
            ids.Add(wire.Id, elementId);
            elementId++;

            // source pin id
            writer.Write(srcPinId);
            // destination pin id
            writer.Write(dstPinId);
            // invert flags
            writer.Write(invertStart);
            writer.Write(invertEnd);
        }

        #endregion
    }

    #endregion
}