using Sheet.Block;
using Sheet.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Threading;

namespace Sheet.Simulation
{
    internal static class SimulationSettings
    {
        public static bool EnableDebug { get; set; }
        public static bool EnableLog { get; set; }
    }

    public abstract class NotifyObject : INotifyPropertyChanged
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
        public long Cycle { get; set; }
        public int Resolution { get; set; }
    }

    public class BoolState : NotifyObject, IBoolState
    {
        public bool? previousState;
        public bool? state;

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

    public enum PinType
    {
        Undefined,
        Input,
        Output
    }

    public class Pin : Element
    {
        public Pin() 
            : base() 
        { 
        }
        public bool IsPinTypeUndefined { get; set; }
        public PinType Type { get; set; }
        public Tuple<Pin, bool>[] Connections { get; set; } // bool is flag for Inverted
    }

    public class Wire : Element
    {
        public Wire()
            : base()
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

    public class Signal : Element
    {
        public Signal() 
            : base() 
        {
        }
        public Tag Tag { get; set; }
    }

    public class TagProperty
    {
        public TagProperty() 
            : base() 
        { 
        }
        public TagProperty(object data) 
            : this() 
        { 
            Data = data; 
        }
        public object Data { get; set; }
    }

    public class Tag : Element, IStateSimulation
    {
        public Tag() 
            : base() 
        { 
            Properties = new Dictionary<string, TagProperty>(); 
        }
        public IDictionary<string, TagProperty> Properties { get; set; }
        public ISimulation Simulation { get; set; }
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
            Pin input = tag.Children
                .Cast<Pin>()
                .Where(x => 
                {
                    return x.Type == PinType.Input
                        && x.Connections != null 
                        && x.Connections.Length > 0;
                })
                .FirstOrDefault();

            //IEnumerable<Pin> outputs = tag
            //    .Children
            //    .Cast<Pin>()
            //    .Where(x => x.Type == PinType.Output);

            if (input == null 
                || input.Connections == null 
                || input.Connections.Length <= 0)
            {
                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print(
                        "No Valid Input/Connections for Id: {0} | State: {1}", 
                        Element.ElementId, 
                        State.State);
                }

                return;
            }

            // get all connected inputs with possible state
            var connections = input
                .Connections
                .Where(x => x.Item1.Type == PinType.Output);

            // set ISimulation dependencies (used for topological sort)
            DependsOn = connections
                .Where(x => x.Item1 != null)
                .Select(y => y.Item1.SimulationParent)
                .Take(1)
                .ToArray();

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
            var states = connections.Select(x => 
            {
                return new Tuple<IBoolState, bool>(
                    (x.Item1.SimulationParent as IStateSimulation).Simulation.State,
                    x.Item2);
            }).ToArray();

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
                    Debug.Print(
                        "Id: {0} | State: {1}", 
                        Element.ElementId, 
                        State.State);
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
            var connections = Element
                .Children
                .Cast<Pin>()
                .Where(pin => pin.Connections != null && pin.Type == PinType.Input)
                .SelectMany(pin => pin.Connections)
                .Where(x => x.Item1.Type == PinType.Output);

            // set ISimulation dependencies (used for topological sort)
            DependsOn = connections
                .Select(y => y.Item1.SimulationParent)
                .ToArray();

            if (SimulationSettings.EnableDebug)
            {
                foreach (var connection in connections)
                {
                    Debug.Print(
                        "Pin: {0} | Inverted: {1} | SimulationParent: {2} | Type: {3}",
                        connection.Item1.ElementId,
                        connection.Item2,
                        (connection.Item1.SimulationParent != null) ? connection.Item1.SimulationParent.ElementId : UInt32.MaxValue,
                        connection.Item1.Type);
                }
            }

            // get all connected inputs with state, where Tuple<IBoolState,bool> is IBoolState and Inverted
            var states = connections
                .Select(x => 
                {
                    return new Tuple<IBoolState, bool>(
                        (x.Item1.SimulationParent as IStateSimulation).Simulation.State, 
                        x.Item2);
                })
                .ToArray();

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
                    Debug.Print(
                        "Id: {0} | State: {1}", 
                        Element.ElementId, 
                        State.State);
                    Debug.Print("");
                }
            }
        }

        private bool? CalculateState(Tuple<IBoolState, bool>[] states)
        {
            int length = states.Length;
            if (length == 1)
                return null;

            bool? result = null;
            for (int i = 0; i < length; i++)
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
            var connections = Element
                .Children
                .Cast<Pin>()
                .Where(pin => pin.Connections != null && pin.Type == PinType.Input)
                .SelectMany(pin => pin.Connections)
                .Where(x => x.Item1.Type == PinType.Output);

            // set ISimulation dependencies (used for topological sort)
            DependsOn = connections
                .Select(y => y.Item1.SimulationParent)
                .ToArray();

            if (SimulationSettings.EnableDebug)
            {
                foreach (var connection in connections)
                {
                    Debug.Print(
                        "Pin: {0} | Inverted: {1} | SimulationParent: {2} | Type: {3}",
                        connection.Item1.ElementId,
                        connection.Item2,
                        (connection.Item1.SimulationParent != null) ? connection.Item1.SimulationParent.ElementId : UInt32.MaxValue,
                        connection.Item1.Type);
                }
            }

            // get all connected inputs with state, where Tuple<IBoolState,bool> is IBoolState and Inverted
            var states = connections
                .Select(x => 
                {
                    return new Tuple<IBoolState, bool>(
                        (x.Item1.SimulationParent as IStateSimulation).Simulation.State, 
                        x.Item2);
                })
                .ToArray();

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
                    Debug.Print(
                        "Id: {0} | State: {1}", 
                        Element.ElementId, 
                        State.State);
                    Debug.Print("");
                }
            }
        }

        private bool? CalculateState(Tuple<IBoolState, bool>[] states)
        {
            int length = states.Length;
            if (length == 1)
                return null;

            bool? result = null;
            for (int i = 0; i < length; i++)
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
            var inputs = Element
                .Children
                .Cast<Pin>()
                .Where(pin => pin.Connections != null && pin.Type == PinType.Input);

            if (inputs == null || inputs.Count() != 1)
            {
                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print(
                        "No Valid Input for Id: {0} | State: {1}", 
                        Element.ElementId, 
                        State.State);
                }

                return;
            }

            // get all connected inputs with possible state
            var connections = inputs
                .First()
                .Connections
                .Where(x => x.Item1.Type == PinType.Output);

            // set ISimulation dependencies (used for topological sort)
            DependsOn = connections
                .Where(x => x.Item1 != null)
                .Select(y => y.Item1.SimulationParent)
                .Take(1)
                .ToArray();

            if (SimulationSettings.EnableDebug)
            {
                foreach (var connection in connections)
                {
                    Debug.Print(
                        "Pin: {0} | Inverted: {1} | SimulationParent: {2} | Type: {3}",
                        connection.Item1.ElementId,
                        connection.Item2,
                        (connection.Item1.SimulationParent != null) ? connection.Item1.SimulationParent.ElementId : UInt32.MaxValue,
                        connection.Item1.Type);
                }
            }

            // get all connected inputs with state
            var states = connections
                .Select(x => 
                {
                    return new Tuple<IBoolState, bool>(
                        (x.Item1.SimulationParent as IStateSimulation).Simulation.State, 
                        x.Item2);
                })
                .ToArray();

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
                    Debug.Print(
                        "Id: {0} | State: {1}", 
                        Element.ElementId, 
                        State.State);
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
            var inputs = Element
                .Children
                .Cast<Pin>()
                .Where(pin => pin.Connections != null && pin.Type == PinType.Input);

            if (inputs == null || inputs.Count() != 1)
            {
                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print(
                        "No Valid Input for Id: {0} | State: {1}", 
                        Element.ElementId, 
                        State.State);
                }

                return;
            }

            // get all connected inputs with possible state
            var connections = inputs
                .First()
                .Connections
                .Where(x => x.Item1.Type == PinType.Output);

            // set ISimulation dependencies (used for topological sort)
            DependsOn = connections
                .Where(x => x.Item1 != null).
                Select(y => y.Item1.SimulationParent)
                .Take(1)
                .ToArray();

            if (SimulationSettings.EnableDebug)
            {
                foreach (var connection in connections)
                {
                    Debug.Print(
                        "Pin: {0} | Inverted: {1} | SimulationParent: {2} | Type: {3}",
                        connection.Item1.ElementId,
                        connection.Item2,
                        (connection.Item1.SimulationParent != null) ? connection.Item1.SimulationParent.ElementId : UInt32.MaxValue,
                        connection.Item1.Type);
                }
            }

            // get all connected inputs with state
            var states = connections
                .Select(x => 
                {
                    return new Tuple<IBoolState, bool>(
                        (x.Item1.SimulationParent as IStateSimulation).Simulation.State, 
                        x.Item2);
                })
                .ToArray();

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
                    Debug.Print(
                        "Id: {0} | State: {1}", 
                        Element.ElementId, 
                        State.State);
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
            var inputs = Element
                .Children
                .Cast<Pin>()
                .Where(pin => pin.Connections != null && pin.Type == PinType.Input);

            if (inputs == null || inputs.Count() != 1)
            {
                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print(
                        "No Valid Input for Id: {0} | State: {1}", 
                        Element.ElementId, 
                        State.State);
                }

                return;
            }

            // get all connected inputs with possible state
            var connections = inputs
                .First()
                .Connections
                .Where(x => x.Item1.Type == PinType.Output);

            // set ISimulation dependencies (used for topological sort)
            DependsOn = connections
                .Where(x => x.Item1 != null)
                .Select(y => y.Item1.SimulationParent)
                .Take(1)
                .ToArray();

            if (SimulationSettings.EnableDebug)
            {
                foreach (var connection in connections)
                {
                    Debug.Print(
                        "Pin: {0} | Inverted: {1} | SimulationParent: {2} | Type: {3}",
                        connection.Item1.ElementId,
                        connection.Item2,
                        (connection.Item1.SimulationParent != null) ? connection.Item1.SimulationParent.ElementId : UInt32.MaxValue,
                        connection.Item1.Type);
                }
            }

            // get all connected inputs with state
            var states = connections
                .Select(x => 
                {
                    return new Tuple<IBoolState, bool>(
                        (x.Item1.SimulationParent as IStateSimulation).Simulation.State, 
                        x.Item2);
                })
                .ToArray();

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
        public Solution() : base() { Tags = new ObservableCollection<Tag>(); }
        public ObservableCollection<Tag> Tags { get; set; }
    }

    public class SimulationContext
    {
        public Timer SimulationTimer { get; set; }
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
                var length = cache.Simulations.Length;

                for (int i = 0; i < length; i++)
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
        public IEnumerable<T> Sort(
            IEnumerable<T> source, 
            Func<T, IEnumerable<T>> dependencies, 
            bool ignoreDependencyCycles)
        {
            var sorted = new List<T>();
            var visited = new HashSet<T>();

            foreach (var item in source)
            {
                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print(
                        "+ {0} depends on:", 
                        (item as ISimulation).Element.Name);
                }

                Visit(
                    item, 
                    visited, 
                    sorted, 
                    dependencies, 
                    ignoreDependencyCycles);

                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print("");
                }
            }

            return sorted;
        }

        private void Visit(
            T item, 
            HashSet<T> visited, 
            List<T> sorted, 
            Func<T, IEnumerable<T>> dependencies, 
            bool ignoreDependencyCycles)
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
                            Debug.Print(
                                "|     {0}", 
                                (dep as ISimulation).Element.Name);
                        }

                        Visit(
                            dep, 
                            visited, 
                            sorted, 
                            dependencies, 
                            ignoreDependencyCycles);
                    }

                    // add items with simulation dependencies
                    sorted.Add(item);
                }

                // add  items without simulation dependencies
                sorted.Add(item);
            }
            else if (!ignoreDependencyCycles && !sorted.Contains(item))
            {
                Debug.Print(
                    "Invalid dependency cycle: {0}", 
                    (item as Element).Name);
            }
        }
    }

    public class SimulationCompiler
    {
        #region StateSimulationDict

        public Dictionary<Type, Func<Element, ISimulation>> StateSimulationDict 
            = new Dictionary<Type, Func<Element, ISimulation>>()
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

        #region Connections

        private Dictionary<UInt32, List<Tuple<Pin, bool>>> MapPinsToWires(
            Element[] elements)
        {
            int length = elements.Length;
            var dict = new Dictionary<UInt32, List<Tuple<Pin, bool>>>();

            for (int i = 0; i < length; i++)
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

        private void Find(
            Pin root, 
            Pin pin, 
            Dictionary<UInt32, Tuple<Pin, bool>> connections, 
            Dictionary<UInt32, List<Tuple<Pin, bool>>> pinToWireDict, 
            int level)
        {
            var connectedPins = pinToWireDict[pin.ElementId]
                .Where(x => 
                {
                    return x.Item1 != pin 
                        && x.Item1 != root 
                        && connections.ContainsKey(x.Item1.ElementId) == false;
                });

            foreach (var p in connectedPins)
            {
                connections.Add(p.Item1.ElementId, p);

                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print(
                        "{0}    Pin: {1} | Inverted: {2} | SimulationParent: {3} | Type: {4}",
                        new string(' ', level),
                        p.Item1.ElementId,
                        p.Item2,
                        p.Item1.SimulationParent.ElementId,
                        p.Item1.Type);
                }

                if (p.Item1.Type == PinType.Undefined 
                    && pinToWireDict.ContainsKey(pin.ElementId) == true)
                {
                    Find(
                        root, 
                        p.Item1, 
                        connections, 
                        pinToWireDict, 
                        level + 4);
                }
            }
        }

        public void Find(Element[] elements)
        {
            if (SimulationSettings.EnableDebug)
            {
                Debug.Print("");
                Debug.Print(
                    "--- FindConnections(), elements.Count: {0}", 
                    elements.Count());
                Debug.Print("");
            }

            var pinToWireDict = MapPinsToWires(elements);

            var pins = elements.Where(x => x is IStateSimulation && x.Children != null)
                               .SelectMany(x => x.Children)
                               .Cast<Pin>()
                               .Where(p => (p.Type == PinType.Undefined || p.Type == PinType.Input) && pinToWireDict.ContainsKey(p.ElementId))
                               .ToArray();

            var length = pins.Length;

            for (int i = 0; i < length; i++)
            {
                var pin = pins[i];

                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print(
                        "Pin  {0} | SimulationParent: {1} | Type: {2}",
                        pin.ElementId,
                        (pin.SimulationParent != null) ? pin.SimulationParent.ElementId : UInt32.MaxValue,
                        pin.Type);
                }

                var connections = new Dictionary<UInt32, Tuple<Pin, bool>>();

                Find(
                    pin, 
                    pin, 
                    connections, 
                    pinToWireDict, 
                    0);

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

        #region Compile

        private void ProcessInput(Pin input, string level)
        {
            var connections = input.Connections;
            var length = connections.Length;

            for (int i = 0; i < length; i++)
            {
                var connection = connections[i];
                bool isUndefined = connection.Item1.Type == PinType.Undefined;

                if (!(connection.Item1.SimulationParent is Context) && isUndefined)
                {
                    if (!(connection.Item1.SimulationParent is Tag))
                    {
                        connection.Item1.Type = PinType.Output;

                        if (SimulationSettings.EnableDebug)
                        {
                            Debug.Print(
                                "{0}{1} -> {2}", 
                                level, 
                                connection.Item1.ElementId, 
                                connection.Item1.Type);
                        }
                    }
                    else
                    {
                        if (SimulationSettings.EnableDebug)
                        {
                            Debug.Print(
                                "{0}(*) {1} -> {2}", 
                                level, 
                                connection.Item1.ElementId, 
                                connection.Item1.Type);
                        }
                    }

                    if (connection.Item1.SimulationParent != null && isUndefined)
                    {
                        ProcessOutput(
                            connection.Item1, 
                            string.Concat(level, "    "));
                    }
                }
            }
        }

        private void ProcessOutput(Pin output, string level)
        {
            var pins = output.SimulationParent
                .Children
                .Where(p => p != output)
                .Cast<Pin>();

            foreach (var pin in pins)
            {
                bool isUndefined = pin.Type == PinType.Undefined;

                if (!(pin.SimulationParent is Context)
                    && !(pin.SimulationParent is Tag) 
                    && isUndefined)
                {
                    pin.Type = PinType.Input;

                    if (SimulationSettings.EnableDebug)
                    {
                        Debug.Print(
                            "{0}{1} -> {2}", 
                            level, 
                            pin.ElementId, 
                            pin.Type);
                    }
                }

                if (pin.Connections != null 
                    && pin.Connections.Length > 0 
                    && isUndefined)
                {
                    ProcessInput(pin, level);
                }
            }
        }

        private void FindPinTypes(IEnumerable<Element> elements)
        {
            // find input connections
            var connections = elements
                .Where(x => x.Children != null)
                .SelectMany(x => x.Children)
                .Cast<Pin>()
                .Where(p => 
                {
                    return p.Connections != null 
                        && p.Type == PinType.Input 
                        && p.Connections.Length > 0 
                        && p.Connections.Any(i => i.Item1.Type == PinType.Undefined);
                })
                .ToArray();

            var length = connections.Length;
            if (length == 0)
            {
                return;
            }

            // process all input connections
            for (int i = 0; i < length; i++)
            {
                var connection = connections[i];
                var simulation = connection.SimulationParent as IStateSimulation;

                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print(
                        "+ {0} -> {1}", 
                        connection.ElementId, 
                        connection.Type);
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
            var length = simulations.Count;

            for (int i = 0; i < length; i++)
            {
                var state = new BoolState();
                var simulation = simulations[i];

                state.State = simulation.InitialState;

                simulation.State = state;
            }
        }

        private void GenerateCompileCache(
            List<ISimulation> simulations, 
            IClock clock)
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

            var length = simulations.Count;

            for (int i = 0; i < length; i++)
            {
                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print(
                        "--- compilation: {0} | Type: {1} ---", 
                        simulations[i].Element.ElementId, 
                        simulations[i].GetType());
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

        private SimulationCache Compile(
            Element[] elements, 
            IClock clock)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var cache = new SimulationCache();

            // -- step 1: reset pin connections ---
            var pins = elements.Where(x => x is Pin).Cast<Pin>();

            Reset(pins);

            // -- step 2: initialize IStateSimulation simulation
            var simulations = new List<ISimulation>();

            var length = elements.Length;
            for (int i = 0; i < length; i++)
            {
                var element = elements[i];
                if (element is IStateSimulation)
                {
                    var simulation = StateSimulationDict[element.GetType()](element);
                    simulations.Add(simulation);
                }
            }

            // -- step 3: update pin connections ---
            Find(elements);

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

        public SimulationCache Compile(
            IEnumerable<Context> contexts, 
            IEnumerable<Tag> tags, 
            IClock clock)
        {
            var elements = contexts.SelectMany(x => x.Children).Concat(tags).ToArray();

            // compile elements
            var cache = Compile(elements, clock);

            // collect unused memory
            System.GC.Collect();

            return cache;
        }

        public SimulationCache Compile(
            Context context, 
            IEnumerable<Tag> tags, 
            IClock clock)
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

    public class SimulationController
    {
        #region Properties

        public SimulationCompiler Compiler { get; private set; }
        public SimulationContext SimulationContext { get; set; }
        public bool IsConsole { get; set; }

        #endregion

        #region Constructor

        public SimulationController()
        {
            Compiler = new SimulationCompiler();
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

                Debug.Print(
                    "Simulation for {0} contexts, elements: {1}", 
                    contexts.Count(), elements.Count());
                Debug.Print(
                    "Debug Simulation Enabled: {0}", 
                    SimulationSettings.EnableDebug);
                Debug.Print(
                    "Have Cache: {0}", 
                    SimulationContext.Cache == null ? false : SimulationContext.Cache.HaveCache);
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

                        if (SimulationSettings.EnableDebug)
                        {
                            var debug = string.Format(
                                "Cycle {0} | {1}ms | vt:{2} rt:{3} dt:{4} id:{5}",
                                SimulationContext.SimulationClock.Cycle,
                                sw.Elapsed.TotalMilliseconds,
                                virtualTime.TotalMilliseconds,
                                realTime.Elapsed.TotalMilliseconds,
                                DateTime.Now - dt,
                                System.Threading.Thread.CurrentThread.ManagedThreadId);

                            if (IsConsole)
                            {
                                Console.Title = debug;
                            }
                            else
                            {
                                Debug.Print(debug);
                            }
                        }
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

            var length = simulations.Length;

            if (SimulationSettings.EnableDebug)
            {
                Debug.Print("");
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 0; i < length; i++)
            {
                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print(
                        "--- simulation: {0} | Type: {1} ---", 
                        simulations[i].Element.ElementId, 
                        simulations[i].GetType());
                    Debug.Print("");
                }

                simulations[i].Calculate();
            }

            sw.Stop();

            if (SimulationSettings.EnableDebug)
            {
                Debug.Print(
                    "Calculate() done in: {0}ms | {1} elements", 
                    sw.Elapsed.TotalMilliseconds, length);
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
            if (SimulationContext != null 
                && SimulationContext.SimulationTimer != null)
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

    public class SolutionRenamer
    {
        private static string CountersNamepsace = "Sheet.Simulation";

        private static Dictionary<string, string> ShortElementNames
            = new Dictionary<string, string>()
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

        private Dictionary<string, int> GetCountersDictionary()
        {
            // element counters based on type
            var types = System.Reflection.Assembly
                .GetAssembly(typeof(Solution))
                .GetTypes()
                .Where(x => x.IsClass && x.Namespace == CountersNamepsace)
                .Select(y => y.ToString().Split('.').Last());

            // counters: key = element Type, value = element counter
            var counters = new Dictionary<string, int>();
            foreach (var type in types)
            {
                counters.Add(type, 0);
            }
            return counters;
        }

        public void Rename(Solution solution)
        {
            // counters: key = element Type, value = element counter
            Dictionary<string, int> counters = GetCountersDictionary();

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
                string project_name = string.Format(
                    "{0}{1}",
                    ShortElementNames["Project"],
                    ++counters["Project"]);

                project.Name = project_name;
                ids.Add(project.Id, project_name);

                //Debug.Print("Project: {0} : {1}", project.Name, project.Id);

                // rename contexts
                var contexts = project.Children.Cast<Context>();

                Rename(contexts, counters, ids);
            }
        }

        public void Rename(Project project)
        {
            // counters: key = element Type, value = element counter
            Dictionary<string, int> counters = GetCountersDictionary();

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

            Rename(contexts, counters, ids);
        }

        public void Rename(Context context)
        {
            // counters: key = element Type, value = element counter
            Dictionary<string, int> counters = GetCountersDictionary();

            // dict: key = element Id, value = generated name for simulation
            Dictionary<string, string> ids = new Dictionary<string, string>();

            Rename(context, counters, ids);
        }

        private void Rename(
            IEnumerable<Context> contexts,
            Dictionary<string, int> counters,
            Dictionary<string, string> ids)
        {
            foreach (var context in contexts)
            {
                Rename(context, counters, ids);
            }
        }

        private void Rename(
            Context context,
            Dictionary<string, int> counters,
            Dictionary<string, string> ids)
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

    public class SolutionSerializer
    {
        #region Fields

        private ObservableCollection<Tag> tags = null;
        private Dictionary<int, Pin> map = null;

        #endregion

        #region Compare

        private bool Compare(string strA, string strB)
        {
            return string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase) == 0;
        }

        #endregion

        #region Ids

        private int SetId(IBlock parent, int nextId)
        {
            foreach (var point in parent.Points)
            {
                point.Id = nextId++;
            }

            foreach (var line in parent.Lines)
            {
                line.Id = nextId++;
            }

            foreach (var block in parent.Blocks)
            {
                block.Id = nextId++;
            }

            return nextId;
        }

        #endregion

        #region Create

        private Tag CreateSignalTag(
            string designation, 
            string description, 
            string signal, 
            string condition)
        {
            var tag = new Tag() { Id = Guid.NewGuid().ToString() };
            tag.Properties.Add("Designation", new TagProperty(designation));
            tag.Properties.Add("Description", new TagProperty(description));
            tag.Properties.Add("Signal", new TagProperty(signal));
            tag.Properties.Add("Condition", new TagProperty(condition));
            return tag;
        }

        private Wire CreateWire(
            Context context, 
            Pin start, 
            Pin end)
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

        private Pin CreatePin(
            Context context, 
            double x, 
            double y, 
            Element parent, 
            string name = "", 
            string factoryName = "", 
            PinType type = PinType.Undefined, 
            bool pinTypeUndefined = true)
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
            var tag = CreateSignalTag("tag" + block.Id.ToString(), "", "", "");
            tags.Add(tag);

            var element = new Signal()
            {
                Name = block.Name.ToLower() + block.Id.ToString(),
                Id = Guid.NewGuid().ToString(),
                X = block.X,
                Y = block.Y,
                Tag = tag,
                Parent = context,
                Block = block
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
                Parent = context,
                Block = block
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
                Parent = context,
                Block = block
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
                Delay = 1.0f,
                Block = block
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
                Delay = 1.0f,
                Block = block
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
                Delay = 1.0f,
                Block = block
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

        #endregion

        #region Serialize

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
                throw new Exception("Unsupported block name.");
            }
        }

        private void SerializerContents(Context context, IBlock root)
        {
            SetId(root, 1);

            foreach (var block in root.Blocks)
            {
                Serialize(context, block);
            }

            foreach (var point in root.Points)
            {
                Serialize(context, point);
            }

            foreach (var line in root.Lines)
            {
                Serialize(context, line);
            }
        }

        public Solution Serialize(IBlock root)
        {
            var solution = new Solution()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "solution"
            };

            var project = new Project()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "project",
                Parent = solution
            };

            solution.Children.Add(project);

            var context = new Context()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "context",
                Parent = project
            };

            project.Children.Add(context);

            map = new Dictionary<int, Pin>();
            tags = new ObservableCollection<Tag>();

            SerializerContents(context, root);

            solution.Tags = tags;

            return solution;
        }

        #endregion
    }

    public interface IUpdateState
    {
        void Set(ILine line, IBoolState state);
        void Set(IRectangle rectangle, IBoolState state);
        void Set(IEllipse ellipse, IBoolState state);
        void Set(IText text, IBoolState state);
        void Set(IImage image, IBoolState state);
        void Set(IBlock parent, IBoolState state);
    }

    public class SolutionSimulation
    {
        private Solution _solution;
        private Clock _clock;
        private SimulationController _controller;
        private int _periodInMillisencods;

        public SolutionSimulation(
            Solution solution, 
            int periodInMillisencods)
        {
            _solution = solution;
            _periodInMillisencods = periodInMillisencods;

            _controller = new SimulationController();
            _clock = new Clock();

            // create simulation context
            _controller.SimulationContext = new SimulationContext()
            {
                Cache = null,
                SimulationClock = _clock
            };

            _controller.IsConsole = false;

            // disable Console debug output
            SimulationSettings.EnableDebug = false;

            // disable Log for Run()
            SimulationSettings.EnableLog = false;
        }

        private void ResetTags()
        {
            if (_solution == null || _solution.Tags == null)
            {
                return;
            }

            // clear Tags children
            foreach (var tag in _solution.Tags)
            {
                foreach (var pin in tag.Children.Cast<Pin>())
                {
                    pin.SimulationParent = null;
                }

                if (tag.Children != null)
                {
                    tag.Children.Clear();
                }
            }
        }

        private void MapSignalToTag(Signal signal, Tag tag)
        {
            if (signal == null
                || signal.Children == null
                || tag == null
                || tag.Children == null)
            {
                return;
            }

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

        private void MapTags(List<Context> contexts)
        {
            var signals = contexts
                .SelectMany(x => x.Children)
                .Where(y => y is Signal)
                .Cast<Signal>()
                .ToList();

            ResetTags();

            MapSignalsToTag(signals);
        }

        private void DebugPrintTagMap()
        {
            foreach (var tag in _solution.Tags)
            {
                Debug.Print("{0}", tag.Properties["Designation"].Data);
                foreach (var pin in tag.Children.Cast<Pin>())
                {
                    Debug.Print(
                        "    -> pin type: {0}, parent: {1}, id: {2}", 
                        pin.Type, 
                        pin.Parent.Name, 
                        pin.Id);
                }
            }
        }

        private void UpdateSignalBlockStates(
            Signal[] signals, 
            IUpdateState state)
        {
            bool update = false;

            // check for changes after 1st cycle
            if (_clock.Cycle > 1)
            {
                foreach (var signal in signals)
                {
                    var tag = signal.Tag;
                    if (tag != null 
                        && tag.Simulation != null 
                        && tag.Simulation.State.State != tag.Simulation.State.PreviousState)
                    {
                        update = true;
                        state.Set(signal.Block, tag.Simulation.State);
                    }
                }

                if (update == false)
                {
                    return;
                }
            }
            // init all states in 1st cycle
            else
            {
                foreach (var signal in signals)
                {
                    var tag = signal.Tag;
                    if (tag != null 
                        && tag.Simulation != null)
                    {
                        state.Set(signal.Block, tag.Simulation.State);
                    }
                }
            }

            // set signal previous state
            foreach (var signal in signals)
            {
                var tag = signal.Tag;
                if (tag != null)
                {
                    tag.Simulation.State.PreviousState = tag.Simulation.State.State;
                }
            }
        }

        private Action GetUpdateAction(List<Context> contexts)
        {
            var signals = contexts
                .SelectMany(x => x.Children)
                .Where(y => y is Signal)
                .Cast<Signal>()
                .ToArray();

            var dispatcher = Dispatcher.CurrentDispatcher;
            var state = new WpfUpdateState();

            var update = new Action(() => UpdateSignalBlockStates(signals, state));
            var updateOnUIThread = new Action(() => dispatcher.BeginInvoke(update));

            return updateOnUIThread;
        }

        public void Start()
        {
            // simulation is already running
            if (_controller.SimulationContext.SimulationTimer != null)
            {
                return;
            }

            if (_solution == null)
            {
                return;
            }

            var projects = _solution.Children.Cast<Project>();
            if (projects == null)
            {
                return;
            }

            var contexts = projects.SelectMany(x => x.Children).Cast<Context>().ToList();
            if (contexts == null)
            {
                return;
            }

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
            _controller.Reset(true);

            // run simulation
            _controller.Run(
                contexts, 
                _solution.Tags, 
                period, 
                GetUpdateAction(contexts));

            // reset simulation parent
            foreach (var element in elements)
            {
                element.SimulationParent = null;
            }
        }

        public void Stop()
        {
            if (_controller.SimulationContext.SimulationTimer != null)
            {
                // stop simulation
                _controller.Stop();

                // reset previous simulation cache
                _controller.Reset(true);

                ResetTags();
            }
        }

        public void EnableDebug(bool enable)
        {
            SimulationSettings.EnableDebug = enable;
        }

        public void EnableLog(bool enable)
        {
            SimulationSettings.EnableLog = enable;
        }
    }
}
