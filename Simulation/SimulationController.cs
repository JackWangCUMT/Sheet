using Sheet.Simulation.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Simulation
{
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
