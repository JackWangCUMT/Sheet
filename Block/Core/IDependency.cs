using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Block.Core
{
    public interface IDependency
    {
        IElement Element { get; set; }
        Action<IElement, IPoint> Update { get; set; }
    }
}
