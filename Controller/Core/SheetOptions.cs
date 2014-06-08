using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Controller.Core
{
    public class SheetOptions
    {
        public double PageOriginX { get; set; }
        public double PageOriginY { get; set; }
        public double PageWidth { get; set; }
        public double PageHeight { get; set; }
        public double SnapSize { get; set; }
        public double GridSize { get; set; }
        public double FrameThickness { get; set; }
        public double GridThickness { get; set; }
        public double SelectionThickness { get; set; }
        public double LineThickness { get; set; }
        public double HitTestSize { get; set; }
        public int DefaultZoomIndex { get; set; }
        public int MaxZoomIndex { get; set; }
        public double[] ZoomFactors { get; set; }
        public double Zoom { get; set; }
        public double PanX { get; set; }
        public double PanY { get; set; }
    }
}
