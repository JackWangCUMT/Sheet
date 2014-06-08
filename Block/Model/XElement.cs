using Sheet.Block.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Block.Model
{
    public abstract class XElement : IElement
    {
        public int Id { get; set; }
        public object Native { get; set; }
    }
}
