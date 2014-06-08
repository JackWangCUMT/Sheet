using Sheet.Simulation.Core;
using Sheet.Simulation.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Simulation.Tests
{
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
            var solution = new Solution() { Id = Guid.NewGuid().ToString(), Name = "solution", DefaultTag = factory.CreateSignalTag("tag", "", "", "") };

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
}
