using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Item.Model
{
    public class RectangleItem : Item
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool IsFilled { get; set; }
        public ItemColor Stroke { get; set; }
        public ItemColor Fill { get; set; }
        public double StrokeThickness { get; set; }
    }
}
