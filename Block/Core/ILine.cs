using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Block.Core
{
    public interface ILine : IElement
    {
        int StartId { get; set; }
        int EndId { get; set; }
        IPoint Start { get; set; }
        IPoint End { get; set; }
    }
}
