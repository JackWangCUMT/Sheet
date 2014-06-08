using Sheet.Simulation.Core;
using Sheet.Simulation.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Simulation.Tests
{
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
}
