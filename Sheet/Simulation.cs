using Sheet.Block;
using Sheet.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Sheet.Simulation
{
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

    public class BoolState : NotifyObject, IBoolState
    {
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

    public enum PinType
    {
        Undefined,
        Input,
        Output
    }

    public class Pin : Element
    {
        public Pin() : base() { }
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
        public Signal() : base() { }
        public Tag Tag { get; set; }
    }

    public class TagProperty
    {
        public TagProperty() : base() { }
        public TagProperty(object data) : this() { Data = data; }
        public object Data { get; set; }
    }

    public class Tag : Element, IStateSimulation
    {
        public Tag() : base() { Properties = new Dictionary<string, TagProperty>(); }
        public IDictionary<string, TagProperty> Properties { get; set; }
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
        public Solution() : base() { Tags = new ObservableCollection<Tag>(); }
        public ObservableCollection<Tag> Tags { get; set; }
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

    public class BufferGate : Element, IStateSimulation
    {
        public BufferGate() : base() { }
        public ISimulation Simulation { get; set; }
    }

    public class NotGate : Element, IStateSimulation
    {
        public NotGate() : base() { }
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

    public class NandGate : Element, IStateSimulation
    {
        public NandGate() : base() { }
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

        private bool? CalculateState(Tuple<IBoolState, bool>[] states)
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

        private bool? CalculateState(Tuple<IBoolState, bool>[] states)
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

    public static class SimulationSettings
    {
        public static bool EnableDebug { get; set; }
        public static bool EnableLog { get; set; }
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
    }

    public class Compiler
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

        private void Find(
            Pin root, 
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

    public class SolutionBinarySerializer
    {
        #region BinaryCommandId

        public enum BinaryCommandId : ushort
        {
            Solution = 0,
            Project = 1,
            Context = 2,
            Pin = 3,
            Signal = 4,
            AndGate = 5,
            OrGate = 6,
            TimerOn = 7,
            TimerOff = 8,
            TimerPulse = 9,
            Connect = 10
        } 

        #endregion

        #region Fields

        public static int SizeOfCommandId = Marshal.SizeOf(typeof(UInt16));
        public static int SizeOfTotalElements = Marshal.SizeOf(typeof(UInt32));

        // dict: key = element Id, value = generated element id for simulation
        private Dictionary<string, UInt32> Ids;

        // generated element id
        private UInt32 ElementId;

        // command id
        private BinaryCommandId CommandId;

        // bool true Byte value
        private const Byte TrueByte = 0x01;

        // bool false Byte value
        private const Byte FalseByte = 0x00;

        #endregion

        #region Open

        public Solution Open(string path)
        {
            using (var fs = File.OpenRead(path))
            {
                using (var ms = new MemoryStream())
                {
                    fs.CopyTo(ms);
                    return Read(ms);
                }
            }
        }

        #endregion

        #region Read

        private Solution Read(MemoryStream ms)
        {
            Element[] elements = null;
            Solution currentSolution = null;
            Project currentProject = null;
            Context currentContext = null;

            using (var reader = new BinaryReader(ms))
            {
                UInt32 totalElements = 0;
                BinaryCommandId commandId = 0;

                long size = ms.Length;
                long dataSize = size - SizeOfTotalElements;

                // get element counter
                ms.Seek(size - SizeOfTotalElements, SeekOrigin.Begin);

                totalElements = reader.ReadUInt32();

                ms.Seek(0, SeekOrigin.Begin);

                // allocate elements array
                elements = new Element[totalElements];

                while (ms.Position < dataSize)
                {
                    commandId = (BinaryCommandId)reader.ReadUInt16();

                    switch (commandId)
                    {
                        // Solution
                        case BinaryCommandId.Solution:
                            {
                                UInt32 Id = reader.ReadUInt32();

                                var solution = AddSolution(elements, ref Id);
                                currentSolution = solution;
                            }
                            break;
                        // Project
                        case BinaryCommandId.Project:
                            {
                                UInt32 Id = reader.ReadUInt32();

                                var project = AddProject(elements, currentSolution, ref Id);
                                currentProject = project;
                            }
                            break;
                        // Context
                        case BinaryCommandId.Context:
                            {
                                UInt32 Id = reader.ReadUInt32();

                                var context = AddContext(elements, currentProject, ref Id);
                                currentContext = context;
                            }
                            break;
                        // Pin
                        case BinaryCommandId.Pin:
                            {
                                UInt32 Id = reader.ReadUInt32();

                                AddPin(elements, currentContext, ref Id);
                            }
                            break;
                        // Signal
                        case BinaryCommandId.Signal:
                            {
                                UInt32 Id = reader.ReadUInt32();
                                UInt32 InputPinId = reader.ReadUInt32();
                                UInt32 OutputPinId = reader.ReadUInt32();

                                AddSignal(elements, currentContext, ref Id, ref InputPinId, ref OutputPinId);
                            }
                            break;
                        // AndGate
                        case BinaryCommandId.AndGate:
                            {
                                UInt32 Id = reader.ReadUInt32();
                                UInt32 LeftPinId = reader.ReadUInt32();
                                UInt32 RightPinId = reader.ReadUInt32();
                                UInt32 TopPinId = reader.ReadUInt32();
                                UInt32 BottomPinId = reader.ReadUInt32();

                                AddAndGate(elements, currentContext, ref Id, ref LeftPinId, ref RightPinId, ref TopPinId, ref BottomPinId);
                            }
                            break;
                        // OrGate
                        case BinaryCommandId.OrGate:
                            {
                                UInt32 Id = reader.ReadUInt32();
                                UInt32 LeftPinId = reader.ReadUInt32();
                                UInt32 RightPinId = reader.ReadUInt32();
                                UInt32 TopPinId = reader.ReadUInt32();
                                UInt32 BottomPinId = reader.ReadUInt32();

                                AddOrGate(elements, currentContext, ref Id, ref LeftPinId, ref RightPinId, ref TopPinId, ref BottomPinId);
                            }
                            break;
                        // TimerOn
                        case BinaryCommandId.TimerOn:
                            {
                                UInt32 Id = reader.ReadUInt32();
                                UInt32 LeftPinId = reader.ReadUInt32();
                                UInt32 RightPinId = reader.ReadUInt32();
                                UInt32 TopPinId = reader.ReadUInt32();
                                UInt32 BottomPinId = reader.ReadUInt32();
                                Single Delay = reader.ReadSingle();

                                AddTimerOn(elements, currentContext, ref Id, ref LeftPinId, ref RightPinId, ref TopPinId, ref BottomPinId, ref Delay);
                            }
                            break;
                        // TimerOff
                        case BinaryCommandId.TimerOff:
                            {
                                UInt32 Id = reader.ReadUInt32();
                                UInt32 LeftPinId = reader.ReadUInt32();
                                UInt32 RightPinId = reader.ReadUInt32();
                                UInt32 TopPinId = reader.ReadUInt32();
                                UInt32 BottomPinId = reader.ReadUInt32();
                                Single Delay = reader.ReadSingle();

                                AddTimerOff(elements, currentContext, ref Id, ref LeftPinId, ref RightPinId, ref TopPinId, ref BottomPinId, ref Delay);
                            }
                            break;
                        // TimerPulse
                        case BinaryCommandId.TimerPulse:
                            {
                                UInt32 Id = reader.ReadUInt32();
                                UInt32 LeftPinId = reader.ReadUInt32();
                                UInt32 RightPinId = reader.ReadUInt32();
                                UInt32 TopPinId = reader.ReadUInt32();
                                UInt32 BottomPinId = reader.ReadUInt32();
                                Single Delay = reader.ReadSingle();

                                AddTimerPulse(elements, currentContext, ref Id, ref LeftPinId, ref RightPinId, ref TopPinId, ref BottomPinId, ref Delay);
                            }
                            break;
                        // Connect
                        case BinaryCommandId.Connect:
                            {
                                UInt32 Id = reader.ReadUInt32();
                                UInt32 SrcPinId = reader.ReadUInt32();
                                UInt32 DstPinId = reader.ReadUInt32();
                                Byte InvertStart = reader.ReadByte();
                                Byte InvertEnd = reader.ReadByte();

                                AddWire(elements, currentContext, ref Id, ref SrcPinId, ref DstPinId, ref InvertStart, ref InvertEnd);
                            }
                            break;
                        default:
                            throw new NotSupportedException("Not supported command id.");
                    }
                }

                // reset elements cache array
                for (UInt32 i = 0; i < totalElements; i++)
                {
                    elements[i] = null;
                }

                elements = null;
            }

            return currentSolution;
        }

        #endregion

        #region Add

        private void AddWire(Element[] elements, Context context, ref UInt32 Id, ref UInt32 SrcPinId, ref UInt32 DstPinId, ref Byte InvertStart, ref Byte InvertEnd)
        {
            var children = context.Children;

            var p_src = elements[SrcPinId];
            var p_dst = elements[DstPinId];

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

                elements[Id] = wire;
            }
        }

        private void AddTimerPulse(Element[] elements, Context context, ref UInt32 Id, ref UInt32 LeftPinId, ref UInt32 RightPinId, ref UInt32 TopPinId, ref UInt32 BottomPinId, ref Single Delay)
        {
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

            elements[Id] = tp;
            elements[TopPinId] = p_top;
            elements[BottomPinId] = p_bottom;
            elements[LeftPinId] = p_left;
            elements[RightPinId] = p_right;
        }

        private void AddTimerOff(Element[] elements, Context context, ref UInt32 Id, ref UInt32 LeftPinId, ref UInt32 RightPinId, ref UInt32 TopPinId, ref UInt32 BottomPinId, ref Single Delay)
        {
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

            elements[Id] = toff;
            elements[TopPinId] = p_top;
            elements[BottomPinId] = p_bottom;
            elements[LeftPinId] = p_left;
            elements[RightPinId] = p_right;
        }

        private void AddTimerOn(Element[] elements, Context context, ref UInt32 Id, ref UInt32 LeftPinId, ref UInt32 RightPinId, ref UInt32 TopPinId, ref UInt32 BottomPinId, ref Single Delay)
        {
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

            elements[Id] = ton;
            elements[TopPinId] = p_top;
            elements[BottomPinId] = p_bottom;
            elements[LeftPinId] = p_left;
            elements[RightPinId] = p_right;
        }

        private void AddOrGate(Element[] elements, Context context, ref UInt32 Id, ref UInt32 LeftPinId, ref UInt32 RightPinId, ref UInt32 TopPinId, ref UInt32 BottomPinId)
        {
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

            elements[Id] = og;
            elements[TopPinId] = p_top;
            elements[BottomPinId] = p_bottom;
            elements[LeftPinId] = p_left;
            elements[RightPinId] = p_right;
        }

        private void AddAndGate(Element[] elements, Context context, ref UInt32 Id, ref UInt32 LeftPinId, ref UInt32 RightPinId, ref UInt32 TopPinId, ref UInt32 BottomPinId)
        {
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

            elements[Id] = ag;
            elements[TopPinId] = p_top;
            elements[BottomPinId] = p_bottom;
            elements[LeftPinId] = p_left;
            elements[RightPinId] = p_right;
        }

        private void AddSignal(Element[] elements, Context context, ref UInt32 Id, ref UInt32 InputPinId, ref UInt32 OutputPinId)
        {
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

            elements[Id] = signal;
            elements[InputPinId] = p_input;
            elements[OutputPinId] = p_output;
        }

        private void AddPin(Element[] elements, Context context, ref UInt32 Id)
        {
            var children = context.Children;

            var p = new Pin() { Type = PinType.Undefined, IsPinTypeUndefined = true }; //PIN

            p.ElementId = Id;

            p.Parent = context;

            children.Add(p);

            elements[Id] = p;
        }

        private Context AddContext(Element[] elements, Project project, ref UInt32 Id)
        {
            var context = new Context();
            elements[Id] = context;

            project.Children.Add(context);
            return context;
        }

        private Project AddProject(Element[] elements, Solution solution, ref UInt32 Id)
        {
            var project = new Project();
            elements[Id] = project;

            solution.Children.Add(project);
            return project;
        }

        private Solution AddSolution(Element[] elements, ref UInt32 Id)
        {
            var solution = new Solution();
            elements[Id] = solution;

            return solution;
        }

        #endregion

        #region Save

        public void Save(string path, Solution solution)
        {
            using (var stream = new FileStream(path, FileMode.Create))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    Ids = new Dictionary<string, UInt32>();
                    ElementId = 0;

                    // write solution
                    WriteSolution(solution, writer);

                    var projects = solution.Children.Cast<Project>();

                    // write projects
                    foreach (var project in projects)
                    {
                        WriteProjectAndChildren(project, writer);
                    }

                    // total elements counter
                    writer.Write(ElementId);
                }
            }
        }

        private void Save(string path, Project project)
        {
            using (var stream = new FileStream(path, FileMode.Create))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    Ids = new Dictionary<string, UInt32>();
                    ElementId = 0;

                    // write dummy solution
                    WriteDummySolution(writer);

                    // write project
                    WriteProjectAndChildren(project, writer);

                    // total elements counter
                    writer.Write(ElementId);
                }
            }
        }

        private void Save(string path, Context context)
        {
            using (var stream = new FileStream(path, FileMode.Create))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    Ids = new Dictionary<string, UInt32>();
                    ElementId = 0;

                    // write dummy solution
                    WriteDummySolution(writer);

                    // write dummy project
                    WriteDummyProject(writer);

                    // write context
                    WriteContextAndChildren(context, writer);

                    // total elements counter
                    writer.Write(ElementId);
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
            var elements = context.Children.Where(x => !(x is Wire));
            foreach (var element in elements)
            {
                WriteElement(element, writer);
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
                default:
                    throw new NotSupportedException("Not supported element type.");
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
            CommandId = BinaryCommandId.Solution;
            writer.Write((ushort)CommandId);
            writer.Write(ElementId);
            ElementId++;
        }

        private void WriteDummyProject(BinaryWriter writer)
        {
            CommandId = BinaryCommandId.Project;
            writer.Write((ushort)CommandId);
            writer.Write(ElementId);
            ElementId++;
        }

        private void WriteSolution(Solution solution, BinaryWriter writer)
        {
            CommandId = BinaryCommandId.Solution;
            writer.Write((ushort)CommandId);
            writer.Write(ElementId);
            Ids.Add(solution.Id, ElementId);
            ElementId++;
        }

        private void WriteProject(Project project, BinaryWriter writer)
        {
            CommandId = BinaryCommandId.Project;
            writer.Write((ushort)CommandId);
            writer.Write(ElementId);
            Ids.Add(project.Id, ElementId);
            ElementId++;
        }

        private void WriteContext(Context context, BinaryWriter writer)
        {
            CommandId = BinaryCommandId.Context;
            writer.Write((ushort)CommandId);
            writer.Write(ElementId);
            Ids.Add(context.Id, ElementId);
            ElementId++;
        }

        private void WritePin(Element element, BinaryWriter writer)
        {
            if (element.Parent == null || element.Parent is Context)
            {
                // pin id
                CommandId = BinaryCommandId.Pin;
                writer.Write((ushort)CommandId);
                writer.Write(ElementId);
                Ids.Add(element.Id, ElementId);
                ElementId++;
            }
        }

        private void WriteSignal(Element element, BinaryWriter writer)
        {
            // signal id
            CommandId = BinaryCommandId.Signal;
            writer.Write((ushort)CommandId);
            writer.Write(ElementId);
            Ids.Add(element.Id, ElementId);
            ElementId++;

            // input pin id
            var i = element.Children.Single(x => x.FactoryName == "I");
            i.ElementId = ElementId;
            writer.Write(ElementId);
            Ids.Add(i.Id, ElementId);
            ElementId++;

            // output pin id
            var o = element.Children.Single(x => x.FactoryName == "O");
            o.ElementId = ElementId;
            writer.Write(ElementId);
            Ids.Add(o.Id, ElementId);
            ElementId++;
        }

        private void WriteAndGate(Element element, BinaryWriter writer)
        {
            // andgate id
            CommandId = BinaryCommandId.AndGate;
            writer.Write((ushort)CommandId);
            writer.Write(ElementId);
            Ids.Add(element.Id, ElementId);
            ElementId++;

            // left pin id
            var left = element.Children.Single(x => x.FactoryName == "L");
            left.ElementId = ElementId;
            writer.Write(ElementId);
            Ids.Add(left.Id, ElementId);
            ElementId++;

            // right pin id
            var right = element.Children.Single(x => x.FactoryName == "R");
            right.ElementId = ElementId;
            writer.Write(ElementId);
            Ids.Add(right.Id, ElementId);
            ElementId++;

            // top pin id
            var top = element.Children.Single(x => x.FactoryName == "T");
            top.ElementId = ElementId;
            writer.Write(ElementId);
            Ids.Add(top.Id, ElementId);
            ElementId++;

            // bottom pin id
            var bottom = element.Children.Single(x => x.FactoryName == "B");
            bottom.ElementId = ElementId;
            writer.Write(ElementId);
            Ids.Add(bottom.Id, ElementId);
            ElementId++;
        }

        private void WriteOrGate(Element element, BinaryWriter writer)
        {
            // orgate id
            CommandId = BinaryCommandId.OrGate;
            writer.Write((ushort)CommandId);
            writer.Write(ElementId);
            Ids.Add(element.Id, ElementId);
            ElementId++;

            // left pin id
            var left = element.Children.Single(x => x.FactoryName == "L");
            left.ElementId = ElementId;
            writer.Write(ElementId);
            Ids.Add(left.Id, ElementId);
            ElementId++;

            // right pin id
            var right = element.Children.Single(x => x.FactoryName == "R");
            right.ElementId = ElementId;
            writer.Write(ElementId);
            Ids.Add(right.Id, ElementId);
            ElementId++;

            // top pin id
            var top = element.Children.Single(x => x.FactoryName == "T");
            top.ElementId = ElementId;
            writer.Write(ElementId);
            Ids.Add(top.Id, ElementId);
            ElementId++;

            // bottom pin id
            var bottom = element.Children.Single(x => x.FactoryName == "B");
            bottom.ElementId = ElementId;
            writer.Write(ElementId);
            Ids.Add(bottom.Id, ElementId);
            ElementId++;
        }

        private void WriteTimerOn(Element element, BinaryWriter writer)
        {
            // timeron id
            CommandId = BinaryCommandId.TimerOn;
            writer.Write((ushort)CommandId);
            writer.Write(ElementId);
            Ids.Add(element.Id, ElementId);
            ElementId++;

            // left pin id
            var left = element.Children.Single(x => x.FactoryName == "L");
            left.ElementId = ElementId;
            writer.Write(ElementId);
            Ids.Add(left.Id, ElementId);
            ElementId++;

            // right pin id
            var right = element.Children.Single(x => x.FactoryName == "R");
            right.ElementId = ElementId;
            writer.Write(ElementId);
            Ids.Add(right.Id, ElementId);
            ElementId++;

            // top pin id
            var top = element.Children.Single(x => x.FactoryName == "T");
            top.ElementId = ElementId;
            writer.Write(ElementId);
            Ids.Add(top.Id, ElementId);
            ElementId++;

            // bottom pin id
            var bottom = element.Children.Single(x => x.FactoryName == "B");
            bottom.ElementId = ElementId;
            writer.Write(ElementId);
            Ids.Add(bottom.Id, ElementId);
            ElementId++;

            // delay
            writer.Write((element as ITimer).Delay);
        }

        private void WriteTimerOff(Element element, BinaryWriter writer)
        {
            // timeroff id
            CommandId = BinaryCommandId.TimerOff;
            writer.Write((ushort)CommandId);
            writer.Write(ElementId);
            Ids.Add(element.Id, ElementId);
            ElementId++;

            // left pin id
            var left = element.Children.Single(x => x.FactoryName == "L");
            left.ElementId = ElementId;
            writer.Write(ElementId);
            Ids.Add(left.Id, ElementId);
            ElementId++;

            // right pin id
            var right = element.Children.Single(x => x.FactoryName == "R");
            right.ElementId = ElementId;
            writer.Write(ElementId);
            Ids.Add(right.Id, ElementId);
            ElementId++;

            // top pin id
            var top = element.Children.Single(x => x.FactoryName == "T");
            top.ElementId = ElementId;
            writer.Write(ElementId);
            Ids.Add(top.Id, ElementId);
            ElementId++;

            // bottom pin id
            var bottom = element.Children.Single(x => x.FactoryName == "B");
            bottom.ElementId = ElementId;
            writer.Write(ElementId);
            Ids.Add(bottom.Id, ElementId);
            ElementId++;

            // delay
            writer.Write((element as ITimer).Delay);
        }

        private void WriteTimerPulse(Element element, BinaryWriter writer)
        {
            // timerpulse id
            CommandId = BinaryCommandId.TimerPulse;
            writer.Write((ushort)CommandId);
            writer.Write(ElementId);
            Ids.Add(element.Id, ElementId);
            ElementId++;

            // left pin id
            var left = element.Children.Single(x => x.FactoryName == "L");
            left.ElementId = ElementId;
            writer.Write(ElementId);
            Ids.Add(left.Id, ElementId);
            ElementId++;

            // right pin id
            var right = element.Children.Single(x => x.FactoryName == "R");
            right.ElementId = ElementId;
            writer.Write(ElementId);
            Ids.Add(right.Id, ElementId);
            ElementId++;

            // top pin id
            var top = element.Children.Single(x => x.FactoryName == "T");
            top.ElementId = ElementId;
            writer.Write(ElementId);
            Ids.Add(top.Id, ElementId);
            ElementId++;

            // bottom pin id
            var bottom = element.Children.Single(x => x.FactoryName == "B");
            bottom.ElementId = ElementId;
            writer.Write(ElementId);
            Ids.Add(bottom.Id, ElementId);
            ElementId++;

            // delay
            writer.Write((element as ITimer).Delay);
        }

        private void WriteWire(Wire wire, BinaryWriter writer)
        {
            UInt32 srcPinId;
            UInt32 dstPinId;
            Byte invertStart;
            Byte invertEnd;

            srcPinId = Ids[wire.Start.Id];
            dstPinId = Ids[wire.End.Id];

            invertStart = wire.InvertStart ? TrueByte : FalseByte;
            invertEnd = wire.InvertEnd ? TrueByte : FalseByte;

            // connect id
            CommandId = BinaryCommandId.Connect;
            writer.Write((ushort)CommandId);
            writer.Write(ElementId);
            Ids.Add(wire.Id, ElementId);
            ElementId++;

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

    public interface IBlockStateUpdate
    {
        void SeState(ILine line, IBoolState state);
        void SeState(IRectangle rectangle, IBoolState state);
        void SeState(IEllipse ellipse, IBoolState state);
        void SeState(IText text, IBoolState state);
        void SeState(IImage image, IBoolState state);
        void SeState(IBlock parent, IBoolState state);
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

        private void MapTags(List<Context> contexts)
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

        private void UpdateSignalBlockStates(Signal[] signals, IBlockStateUpdate helper)
        {
            bool update = false;

            // check for changes after 1st cycle
            if (_clock.Cycle > 1)
            {
                foreach (var signal in signals)
                {
                    var tag = signal.Tag;
                    if (tag != null && tag.Simulation != null && tag.Simulation.State.State != tag.Simulation.State.PreviousState)
                    {
                        update = true;
                        helper.SeState(signal.Block, tag.Simulation.State);
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
                    if (tag != null && tag.Simulation != null)
                    {
                        helper.SeState(signal.Block, tag.Simulation.State);
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
            var signals = contexts.SelectMany(x => x.Children).Where(y => y is Signal).Cast<Signal>().ToArray();
            var dispatcher = Dispatcher.CurrentDispatcher;
            var helper = new WpfBlockStateUpdate();

            var update = new Action(() => UpdateSignalBlockStates(signals, helper));
            var updateOnUIThread = new Action(() => dispatcher.BeginInvoke(update));

            return updateOnUIThread;
        }

        public void Start()
        {
            // simulation is already running
            if (_controller.SimulationContext.SimulationTimer != null)
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
            _controller.Reset(true);

            // run simulation
            _controller.Run(contexts, _solution.Tags, period, GetUpdateAction(contexts));

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
