using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Simulation
{
    #region Model.Core

    public abstract class NotifyObject : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Implementation

        public virtual void Notify(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    public interface IId
    {
        string Id { get; set; }
    }

    public interface ILocation
    {
        double X { get; set; }
        double Y { get; set; }
        double Z { get; set; }
        bool IsLocked { get; set; }
    }

    public interface ISelected
    {
        bool IsSelected { get; set; }
    }

    public abstract class Element : NotifyObject, IId, ILocation, ISelected
    {
        #region Construtor

        public Element() { }

        #endregion

        #region IId Implementation

        private string id;

        public string Id
        {
            get { return id; }
            set
            {
                if (value != id)
                {
                    id = value;
                    Notify("Id");
                }
            }
        }

        #endregion

        #region ILocation Implementation

        private double x;
        private double y;
        private double z;
        private bool isLocked = false;

        public double X
        {
            get { return x; }
            set
            {
                if (value != x)
                {
                    x = value;
                    Notify("X");
                }
            }
        }

        public double Y
        {
            get { return y; }
            set
            {
                if (value != y)
                {
                    y = value;
                    Notify("Y");
                }
            }
        }

        public double Z
        {
            get { return z; }
            set
            {
                if (value != z)
                {
                    z = value;
                    Notify("Z");
                }
            }
        }

        public bool IsLocked
        {
            get { return isLocked; }
            set
            {
                if (value != isLocked)
                {
                    isLocked = value;
                    Notify("IsLocked");
                }
            }
        }

        #endregion

        #region ISelected Implementation

        private bool isSelected = false;

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (value != isSelected)
                {
                    isSelected = value;
                    Notify("IsSelected");
                }
            }
        }

        #endregion

        #region Properties

        private UInt32 elementId;
        private string name = string.Empty;
        private string factoryName = string.Empty;
        private bool isEditable = true;
        private bool selectChildren = true;
        private Element parent = null;
        private ObservableCollection<Element> children = new ObservableCollection<Element>();

        public UInt32 ElementId
        {
            get { return elementId; }
            set
            {
                if (value != elementId)
                {
                    elementId = value;
                    Notify("ElementId");
                }
            }
        }

        public string Name
        {
            get { return name; }
            set
            {
                if (value != name)
                {
                    name = value;
                    Notify("Name");
                }
            }
        }

        public string FactoryName
        {
            get { return factoryName; }
            set
            {
                if (value != factoryName)
                {
                    factoryName = value;
                    Notify("FactoryName");
                }
            }
        }

        public bool IsEditable
        {
            get { return isEditable; }
            set
            {
                if (value != isEditable)
                {
                    isEditable = value;
                    Notify("IsEditable");
                }
            }
        }

        public bool SelectChildren
        {
            get { return selectChildren; }
            set
            {
                if (value != selectChildren)
                {
                    selectChildren = value;
                    Notify("SelectChildren");
                }
            }
        }

        public Element Parent
        {
            get { return parent; }
            set
            {
                if (value != parent)
                {
                    parent = value;
                    Notify("Parent");
                }
            }
        }

        public ObservableCollection<Element> Children
        {
            get { return children; }
            set
            {
                if (value != children)
                {
                    children = value;
                    Notify("Children");
                }
            }
        }

        public Element SimulationParent { get; set; }

        #endregion

        #region Clone

        public static ObservableCollection<T> CopyObservableCollection<T>(IEnumerable<T> list) where T : Element
        {
            return new ObservableCollection<T>(list.Select(x => x.Clone()).Cast<T>());
        }

        public abstract object Clone();

        #endregion
    }

    public interface ITimer
    {
        float Delay { get; set; }
        string Unit { get; set; }
    }

    public class Property : NotifyObject
    {
        #region Constructor

        public Property()
            : base()
        {
        }

        public Property(object data)
            : this()
        {
            this.data = data;
        }

        #endregion

        #region Properties

        private object data;

        public object Data
        {
            get { return data; }
            set
            {
                if (value != data)
                {
                    data = value;
                    Notify("Data");
                }
            }
        }

        #endregion
    }

    public class Location : NotifyObject, ILocation
    {
        #region Construtor

        public Location() { }

        #endregion

        #region ILocation Implementation

        private double x;
        private double y;
        private double z;
        private bool isLocked = false;

        public double X
        {
            get { return x; }
            set
            {
                if (value != x)
                {
                    x = value;
                    Notify("X");
                }
            }
        }

        public double Y
        {
            get { return y; }
            set
            {
                if (value != y)
                {
                    y = value;
                    Notify("Y");
                }
            }
        }

        public double Z
        {
            get { return z; }
            set
            {
                if (value != z)
                {
                    z = value;
                    Notify("Z");
                }
            }
        }

        public bool IsLocked
        {
            get { return isLocked; }
            set
            {
                if (value != isLocked)
                {
                    isLocked = value;
                    Notify("IsLocked");
                }
            }
        }

        #endregion
    }

    #endregion

    #region Model.Enums

    public enum PinType
    {
        Undefined,
        Input,
        Output
    }

    public enum PageType
    {
        Undefined,
        Title,
        Logic,
        Playground
    }

    public enum LabelPosition
    {
        Left,
        Right,
        Top,
        Bottom
    }

    public enum Alignment
    {
        Undefined,
        Left,
        Right,
        Top,
        Bottom
    }

    #endregion

    #region Model.Elements.Basic

    public class Pin : Element
    {
        #region Construtor

        public Pin() : base() { }

        #endregion

        #region Properties

        public Alignment Alignment { get; set; }
        public bool IsPinTypeUndefined { get; set; }
        public PinType Type { get; set; }

        // connection format: Tuple<Pin,bool> where bool is flag Inverted==True|False
        public Tuple<Pin, bool>[] Connections { get; set; }

        #endregion

        #region Clone

        public override object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }

    public class Signal : Element
    {
        #region Construtor

        public Signal() : base() { }

        #endregion

        #region Properties

        private Tag tag;
        public Tag Tag
        {
            get { return tag; }
            set
            {
                if (value != tag)
                {
                    tag = value;

                    Notify("Tag");
                }
            }
        }

        #endregion

        #region Clone

        public override object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }

    public class Wire : Element
    {
        #region Construtor

        public Wire()
            : base()
        {
            this.Initialize();
        }

        #endregion

        #region Methods

        public void CalculateLocation()
        {
            // check for valid stan/end pin
            if (this.start == null || this.end == null)
                return;

            // check if object was initialized
            if (this.startPoint == null || this.endPoint == null || this.startCenter == null || this.endCenter == null)
                return;

            Options options = Defaults.Options;

            if (options != null)
                this.thickness = options.Thickness / 2.0;
            else
                this.thickness = 1.0 / 2.0;

            double startX = this.start.X;
            double startY = this.start.Y;
            double endX = this.end.X;
            double endY = this.end.Y;

            // calculate new inverted start/end position
            double alpha = Math.Atan2(startY - endY, endX - startX);
            double theta = Math.PI - alpha;
            double zet = theta - Math.PI / 2;
            double sizeX = Math.Sin(zet) * (invertedThickness - thickness);
            double sizeY = Math.Cos(zet) * (invertedThickness - thickness);

            // TODO: shorten wire
            if (options != null && this.DisableShortenWire == false /* && options.CaptureMouse == false */)
            {
                bool isStartSignal = start.Parent is Signal;
                bool isEndSignal = end.Parent is Signal;

                // shorten start
                if (isStartSignal == true && isEndSignal == false && options.ShortenLineStarts == true)
                {
                    if (Math.Round(startY, 1) == Math.Round(endY, 1))
                    {
                        startX = end.X - 15;
                    }
                }

                // shorten end
                if (isStartSignal == false && isEndSignal == true && options.ShortenLineEnds == true)
                {
                    if (Math.Round(startY, 1) == Math.Round(endY, 1))
                    {
                        endX = start.X + 15;
                    }
                }
            }

            // set wire start location
            if (this.invertStart)
            {
                startCenter.X = startX + sizeX - invertedThickness;
                startCenter.Y = startY - sizeY - invertedThickness;

                startPoint.X = startX + (2 * sizeX);
                startPoint.Y = startY - (2 * sizeY);
            }
            else
            {
                startCenter.X = startX;
                startCenter.Y = startY;

                startPoint.X = startX;
                startPoint.Y = startY;
            }

            // set line end location
            if (this.invertEnd)
            {
                endCenter.X = endX - sizeX - invertedThickness;
                endCenter.Y = endY + sizeY - invertedThickness;

                endPoint.X = endX - (2 * sizeX);
                endPoint.Y = endY + (2 * sizeY);
            }
            else
            {
                endCenter.X = endX;
                endCenter.Y = endY;

                endPoint.X = endX;
                endPoint.Y = endY;
            }
        }

        public void Initialize()
        {
            this.startPoint = new Location();
            this.endPoint = new Location();
            this.startCenter = new Location();
            this.endCenter = new Location();

            this.invertedThickness = 10.0 / 2.0;

            Options options = Defaults.Options;

            if (options != null)
            {
                this.thickness = options.Thickness / 2.0;

                options.PropertyChanged += Options_PropertyChanged;
            }
            else
            {
                this.thickness = 1.0 / 2.0;
            }

            this.CalculateLocation();
        }

        private void Options_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // TODO: 
            if (e.PropertyName == "Thickness" || e.PropertyName == "ShortenLineStarts" || e.PropertyName == "ShortenLineEnds" || e.PropertyName == "CaptureMouse")
            {
                this.CalculateLocation();
            }
        }

        #endregion

        #region Events

        private void PinPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "X" || e.PropertyName == "Y")
            {
                this.CalculateLocation();
            }
        }

        #endregion

        #region Properties

        private double invertedThickness;
        private double thickness;

        private Pin start;
        private Pin end;

        private bool invertStart = false;
        private bool invertEnd = false;

        private bool disableShortenWire = false;

        private Location startPoint;
        private Location endPoint = new Location();
        private Location startCenter = new Location();
        private Location endCenter = new Location();

        public Pin Start
        {
            get { return start; }
            set
            {
                if (value != start)
                {
                    // remove listener for start pin change notifications
                    if (start != null)
                        start.PropertyChanged -= PinPropertyChanged;

                    if (start != null)
                        Children.Remove(start);

                    start = value;

                    // add listener for start pin change notifications
                    if (start != null)
                        start.PropertyChanged += PinPropertyChanged;

                    if (start != null)
                        Children.Add(start);

                    // update inveted start/end location
                    this.CalculateLocation();

                    Notify("Start");
                }
            }
        }

        public Pin End
        {
            get { return end; }
            set
            {
                if (value != end)
                {
                    // remove listener for end pin change notifications
                    if (end != null)
                        end.PropertyChanged -= PinPropertyChanged;

                    if (end != null)
                        Children.Remove(end);

                    end = value;

                    // add listener for end pin change notifications
                    if (end != null)
                        end.PropertyChanged += PinPropertyChanged;

                    if (end != null)
                        Children.Add(end);

                    // update inveted start/end location
                    this.CalculateLocation();

                    Notify("End");
                }
            }
        }

        public bool InvertStart
        {
            get { return invertStart; }
            set
            {
                if (value != invertStart)
                {
                    invertStart = value;

                    // update inveted start/end location
                    this.CalculateLocation();

                    Notify("InvertStart");
                }
            }
        }

        public bool DisableShortenWire
        {
            get { return disableShortenWire; }
            set
            {
                if (value != disableShortenWire)
                {
                    disableShortenWire = value;

                    // update inveted start/end location
                    this.CalculateLocation();

                    Notify("DisableShortenWire");
                }
            }
        }

        public bool InvertEnd
        {
            get { return invertEnd; }
            set
            {
                if (value != invertEnd)
                {
                    invertEnd = value;

                    // update inveted start/end location
                    this.CalculateLocation();

                    Notify("InvertEnd");
                }
            }
        }

        public Location StartPoint
        {
            get { return startPoint; }
            set
            {
                if (value != startPoint)
                {
                    startPoint = value;
                    Notify("StartPoint");
                }
            }
        }

        public Location EndPoint
        {
            get { return endPoint; }
            set
            {
                if (value != endPoint)
                {
                    endPoint = value;
                    Notify("EndPoint");
                }
            }
        }

        public Location StartCenter
        {
            get { return startCenter; }
            set
            {
                if (value != startCenter)
                {
                    startCenter = value;
                    Notify("StartCenter");
                }
            }
        }

        public Location EndCenter
        {
            get { return endCenter; }
            set
            {
                if (value != endCenter)
                {
                    endCenter = value;
                    Notify("EndCenter");
                }
            }
        }

        #endregion

        #region Clone

        public override object Clone()
        {
            Wire element = (Wire)this.MemberwiseClone();
            element.Children = Element.CopyObservableCollection<Element>(this.Children);
            element.Id = Guid.NewGuid().ToString();
            return element;
        }

        #endregion
    }

    public class Tag : Element, IStateSimulation
    {
        #region Construtor

        public Tag()
            : base()
        {
            this.properties = new Dictionary<string, Property>();
        }

        #endregion

        #region Properties

        private IDictionary<string, Property> properties = null;
        public IDictionary<string, Property> Properties
        {
            get { return properties; }
            set
            {
                if (value != properties)
                {
                    properties = value;
                    Notify("Properties");
                }
            }
        }

        #endregion

        #region IStateSimulation

        public ISimulation simulation;
        public ISimulation Simulation
        {
            get { return simulation; }
            set
            {
                if (value != simulation)
                {
                    simulation = value;

                    Notify("Simulation");
                }
            }
        }

        #endregion

        #region Clone

        public override object Clone()
        {
            Tag tag = (Tag)this.MemberwiseClone();
            return tag;
        }

        #endregion
    }

    #endregion

    #region Model.Elements.Gates

    public class AndGate : Element, IStateSimulation
    {
        #region Construtor

        public AndGate() : base() { }

        #endregion

        #region IStateSimulation

        public ISimulation simulation;
        public ISimulation Simulation
        {
            get { return simulation; }
            set
            {
                if (value != simulation)
                {
                    simulation = value;

                    Notify("Simulation");
                }
            }
        }

        #endregion

        #region Clone

        public override object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }

    public class OrGate : Element, IStateSimulation
    {
        #region Construtor

        public OrGate() : base() { }

        #endregion

        #region IStateSimulation

        public ISimulation simulation;
        public ISimulation Simulation
        {
            get { return simulation; }
            set
            {
                if (value != simulation)
                {
                    simulation = value;

                    Notify("Simulation");
                }
            }
        }

        #endregion

        #region Clone

        public override object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }

    public class NotGate : Element, IStateSimulation
    {
        #region Construtor

        public NotGate() : base() { }

        #endregion

        #region IStateSimulation

        public ISimulation simulation;
        public ISimulation Simulation
        {
            get { return simulation; }
            set
            {
                if (value != simulation)
                {
                    simulation = value;

                    Notify("Simulation");
                }
            }
        }

        #endregion

        #region Clone

        public override object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }

    public class BufferGate : Element, IStateSimulation
    {
        #region Construtor

        public BufferGate() : base() { }

        #endregion

        #region IStateSimulation

        public ISimulation simulation;
        public ISimulation Simulation
        {
            get { return simulation; }
            set
            {
                if (value != simulation)
                {
                    simulation = value;

                    Notify("Simulation");
                }
            }
        }

        #endregion

        #region Clone

        public override object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }

    public class NandGate : Element, IStateSimulation
    {
        #region Construtor

        public NandGate() : base() { }

        #endregion

        #region IStateSimulation

        public ISimulation simulation;
        public ISimulation Simulation
        {
            get { return simulation; }
            set
            {
                if (value != simulation)
                {
                    simulation = value;

                    Notify("Simulation");
                }
            }
        }

        #endregion

        #region Clone

        public override object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }

    public class NorGate : Element, IStateSimulation
    {
        #region Construtor

        public NorGate() : base() { }

        #endregion

        #region IStateSimulation

        public ISimulation simulation;
        public ISimulation Simulation
        {
            get { return simulation; }
            set
            {
                if (value != simulation)
                {
                    simulation = value;

                    Notify("Simulation");
                }
            }
        }

        #endregion

        #region Clone

        public override object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }

    public class XorGate : Element, IStateSimulation
    {
        #region Construtor

        public XorGate() : base() { }

        #endregion

        #region IStateSimulation

        public ISimulation simulation;
        public ISimulation Simulation
        {
            get { return simulation; }
            set
            {
                if (value != simulation)
                {
                    simulation = value;

                    Notify("Simulation");
                }
            }
        }

        #endregion

        #region Clone

        public override object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }

    public class XnorGate : Element, IStateSimulation
    {
        #region Construtor

        public XnorGate() : base() { }

        #endregion

        #region IStateSimulation

        public ISimulation simulation;
        public ISimulation Simulation
        {
            get { return simulation; }
            set
            {
                if (value != simulation)
                {
                    simulation = value;

                    Notify("Simulation");
                }
            }
        }

        #endregion

        #region Clone

        public override object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }

    #endregion

    #region Model.Elements.Memory

    public class MemoryResetPriority : Element, IStateSimulation
    {
        #region Construtor

        public MemoryResetPriority() : base() { }

        #endregion

        #region IStateSimulation

        public ISimulation simulation;
        public ISimulation Simulation
        {
            get { return simulation; }
            set
            {
                if (value != simulation)
                {
                    simulation = value;

                    Notify("Simulation");
                }
            }
        }

        #endregion

        #region Clone

        public override object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }

    public class MemorySetPriority : Element, IStateSimulation
    {
        #region Construtor

        public MemorySetPriority() : base() { }

        #endregion

        #region IStateSimulation

        public ISimulation simulation;
        public ISimulation Simulation
        {
            get { return simulation; }
            set
            {
                if (value != simulation)
                {
                    simulation = value;

                    Notify("Simulation");
                }
            }
        }

        #endregion

        #region Clone

        public override object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }

    #endregion

    #region Model.Elements.Timers

    public class TimerOn : Element, ITimer, IStateSimulation
    {
        #region Construtor

        public TimerOn() : base() { }

        #endregion

        #region ITimer Implementation

        private float delay = 1.0F;
        private string unit = "s";

        public float Delay
        {
            get { return delay; }
            set
            {
                if (value != delay)
                {
                    delay = value;
                    Notify("Delay");
                }
            }
        }

        public string Unit
        {
            get { return unit; }
            set
            {
                if (value != unit)
                {
                    unit = value;
                    Notify("Unit");
                }
            }
        }

        #endregion

        #region IStateSimulation

        public ISimulation simulation;
        public ISimulation Simulation
        {
            get { return simulation; }
            set
            {
                if (value != simulation)
                {
                    simulation = value;

                    Notify("Simulation");
                }
            }
        }

        #endregion

        #region Properties

        private LabelPosition labelPosition = LabelPosition.Top;
        public LabelPosition LabelPosition
        {
            get { return labelPosition; }
            set
            {
                if (value != labelPosition)
                {
                    labelPosition = value;
                    Notify("LabelPosition");
                }
            }
        }

        #endregion

        #region Clone

        public override object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }

    public class TimerOff : Element, ITimer, IStateSimulation
    {
        #region Construtor

        public TimerOff() : base() { }

        #endregion

        #region ITimer Implementation

        private float delay = 1.0F;
        private string unit = "s";

        public float Delay
        {
            get { return delay; }
            set
            {
                if (value != delay)
                {
                    delay = value;
                    Notify("Delay");
                }
            }
        }

        public string Unit
        {
            get { return unit; }
            set
            {
                if (value != unit)
                {
                    unit = value;
                    Notify("Unit");
                }
            }
        }

        #endregion

        #region IStateSimulation

        public ISimulation simulation;
        public ISimulation Simulation
        {
            get { return simulation; }
            set
            {
                if (value != simulation)
                {
                    simulation = value;

                    Notify("Simulation");
                }
            }
        }

        #endregion

        #region Properties

        private LabelPosition labelPosition = LabelPosition.Top;
        public LabelPosition LabelPosition
        {
            get { return labelPosition; }
            set
            {
                if (value != labelPosition)
                {
                    labelPosition = value;
                    Notify("LabelPosition");
                }
            }
        }

        #endregion

        #region Clone

        public override object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }

    public class TimerPulse : Element, ITimer, IStateSimulation
    {
        #region Construtor

        public TimerPulse() : base() { }

        #endregion

        #region ITimer Implementation

        private float delay = 1.0F;
        private string unit = "s";

        public float Delay
        {
            get { return delay; }
            set
            {
                if (value != delay)
                {
                    delay = value;
                    Notify("Delay");
                }
            }
        }

        public string Unit
        {
            get { return unit; }
            set
            {
                if (value != unit)
                {
                    unit = value;
                    Notify("Unit");
                }
            }
        }

        #endregion

        #region IStateSimulation

        public ISimulation simulation;
        public ISimulation Simulation
        {
            get { return simulation; }
            set
            {
                if (value != simulation)
                {
                    simulation = value;

                    Notify("Simulation");
                }
            }
        }

        #endregion

        #region Properties

        private LabelPosition labelPosition = LabelPosition.Top;
        public LabelPosition LabelPosition
        {
            get { return labelPosition; }
            set
            {
                if (value != labelPosition)
                {
                    labelPosition = value;
                    Notify("LabelPosition");
                }
            }
        }

        #endregion

        #region Clone

        public override object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }

    #endregion

    #region Model.Solution

    public class Context : Element
    {
        #region Construtor

        public Context() : base() { }

        #endregion

        #region Properties

        private int number;
        private PageType pageType = PageType.Logic;
        private ObservableCollection<Element> selectedElements = null;

        public int Number
        {
            get { return number; }
            set
            {
                if (value != number)
                {
                    number = value;
                    Notify("Number");
                }
            }
        }

        public PageType PageType
        {
            get { return pageType; }
            set
            {
                if (value != pageType)
                {
                    pageType = value;
                    Notify("PageType");
                }
            }
        }

        public ObservableCollection<Element> SelectedElements
        {
            get { return selectedElements; }
            set
            {
                if (value != selectedElements)
                {
                    selectedElements = value;
                    Notify("SelectedElements");
                }
            }
        }

        #endregion

        #region Clone

        public override object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }

    public class Project : Element
    {
        #region Construtor

        public Project() : base() { }

        #endregion

        #region Properties

        private Title title = new Title();

        public Title Title
        {
            get { return title; }
            set
            {
                if (value != title)
                {
                    title = value;
                    Notify("Title");
                }
            }
        }

        #endregion

        #region Clone

        public override object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }

    public class Solution : Element
    {
        #region Construtor

        public Solution() : base() { }

        #endregion

        #region Properties

        private ObservableCollection<Tag> tags = new ObservableCollection<Tag>();
        private Tag defaultTag = null;

        public ObservableCollection<Tag> Tags
        {
            get { return tags; }
            set
            {
                if (value != tags)
                {
                    tags = value;
                    Notify("Tags");
                }
            }
        }

        public Tag DefaultTag
        {
            get { return defaultTag; }
            set
            {
                if (value != defaultTag)
                {
                    defaultTag = value;
                    Notify("DefaultTag");
                }
            }
        }

        #endregion

        #region Clone

        public override object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }

    public class Title : NotifyObject
    {
        #region Construtor

        public Title() : base() { }

        #endregion

        #region Properties

        private string documentId = string.Empty;
        private string documentTitle = string.Empty;
        private string documentNumber = string.Empty;

        public string DocumentId
        {
            get { return documentId; }
            set
            {
                if (value != documentId)
                {
                    documentId = value;
                    Notify("DocumentId");
                }
            }
        }

        public string DocumentTitle
        {
            get { return documentTitle; }
            set
            {
                if (value != documentTitle)
                {
                    documentTitle = value;
                    Notify("DocumentTitle");
                }
            }
        }

        public string DocumentNumber
        {
            get { return documentNumber; }
            set
            {
                if (value != documentNumber)
                {
                    documentNumber = value;
                    Notify("DocumentNumber");
                }
            }
        }

        #endregion
    }

    public class Options : NotifyObject
    {
        #region Construtor

        public Options() : base() { }

        #endregion

        #region Properties

        private bool hidePins = false;
        private bool hideHelperLines = true;

        private bool shortenLineStarts = true;
        private bool shortenLineEnds = false;

        private double x = 0.0;
        private double y = 0.0;
        private double zoom = 1.0;
        private double zoomSpeed = 3.5;
        private double thickness = 1.0;
        private double snap = 15.0;
        private double offsetX = 0.0;
        private double offsetY = 5.0;

        private bool isSnapEnabled = true;
        private bool isAutoFitEnabled = true;

        private bool captureMouse = false;

        private bool isPrinting = false;

        private bool enableCharts = false;

        private int simulationPeriod = 100;

        private bool simulationIsRunning = false;

        private string currentElement = "Signal";

        private bool showSolutionExplorer = true;
        private bool showToolbox = true;

        private bool enableColors = true;
        private bool enableRecent = true;
        private bool enableUndoRedo = true;

        private bool disablePrintColors = false;

        private PageType defaultPageType = PageType.Logic;

        public bool HidePins
        {
            get { return hidePins; }
            set
            {
                if (value != hidePins)
                {
                    hidePins = value;
                    Notify("HidePins");
                }
            }
        }

        public bool HideHelperLines
        {
            get { return hideHelperLines; }
            set
            {
                if (value != hideHelperLines)
                {
                    hideHelperLines = value;
                    Notify("HideHelperLines");
                }
            }
        }

        public bool ShortenLineStarts
        {
            get { return shortenLineStarts; }
            set
            {
                if (value != shortenLineStarts)
                {
                    shortenLineStarts = value;
                    Notify("ShortenLineStarts");
                }
            }
        }

        public bool ShortenLineEnds
        {
            get { return shortenLineEnds; }
            set
            {
                if (value != shortenLineEnds)
                {
                    shortenLineEnds = value;
                    Notify("ShortenLineEnds");
                }
            }
        }

        public double X
        {
            get { return x; }
            set
            {
                if (value != x)
                {
                    x = value;
                    Notify("X");
                }
            }
        }

        public double Y
        {
            get { return y; }
            set
            {
                if (value != y)
                {
                    y = value;
                    Notify("Y");
                }
            }
        }

        public double Zoom
        {
            get { return zoom; }
            set
            {
                if (value != zoom)
                {
                    zoom = value;
                    Notify("Zoom");
                    Notify("Thickness");
                }
            }
        }

        public double ZoomSpeed
        {
            get { return zoomSpeed; }
            set
            {
                if (value != zoomSpeed)
                {
                    zoomSpeed = value;
                    Notify("ZoomSpeed");
                }
            }
        }

        public double Thickness
        {
            get { return thickness / zoom; }
            set
            {
                if (value != thickness)
                {
                    thickness = value / zoom;
                    Notify("Thickness");
                }
            }
        }

        public double Snap
        {
            get { return snap; }
            set
            {
                if (value != snap)
                {
                    snap = value;
                    Notify("Snap");
                }
            }
        }

        public double OffsetX
        {
            get { return offsetX; }
            set
            {
                if (value != offsetX)
                {
                    offsetX = value;
                    Notify("OffsetX");
                }
            }
        }

        public double OffsetY
        {
            get { return offsetY; }
            set
            {
                if (value != offsetY)
                {
                    offsetY = value;
                    Notify("OffsetY");
                }
            }
        }

        public bool IsSnapEnabled
        {
            get { return isSnapEnabled; }
            set
            {
                if (value != isSnapEnabled)
                {
                    isSnapEnabled = value;
                    Notify("IsSnapEnabled");
                }
            }
        }

        public bool IsAutoFitEnabled
        {
            get { return isAutoFitEnabled; }
            set
            {
                if (value != isAutoFitEnabled)
                {
                    isAutoFitEnabled = value;
                    Notify("IsAutoFitEnabled");
                }
            }
        }

        public bool EnableCharts
        {
            get { return enableCharts; }
            set
            {
                if (value != enableCharts)
                {
                    enableCharts = value;
                    Notify("EnableCharts");
                }
            }
        }

        public int SimulationPeriod
        {
            get { return simulationPeriod; }
            set
            {
                if (value != simulationPeriod)
                {
                    simulationPeriod = value;
                    Notify("SimulationPeriod");
                }
            }
        }

        public bool CaptureMouse
        {
            get { return captureMouse; }
            set
            {
                if (value != captureMouse)
                {
                    captureMouse = value;
                    Notify("CaptureMouse");
                }
            }
        }

        public volatile bool Sync;

        public bool IsPrinting
        {
            get { return isPrinting; }
            set
            {
                if (value != isPrinting)
                {
                    isPrinting = value;
                    Notify("IsPrinting");
                }
            }
        }

        public bool SimulationIsRunning
        {
            get { return simulationIsRunning; }
            set
            {
                if (value != simulationIsRunning)
                {
                    simulationIsRunning = value;
                    Notify("SimulationIsRunning");
                }
            }
        }

        public string CurrentElement
        {
            get { return currentElement; }
            set
            {
                if (value != currentElement)
                {
                    currentElement = value;
                    Notify("CurrentElement");
                }
            }
        }

        public bool ShowSolutionExplorer
        {
            get { return showSolutionExplorer; }
            set
            {
                if (value != showSolutionExplorer)
                {
                    showSolutionExplorer = value;
                    Notify("ShowSolutionExplorer");
                }
            }
        }

        public bool ShowToolbox
        {
            get { return showToolbox; }
            set
            {
                if (value != showToolbox)
                {
                    showToolbox = value;
                    Notify("ShowToolbox");
                }
            }
        }

        public bool EnableColors
        {
            get { return enableColors; }
            set
            {
                if (value != enableColors)
                {
                    enableColors = value;
                    Notify("EnableColors");
                }
            }
        }

        public bool EnableRecent
        {
            get { return enableRecent; }
            set
            {
                if (value != enableRecent)
                {
                    enableRecent = value;
                    Notify("EnableRecent");
                }
            }
        }

        public bool EnableUndoRedo
        {
            get { return enableUndoRedo; }
            set
            {
                if (value != enableUndoRedo)
                {
                    enableUndoRedo = value;
                    Notify("EnableUndoRedo");
                }
            }
        }

        public bool DisablePrintColors
        {
            get { return disablePrintColors; }
            set
            {
                if (value != disablePrintColors)
                {
                    disablePrintColors = value;
                    Notify("DisablePrintColors");
                }
            }
        }

        public PageType DefaultPageType
        {
            get { return defaultPageType; }
            set
            {
                if (value != defaultPageType)
                {
                    defaultPageType = value;
                    Notify("DefaultPageType");
                }
            }
        }

        #endregion
    }

    public static class Defaults
    {
        #region Constants

        public const string DataFormat = "LogicObjectType";

        public const string OptionsFileName = "options.json";
        public const string RecentFileName = "recent.json";
        public const string ColorsFileName = "colors.json";

        public const string DefaultElement = "Signal";

        public const double NewPinZIndex = 1.0;
        public const double PinZIndex = 2.0;
        public const double LineZIndex = 1.0;
        public const double SignalZIndex = 1.0;
        public const double ElementZIndex = 1.0;

        #endregion

        #region Properties

        private static Options options = new Options();

        public static Options Options
        {
            get { return options; }
            set
            {
                if (value != options)
                {
                    options = value;
                }
            }
        }

        #endregion

        #region Methods

        public static void SetDefaults(Options options)
        {
            options.HidePins = false;
            options.HideHelperLines = true;

            options.ShortenLineStarts = true;
            options.ShortenLineEnds = false;

            options.X = 0.0;
            options.Y = 0.0;

            options.Zoom = 1.0;
            options.ZoomSpeed = 3.5;
            options.Thickness = 1.0;
            options.Snap = 15.0;
            options.OffsetX = 0.0;
            options.OffsetY = 5.0;

            options.IsSnapEnabled = true;
            options.IsAutoFitEnabled = true;

            options.CaptureMouse = false;
            options.Sync = false;
            options.IsPrinting = false;

            options.EnableCharts = false;

            options.SimulationPeriod = 100;

            options.CurrentElement = DefaultElement;

            options.ShowSolutionExplorer = true;
            options.ShowToolbox = true;

            options.EnableColors = true;
            options.EnableRecent = true;
            options.EnableUndoRedo = true;

            options.DisablePrintColors = false;

            options.DefaultPageType = PageType.Logic;
        }

        #endregion
    }

    #endregion

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
                    System.Diagnostics.Debug.Print("No Valid Input for Id: {0} | State: {1}", Element.ElementId, State.State);
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
                    System.Diagnostics.Debug.Print("Pin: {0} | Inverted: {1} | SimulationParent: {2} | Type: {3}",
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
                    System.Diagnostics.Debug.Print("Id: {0} | State: {1}", Element.ElementId, State.State);
                    System.Diagnostics.Debug.Print("");
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
                    System.Diagnostics.Debug.Print("No Valid Input for Id: {0} | State: {1}", Element.ElementId, State.State);
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
                    System.Diagnostics.Debug.Print("Pin: {0} | Inverted: {1} | SimulationParent: {2} | Type: {3}",
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
                    System.Diagnostics.Debug.Print("Id: {0} | State: {1}", Element.ElementId, State.State);
                    System.Diagnostics.Debug.Print("");
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
                    System.Diagnostics.Debug.Print("No Valid Input for Id: {0} | State: {1}", Element.ElementId, State.State);
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
                    System.Diagnostics.Debug.Print("Pin: {0} | Inverted: {1} | SimulationParent: {2} | Type: {3}",
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
                    System.Diagnostics.Debug.Print("Id: {0} | State: {1}", Element.ElementId, State.State);
                    System.Diagnostics.Debug.Print("");
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

    public class TagSimulation : NotifyObject, ISimulation
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

        public IBoolState state;

        public IBoolState State
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
                    System.Diagnostics.Debug.Print("No Valid Input/Connections for Id: {0} | State: {1}", Element.ElementId, State.State);
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
                    System.Diagnostics.Debug.Print("Pin: {0} | Inverted: {1} | SimulationParent: {2} | Type: {3}",
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
                    System.Diagnostics.Debug.Print("Id: {0} | State: {1}", Element.ElementId, State.State);
                    System.Diagnostics.Debug.Print("");
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
                    System.Diagnostics.Debug.Print("Pin: {0} | Inverted: {1} | SimulationParent: {2} | Type: {3}",
                    connection.Item1.ElementId,
                    connection.Item2,
                    (connection.Item1.SimulationParent != null) ? connection.Item1.SimulationParent.ElementId : UInt32.MaxValue,
                    connection.Item1.Type);
                }
            }

            // get all connected inputs with state, where Tuple<IState,bool> is IState and Inverted
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
                    System.Diagnostics.Debug.Print("Id: {0} | State: {1}", Element.ElementId, State.State);
                    System.Diagnostics.Debug.Print("");
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
                    System.Diagnostics.Debug.Print("Pin: {0} | Inverted: {1} | SimulationParent: {2} | Type: {3}",
                    connection.Item1.ElementId,
                    connection.Item2,
                    (connection.Item1.SimulationParent != null) ? connection.Item1.SimulationParent.ElementId : UInt32.MaxValue,
                    connection.Item1.Type);
                }
            }

            // get all connected inputs with state, where Tuple<IState,bool> is IState and Inverted
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
                    System.Diagnostics.Debug.Print("Id: {0} | State: {1}", Element.ElementId, State.State);
                    System.Diagnostics.Debug.Print("");
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

    public class BoolState : NotifyObject, IBoolState
    {
        #region IState

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

        #endregion
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
                    System.Diagnostics.Debug.Print("{0}    Pin: {1} | Inverted: {2} | SimulationParent: {3} | Type: {4}",
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
                System.Diagnostics.Debug.Print("");
                System.Diagnostics.Debug.Print("--- FindConnections(), elements.Count: {0}", elements.Count());
                System.Diagnostics.Debug.Print("");
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
                    System.Diagnostics.Debug.Print("Pin  {0} | SimulationParent: {1} | Type: {2}",
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
                System.Diagnostics.Debug.Print("");
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
                            System.Diagnostics.Debug.Print("{0}{1} -> {2}", level, connection.Item1.ElementId, connection.Item1.Type);
                        }
                    }
                    else
                    {
                        if (SimulationSettings.EnableDebug)
                        {
                            System.Diagnostics.Debug.Print("{0}(*) {1} -> {2}", level, connection.Item1.ElementId, connection.Item1.Type);
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
                        System.Diagnostics.Debug.Print("{0}{1} -> {2}", level, pin.ElementId, pin.Type);
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
                    System.Diagnostics.Debug.Print("+ {0} -> {1}", connection.ElementId, connection.Type);
                }

                ProcessInput(connection, "  ");

                if (SimulationSettings.EnableDebug)
                {
                    System.Diagnostics.Debug.Print("");
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
                    System.Diagnostics.Debug.Print("--- warning: no ISimulation elements ---");
                    System.Diagnostics.Debug.Print("");
                }

                return;
            }

            var lenght = simulations.Count;

            for (int i = 0; i < lenght; i++)
            {
                if (SimulationSettings.EnableDebug)
                {
                    System.Diagnostics.Debug.Print("--- compilation: {0} | Type: {1} ---", simulations[i].Element.ElementId, simulations[i].GetType());
                    System.Diagnostics.Debug.Print("");
                }

                simulations[i].Compile();

                simulations[i].Clock = clock;

                if (SimulationSettings.EnableDebug)
                {
                    System.Diagnostics.Debug.Print("");
                }
            }
        }

        private static void Calculate(ISimulation[] simulations)
        {
            if (SimulationSettings.EnableDebug)
            {
                System.Diagnostics.Debug.Print("");
            }

            if (simulations == null)
            {
                if (SimulationSettings.EnableDebug)
                {
                    System.Diagnostics.Debug.Print("--- warning: no ISimulation elements ---");
                    System.Diagnostics.Debug.Print("");
                }

                return;
            }

            var lenght = simulations.Length;

            if (SimulationSettings.EnableDebug)
            {
                System.Diagnostics.Debug.Print("");
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 0; i < lenght; i++)
            {
                if (SimulationSettings.EnableDebug)
                {
                    System.Diagnostics.Debug.Print("--- simulation: {0} | Type: {1} ---", simulations[i].Element.ElementId, simulations[i].GetType());
                    System.Diagnostics.Debug.Print("");
                }

                simulations[i].Calculate();
            }

            sw.Stop();

            if (SimulationSettings.EnableDebug)
            {
                System.Diagnostics.Debug.Print("Calculate() done in: {0}ms | {1} elements", sw.Elapsed.TotalMilliseconds, lenght);
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
                System.Diagnostics.Debug.Print("--- elements with input connected ---");
                System.Diagnostics.Debug.Print("");
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
                System.Diagnostics.Debug.Print("-- dependencies ---");
                System.Diagnostics.Debug.Print("");
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
                System.Diagnostics.Debug.Print("-- sorted dependencies ---");
                System.Diagnostics.Debug.Print("");

                foreach (var simulation in sortedSimulations)
                {
                    System.Diagnostics.Debug.Print("{0}", simulation.Element.ElementId);
                }

                System.Diagnostics.Debug.Print("");
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

            System.Diagnostics.Debug.Print("Compile() done in: {0}ms", sw.Elapsed.TotalMilliseconds);

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
                    System.Diagnostics.Debug.Print("+ {0} depends on:", (item as ISimulation).Element.Name);
                }

                Visit(item, visited, sorted, dependencies);

                if (SimulationSettings.EnableDebug)
                {
                    System.Diagnostics.Debug.Print("");
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
                            System.Diagnostics.Debug.Print("|     {0}", (dep as ISimulation).Element.Name);
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
            //    System.Diagnostics.Debug.Print("Invalid dependency cycle: {0}", (item as Element).Name);
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

                System.Diagnostics.Debug.Print("Simulation for {0} contexts, elements: {1}", contexts.Count(), elements.Count());
                System.Diagnostics.Debug.Print("Debug Simulation Enabled: {0}", SimulationSettings.EnableDebug);
                System.Diagnostics.Debug.Print("Have Cache: {0}", CurrentSimulationContext.Cache == null ? false : CurrentSimulationContext.Cache.HaveCache);
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
                            System.Diagnostics.Debug.Print("Cycle {0} | {1}ms | vt:{2} rt:{3} dt:{4} id:{5}",
                                SimulationClock.Cycle,
                                sw.Elapsed.TotalMilliseconds,
                                virtualTime.TotalMilliseconds,
                                realTime.Elapsed.TotalMilliseconds,
                                DateTime.Now - dt,
                                System.Threading.Thread.CurrentThread.ManagedThreadId);
                        }
                        */

                        System.Diagnostics.Debug.Print("Cycle {0} | {1}ms | vt:{2} rt:{3} dt:{4} id:{5}",
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

            System.Diagnostics.Debug.Print("{1}: {0}", message, logPath);
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
            System.Diagnostics.Debug.Print("Done {0}.", message);
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
