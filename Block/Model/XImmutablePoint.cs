using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Block.Model
{
    public struct XImmutablePoint
    {
        public double X { get; private set; }
        public double Y { get; private set; }
        public XImmutablePoint(double x, double y)
            : this()
        {
            X = x;
            Y = y;
        }
    }
}
