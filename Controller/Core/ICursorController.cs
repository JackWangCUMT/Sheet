using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Controller.Core
{
    public interface ICursorController
    {
        void Set(SheetCursor cursor);
        SheetCursor Get();
    }
}
