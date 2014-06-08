using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Item.Model
{
    public class TextItem : Item
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public int HAlign { get; set; }
        public int VAlign { get; set; }
        public double Size { get; set; }
        public string Text { get; set; }
        public ItemColor Foreground { get; set; }
        public ItemColor Backgroud { get; set; }
    }
}
