using Sheet.Simulation.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Simulation.Tests
{
    public class TestSolutionSimulationFactory : ISolutionSimulationFactory
    {
        #region Fields

        private Solution _solution;
        private Clock _clock;
        private SimulationFactory _factory;
        private int _periodInMillisencods;
        private bool _isSimulationRunning = false;

        #endregion

        #region Properties

        public Solution Solution { get { return _solution; } }
        public Clock Clock { get { return _clock; } }
        public SimulationFactory SimulationFactory { get { return _factory; } }
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

        #region Simulation

        private void Initialize()
        {
            _factory = new SimulationFactory();
            _clock = new Clock();

            // create simulation context
            _factory.CurrentSimulationContext = new SimulationContext()
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
            if (_factory.CurrentSimulationContext.SimulationTimer != null)
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
            _factory.Run(contexts, _solution.Tags, period, () => { });
            _isSimulationRunning = true;

            // reset simulation parent
            foreach (var element in elements)
            {
                element.SimulationParent = null;
            }
        }

        public void Stop()
        {
            if (_factory.CurrentSimulationContext.SimulationTimer != null)
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
}
