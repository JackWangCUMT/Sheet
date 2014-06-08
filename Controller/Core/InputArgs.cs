using Sheet.Block.Core;
using Sheet.Item.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Controller.Core
{
    public class InputArgs
    {
        // Mouse Generic
        public bool OnlyControl { get; set; }
        public bool OnlyShift { get; set; }
        public ItemType SourceType { get; set; }
        public ImmutablePoint SheetPosition { get; set; }
        public ImmutablePoint RootPosition { get; set; }
        public Action<bool> Handled { get; set; }
        // Mouse Wheel
        public int Delta { get; set; }
        // Mouse Down
        public InputButton Button { get; set; }
        public int Clicks { get; set; }
    }
}
