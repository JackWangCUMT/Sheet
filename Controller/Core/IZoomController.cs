using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Controller.Core
{
    public interface IZoomController
    {
        int ZoomIndex { get; set; }
        double Zoom { get; set; }
        double PanX { get; set; }
        double PanY { get; set; }
        void AutoFit();
        void ActualSize();
    }
}
