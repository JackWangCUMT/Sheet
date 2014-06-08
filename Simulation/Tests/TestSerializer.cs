using Sheet.Block.Core;
using Sheet.Simulation.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Simulation.Tests
{
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
            if (parent.Points != null)
            {
                foreach (var point in parent.Points)
                {
                    point.Id = nextId++;
                }
            }

            if (parent.Lines != null)
            {
                foreach (var line in parent.Lines)
                {
                    line.Id = nextId++;
                }
            }

            if (parent.Blocks != null)
            {
                foreach (var block in parent.Blocks)
                {
                    block.Id = nextId++;
                }
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
                Parent = context
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
                Parent = context
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
                Parent = context
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
                Delay = 1.0f
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
                Delay = 1.0f
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
                Delay = 1.0f
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
                throw new Exception("Unsupported block Name");
            }
        }

        private void SerializerContents(Context context, IBlock root)
        {
            SetId(root, 1);

            if (root.Blocks != null)
            {
                foreach (var block in root.Blocks)
                {
                    Serialize(context, block);
                }
            }

            if (root.Points != null)
            {
                foreach (var point in root.Points)
                {
                    Serialize(context, point);
                }
            }

            if (root.Lines != null)
            {
                foreach (var line in root.Lines)
                {
                    Serialize(context, line);
                }
            }
        }

        public Solution Serialize(IBlock root)
        {
            var solution = new Solution() { Id = Guid.NewGuid().ToString(), Name = "solution", DefaultTag = null };

            var project = new Project() { Id = Guid.NewGuid().ToString(), Name = "project", Parent = solution };
            solution.Children.Add(project);

            var context = new Context() { Id = Guid.NewGuid().ToString(), Name = "context", Parent = project };
            project.Children.Add(context);

            map = new Dictionary<int, Pin>();
            tags = new ObservableCollection<Tag>();

            SerializerContents(context, root);

            solution.Tags = tags;

            return solution;
        }

        #endregion
    }
}
