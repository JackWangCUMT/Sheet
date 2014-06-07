using Sheet.Simulation.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Simulation.Tests
{
    public class TestFactory
    {
        #region Create

        public Tag CreateSignalTag(string designation, string description, string signal, string condition)
        {
            var tag = new Tag() { Id = Guid.NewGuid().ToString() };
            tag.Properties.Add("Designation", new Property(designation));
            tag.Properties.Add("Description", new Property(description));
            tag.Properties.Add("Signal", new Property(signal));
            tag.Properties.Add("Condition", new Property(condition));
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
