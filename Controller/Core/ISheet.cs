using Sheet.Block.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Controller.Core
{
    public interface ISheet
    {
        double Width { get; set; }
        double Height { get; set; }
        bool IsCaptured { get; }
        object GetParent();
        void SetParent(object parent);
        void Add(IElement element);
        void Remove(IElement element);
        void Add(object element);
        void Remove(object element);
        void Capture();
        void ReleaseCapture();
    }
}
