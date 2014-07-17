using Sheet.Simulation.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Simulation.Elements
{
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
}
