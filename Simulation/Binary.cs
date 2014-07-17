using Sheet.Simulation.Core;
using Sheet.Simulation.Elements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Simulation.Binary
{
    /*
    command Id: sizeof(UInt16)
    command data: sizeof(command)
    ...
    elements counter: sizeof(UInt32)
    
    where:
    Id Command
    ---------------
    0    Solution
    1    Project
    2    Context
    3    Pin
    4    Signal
    5    AndGate
    6    OrGate
    7    TimerOn
    8    TimerOff
    9    TimerPulse
    10   Connect

    Solution command:
    UInt32 Id;

    Project command:
    UInt32 Id;

    Context command:
    UInt32 Id;

    Pin command:
    UInt32 Id;

    Signal command:
    UInt32 Id;
    UInt32 InputPinId;
    UInt32 OutputPinId;

    AndGate command:
    UInt32 Id;
    UInt32 LeftPinId;
    UInt32 RightPinId;
    UInt32 TopPinId;
    UInt32 BottomPinId;

    OrGate command:
    UInt32 Id;
    UInt32 LeftPinId;
    UInt32 RightPinId;
    UInt32 TopPinId;
    UInt32 BottomPinId;

    TimerOn command:
    UInt32 Id;
    UInt32 LeftPinId;
    UInt32 RightPinId;
    UInt32 TopPinId;
    UInt32 BottomPinId;
    Single Delay;

    TimerOff command:
    UInt32 Id;
    UInt32 LeftPinId;
    UInt32 RightPinId;
    UInt32 TopPinId;
    UInt32 BottomPinId;
    Single Delay;

    TimerPulse command:
    UInt32 Id;
    UInt32 LeftPinId;
    UInt32 RightPinId;
    UInt32 TopPinId;
    UInt32 BottomPinId;
    Single Delay;

    Connect command:
    UInt32 Id;
    UInt32 SrcPinId;
    UInt32 DstPinId;
    Byte InvertStart;
    Byte InvertEnd;
    */

    public static class BinaryCommandId
    {
        public const UInt16 Solution = 0;
        public const UInt16 Project = 1;
        public const UInt16 Context = 2;
        public const UInt16 Pin = 3;
        public const UInt16 Signal = 4;
        public const UInt16 AndGate = 5;
        public const UInt16 OrGate = 6;
        public const UInt16 TimerOn = 7;
        public const UInt16 TimerOff = 8;
        public const UInt16 TimerPulse = 9;
        public const UInt16 Connect = 10;
    }

    public interface IBinarySolutionReader
    {
        Solution Open(string path);
    }

    public interface IBinarySolutionWriter
    {
        void Save(string path, Solution solution);
    }

    public class BinarySolutionReader : IBinarySolutionReader
    {
        #region Fields

        public static int SizeOfCommandId = Marshal.SizeOf(typeof(UInt16));
        public static int SizeOfTotalElements = Marshal.SizeOf(typeof(UInt32));

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
                UInt16 commandId = 0;

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
                    commandId = reader.ReadUInt16();

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
    }

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
