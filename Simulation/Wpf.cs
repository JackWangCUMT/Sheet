using Sheet.Block;
using Sheet.Simulation.Core;
using Sheet.Simulation.Elements;
using Sheet.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Threading;

namespace Sheet.Simulation.Wpf
{
    public interface ISolutionSerializer
    {
        Solution Serialize(IBlock root);
    }

    public interface ISolutionSimulationController
    {
        void EnableDebug(bool enable);
        void EnableLog(bool enable);
        void Start();
        void Stop();
    }

    public interface ISolutionSimulationFactory
    {
        Clock Clock { get; }
        bool IsSimulationRunning { get; }
        SimulationController SimulationController { get; }
        Solution Solution { get; }

        void Start();
        void Stop();
    }

    public interface ISolutionRenamer
    {
        void Rename(Solution solution);
        void Rename(Project project);
        void Rename(Context context);
    }

    public interface ISolutionFactory
    {
        AndGate CreateAndGate(Context context, double x, double y, string name);
        BufferGate CreateBufferGate(Context context, double x, double y, string name);
        Element CreateElementFromType(Context context, string type, double x, double y);
        MemoryResetPriority CreateMemoryResetPriority(Context context, double x, double y, string name);
        MemorySetPriority CreateMemorySetPriority(Context context, double x, double y, string name);
        NandGate CreateNandGate(Context context, double x, double y, string name);
        NorGate CreateNorGate(Context context, double x, double y, string name);
        NotGate CreateNotGate(Context context, double x, double y, string name);
        OrGate CreateOrGate(Context context, double x, double y, string name);
        Pin CreatePin(Context context, double x, double y, Element parent, string name, string factoryName, PinType type, bool isPinTypeUndefined);
        Signal CreateSignal(Context context, double x, double y, string name);
        Tag CreateSignalTag(string designation, string description, string signal, string condition);
        TimerOff CreateTimerOff(Context context, double x, double y, string name);
        TimerOn CreateTimerOn(Context context, double x, double y, string name);
        TimerPulse CreateTimerPulse(Context context, double x, double y, string name);
        Wire CreateWire(Context context, Pin start, Pin end);
        XnorGate CreateXnorGate(Context context, double x, double y, string name);
        XorGate CreateXorGate(Context context, double x, double y, string name);
    }

    public interface IBlockSimulationHelper
    {
        void SeState(ILine line, IBoolState state);
        void SeState(IRectangle rectangle, IBoolState state);
        void SeState(IEllipse ellipse, IBoolState state);
        void SeState(IText text, IBoolState state);
        void SeState(IImage image, IBoolState state);
        void SeState(IBlock parent, IBoolState state);
    }

    public class TestSerializer : ISolutionSerializer
    {
        #region Fields

        private TestFactory _factory = new TestFactory();
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

        private Tag CreateSignalTag(string designation, string description, string signal, string condition)
        {
            var tag = new Tag() { Id = Guid.NewGuid().ToString() };
            tag.Properties.Add("Designation", new TagProperty(designation));
            tag.Properties.Add("Description", new TagProperty(description));
            tag.Properties.Add("Signal", new TagProperty(signal));
            tag.Properties.Add("Condition", new TagProperty(condition));
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

    public class TestSolutionSimulationController : ISolutionSimulationController
    {
        #region Fields

        private Solution _solution;
        private TestSolutionSimulationFactory _simulation;

        #endregion

        #region Constructor

        public TestSolutionSimulationController(Solution solution, int period)
        {
            _solution = solution;
            _simulation = new TestSolutionSimulationFactory(_solution, period);
        }

        #endregion

        #region Simulation Settings

        public void EnableDebug(bool enable)
        {
            SimulationSettings.EnableDebug = enable;
        }

        public void EnableLog(bool enable)
        {
            SimulationSettings.EnableLog = enable;
        }

        #endregion

        #region Simulation

        public void Start()
        {
            _simulation.Start();
        }

        public void Stop()
        {
            _simulation.Stop();
        }

        #endregion

        #region Demo Solution

        public static Solution CreateDemoSolution()
        {
            var factory = new TestFactory();

            // create solution
            var solution = new Solution() { Id = Guid.NewGuid().ToString(), Name = "solution" };

            var project = new Project() { Id = Guid.NewGuid().ToString(), Name = "project", Parent = solution };
            solution.Children.Add(project);

            var context = new Context() { Id = Guid.NewGuid().ToString(), Name = "context", Parent = project };
            project.Children.Add(context);

            // create tags
            var tag1 = factory.CreateSignalTag("tag1", "", "", "");
            var tag2 = factory.CreateSignalTag("tag2", "", "", "");
            var tag3 = factory.CreateSignalTag("tag3", "", "", "");
            solution.Tags.Add(tag1);
            solution.Tags.Add(tag2);
            solution.Tags.Add(tag3);

            // context children
            var s1 = factory.CreateSignal(context, 0, 0);
            var s2 = factory.CreateSignal(context, 0, 0);
            var s3 = factory.CreateSignal(context, 0, 0);
            var ag1 = factory.CreateAndGate(context, 0, 0);
            var p1 = factory.CreatePin(context, 0, 0, context);

            var o_s1 = s1.Children.Single((e) => e.FactoryName == "O") as Pin;
            var o_s2 = s2.Children.Single((e) => e.FactoryName == "O") as Pin;
            var t_ag1 = ag1.Children.Single((e) => e.FactoryName == "T") as Pin;
            var l_ag1 = ag1.Children.Single((e) => e.FactoryName == "L") as Pin;
            var r_ag1 = ag1.Children.Single((e) => e.FactoryName == "R") as Pin;
            var i_s3 = s1.Children.Single((e) => e.FactoryName == "I") as Pin;

            var w1 = factory.CreateWire(context, o_s1, p1);
            var w2 = factory.CreateWire(context, p1, t_ag1);
            var w3 = factory.CreateWire(context, o_s2, l_ag1);
            var w4 = factory.CreateWire(context, r_ag1, i_s3);

            // associate tags
            s1.Tag = tag1;
            s2.Tag = tag2;
            s3.Tag = tag3;

            return solution;
        }

        #endregion
    }

    public class TestSolutionSimulationFactory : ISolutionSimulationFactory
    {
        #region Fields

        private Solution _solution;
        private Clock _clock;
        private SimulationController _factory;
        private int _periodInMillisencods;
        private bool _isSimulationRunning = false;

        #endregion

        #region Properties

        public Solution Solution { get { return _solution; } }
        public Clock Clock { get { return _clock; } }
        public SimulationController SimulationController { get { return _factory; } }
        public bool IsSimulationRunning { get { return _isSimulationRunning; } }

        #endregion

        #region Constructor

        public TestSolutionSimulationFactory(Solution solution, int periodInMillisencods = 100)
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

        #endregion

        #region Update

        private void UpdateSignalBlockStates(Signal[] signals, IBlockSimulationHelper helper)
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
            var helper = new WpfBlockSimulationHelper();

            var update = new Action(() => UpdateSignalBlockStates(signals, helper));
            var updateOnUIThread = new Action(() => dispatcher.BeginInvoke(update));

            return updateOnUIThread;
        }

        #endregion

        #region Simulation

        private void Initialize()
        {
            _factory = new SimulationController();
            _clock = new Clock();

            // create simulation context
            _factory.SimulationContext = new SimulationContext()
            {
                Cache = null,
                SimulationClock = _clock
            };

            _factory.IsConsole = false;

            // disable Console debug output
            SimulationSettings.EnableDebug = false;

            // disable Log for Run()
            SimulationSettings.EnableLog = false;
        }

        public void Start()
        {
            // simulation is already running
            if (_factory.SimulationContext.SimulationTimer != null)
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
            _factory.Reset(true);

            // run simulation
            _factory.Run(contexts, _solution.Tags, period, GetUpdateAction(contexts));
            _isSimulationRunning = true;

            // reset simulation parent
            foreach (var element in elements)
            {
                element.SimulationParent = null;
            }
        }

        public void Stop()
        {
            if (_factory.SimulationContext.SimulationTimer != null)
            {
                // stop simulation
                _factory.Stop();

                _isSimulationRunning = false;

                // reset previous simulation cache
                _factory.Reset(true);

                ResetTags();
            }
        }

        #endregion
    }

    public class TestSolutionRenamer : ISolutionRenamer
    {
        #region Fields

        public const string CountersNamepsace = "Sheet.Simulation.Elements";

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

        #endregion

        #region Counters

        private Dictionary<string, int> GetCountersDictionary()
        {
            // element counters based on type
            var types = System.Reflection.Assembly.GetAssembly(typeof(Solution))
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

        #endregion

        #region Rename

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

        private void Rename(IEnumerable<Context> contexts, Dictionary<string, int> counters, Dictionary<string, string> ids)
        {
            foreach (var context in contexts)
            {
                Rename(context, counters, ids);
            }
        }

        private void Rename(Context context, Dictionary<string, int> counters, Dictionary<string, string> ids)
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

        #endregion
    }

    public class TestFactory : ISolutionFactory
    {
        #region Create

        public Tag CreateSignalTag(string designation, string description, string signal, string condition)
        {
            var tag = new Tag() { Id = Guid.NewGuid().ToString() };
            tag.Properties.Add("Designation", new TagProperty(designation));
            tag.Properties.Add("Description", new TagProperty(description));
            tag.Properties.Add("Signal", new TagProperty(signal));
            tag.Properties.Add("Condition", new TagProperty(condition));
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

        #endregion
    }
}