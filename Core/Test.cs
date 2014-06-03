using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet
{
    public class TestHistoryController: IHistoryController
    {
        public void Register(string message)
        {
        }

        public void Reset()
        {
        }

        public void Undo()
        {
        }

        public void Redo()
        {
        }
    }

    public class TestLibraryController : ILibraryController
    {
        public BlockItem GetSelected()
        {
            return null;
        }

        public void SetSelected(BlockItem block)
        {
        }

        public IEnumerable<BlockItem> GetSource()
        {
            return null;
        }

        public void SetSource(IEnumerable<BlockItem> source)
        {
        }
    }

    public class TestZoomController : IZoomController
    {
        public int ZoomIndex { get; set; }
        public double Zoom { get; set; }
        public double PanX { get; set; }
        public double PanY { get; set; }
        public void AutoFit() { }
        public void ActualSize() { }

        public TestZoomController()
        {
            ZoomIndex = 9;
            Zoom = 1.0;
            PanX = 0.0;
            PanY = 0.0;
        }
    }

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
