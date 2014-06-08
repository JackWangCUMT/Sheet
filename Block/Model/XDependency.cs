using Sheet.Block.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Block.Model
{
    public class XDependency : IDependency
    {
        public IElement Element { get; set; }
        public Action<IElement, IPoint> Update { get; set; }
        public XDependency(IElement element, Action<IElement, IPoint> update)
        {
            Element = element;
            Update = update;
        }
    }
}
