using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Block.Core
{
    public interface IElement
    {
        int Id { get; set; }
        object Native { get; set; }
    }
}
