using Sheet.Simulation.Core;
using Sheet.Simulation.Elements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Simulation.Binary
{
    public class BinarySolutionWriter : IBinarySolutionWriter
    {
        #region Fields

        // dict: key = element Id, value = generated element id for simulation
        private Dictionary<string, UInt32> Ids;

        // generated element id
        private UInt32 ElementId;

        // command id
        private UInt16 CommandId;

        // bool true Byte value
        private const Byte TrueByte = 0x01;

        // bool false Byte value
        private const Byte FalseByte = 0x00;

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
            writer.Write(CommandId);
            writer.Write(ElementId);
            ElementId++;
        }

        private void WriteDummyProject(BinaryWriter writer)
        {
            CommandId = BinaryCommandId.Project;
            writer.Write(CommandId);
            writer.Write(ElementId);
            ElementId++;
        }

        private void WriteSolution(Solution solution, BinaryWriter writer)
        {
            CommandId = BinaryCommandId.Solution;
            writer.Write(CommandId);
            writer.Write(ElementId);
            Ids.Add(solution.Id, ElementId);
            ElementId++;
        }

        private void WriteProject(Project project, BinaryWriter writer)
        {
            CommandId = BinaryCommandId.Project;
            writer.Write(CommandId);
            writer.Write(ElementId);
            Ids.Add(project.Id, ElementId);
            ElementId++;
        }

        private void WriteContext(Context context, BinaryWriter writer)
        {
            CommandId = BinaryCommandId.Context;
            writer.Write(CommandId);
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
                writer.Write(CommandId);
                writer.Write(ElementId);
                Ids.Add(element.Id, ElementId);
                ElementId++;
            }
        }

        private void WriteSignal(Element element, BinaryWriter writer)
        {
            // signal id
            CommandId = BinaryCommandId.Signal;
            writer.Write(CommandId);
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
            writer.Write(CommandId);
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
            writer.Write(CommandId);
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
            writer.Write(CommandId);
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
            writer.Write(CommandId);
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
            writer.Write(CommandId);
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
            writer.Write(CommandId);
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
}
