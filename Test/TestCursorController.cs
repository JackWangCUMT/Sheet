using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Test
{
    public class TestCursorController : ICursorController
    {
        private SheetCursor _cursor = SheetCursor.Unknown;

        public void Set(SheetCursor cursor)
        {
            _cursor = cursor;
        }

        public SheetCursor Get()
        {
            return _cursor;
        }
    }
}
