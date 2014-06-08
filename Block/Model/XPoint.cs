using Sheet.Block.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Block.Model
{
    public class XPoint : XElement, IPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public bool IsVisible { get; set; }
        public IList<IDependency> Connected { get; set; }
        public XPoint(object element, double x, double y, bool isVisible)
        {
            Native = element;
            X = x;
            Y = y;
            IsVisible = isVisible;
            Connected = new List<IDependency>();
        }
    }
}
