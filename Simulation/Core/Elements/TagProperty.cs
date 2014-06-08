using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Simulation.Core
{
    public class TagProperty
    {
        public TagProperty() : base() { }
        public TagProperty(object data) : this() { Data = data; }
        public object Data { get; set; }
    }
}
