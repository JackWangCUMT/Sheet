using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Block.Model
{
    public struct XImmutableRect
    {
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }
        public XImmutableRect(double x, double y, double width, double height)
            : this()
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}
