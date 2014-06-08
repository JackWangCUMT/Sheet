using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Block.Core
{
    public interface IPoint : IElement
    {
        double X { get; set; }
        double Y { get; set; }
        bool IsVisible { get; set; }
        IList<IDependency> Connected { get; set; }
    }
}
