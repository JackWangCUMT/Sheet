using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulation.Core;
using Simulation.Model;

namespace Simulation.Core
{
    #region Model.Core

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

    #endregion

    #region Model.Enums

    public enum PinType
    {
        Undefined,
        Input,
        Output
    }

    #endregion
}

namespace Simulation.Model
{
    #region Model.Elements.Basic

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

    #endregion

    #region Model.Elements.Gates

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

    #endregion

    #region Model.Elements.Memory

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

    #endregion

    #region Model.Elements.Timers

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

    #endregion

    #region Model.Solution

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

    #endregion
}

namespace Simulation
{
    #region Simulation.Core

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
        #region Properties

        public bool HaveCache { get; set; }
        public ISimulation[] Simulations { get; set; }
        public IBoolState[] States { get; set; }

        #endregion

        #region Reset

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

        #endregion
    }

    public class SimulationContext
    {
        public System.Threading.Timer SimulationTimer { get; set; }
        public object SimulationTimerSync { get; set; }
        public IClock SimulationClock { get; set; }
        public SimulationCache Cache { get; set; }
    }

    #endregion

    #region Simulation.Elements

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

    #endregion

    #region Simulation

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

    public class BoolState : IBoolState
    {
        public bool? PreviousState { get; set; }
        public bool? State { get; set; }
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
                                                  .Where(x => x.IsClass && x.Namespace == "Simulation.Model")
                                                  .Select(y => y.ToString().Split('.').Last());

            // counters: key = element Type, value = element counter
            var counters = new Dictionary<string, int>();
            foreach (var type in types)
                counters.Add(type, 0);

            return counters;
        }

        private Dictionary<string, string> ShortElementNames =
            new Dictionary<string, string>()
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

            //System.Diagnostics.Debug.Print("Solution: {0} : {1}", solution.Name, solution.Id);

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

                //System.Diagnostics.Debug.Print("Project: {0} : {1}", project.Name, project.Id);

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

            //System.Diagnostics.Debug.Print("Project: {0} : {1}", project.Name, project.Id);

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

        private void AutoRenameElements(IEnumerable<Context> contexts,
                                               Dictionary<string, int> counters,
                                               Dictionary<string, string> ids)
        {
            foreach (var context in contexts)
            {
                AutoRenameContext(context, counters, ids);
            }
        }

        public void AutoRenameContext(Context context,
                                             Dictionary<string, int> counters,
                                             Dictionary<string, string> ids)
        {
            string context_name = string.Format("{0}{1}",
                ShortElementNames["Context"],
                ++counters["Context"]);

            context.Name = context_name;
            ids.Add(context.Id, context_name);

            //System.Diagnostics.Debug.Print("Context: {0} : {1}", context.Name, context.Id);

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

                    //System.Diagnostics.Debug.Print("{2}: {0} : {1}", child.Name, child.Id, type);
                }
                // standalone element
                else
                {
                    string child_name = string.Format("{0}{1}",
                        ShortElementNames[type],
                        ++counters[type]);

                    child.Name = child_name;
                    ids.Add(child.Id, child_name);

                    //System.Diagnostics.Debug.Print("{2}: {0} : {1}", child.Name, child.Id, type);
                }
            }
        }
    }

    public class TestDemoSolution
    {
        private TestFactory _factory = new TestFactory();
        private TestSimulation _simulation = null;
        private Solution _solution = null;

        public TestDemoSolution(int period)
        {
            _solution = CreateTestSolution();

            var renamer = new TestRenamer();
            renamer.AutoRename(_solution);

            _simulation = new TestSimulation(_solution, period);
        }

        private Solution CreateTestSolution()
        {
            // create solution
            var solution = new Solution() { Id = Guid.NewGuid().ToString(), Name = "solution", DefaultTag = _factory.CreateSignalTag("tag", "", "", "") };

            var project = new Project() { Id = Guid.NewGuid().ToString(), Name = "project", Parent = solution };
            solution.Children.Add(project);

            var context = new Context() { Id = Guid.NewGuid().ToString(), Parent = project };
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

    #endregion
}