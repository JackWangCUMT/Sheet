using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Item.Model
{
    public class LineItem : Item
    {
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
        public ItemColor Stroke { get; set; }
        public double StrokeThickness { get; set; }
        public int StartId { get; set; }
        public int EndId { get; set; }
    }
}
