using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Test
{
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
}
