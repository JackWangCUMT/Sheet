using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Item.Model
{
    public class BlockItem : Item
    {
        public double X { get; set; }
        public double Y { get; set; }
        public string Name { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public ItemColor Backgroud { get; set; }
        public int DataId { get; set; }
        public List<PointItem> Points { get; set; }
        public List<LineItem> Lines { get; set; }
        public List<RectangleItem> Rectangles { get; set; }
        public List<EllipseItem> Ellipses { get; set; }
        public List<TextItem> Texts { get; set; }
        public List<ImageItem> Images { get; set; }
        public List<BlockItem> Blocks { get; set; }
        public BlockItem(int id, double x, double y, double widht, double height, int dataId, string name)
        {
            X = x;
            Y = y;
            Id = id;
            DataId = dataId;
            Name = name;
            Width = widht;
            Height = height;
            Backgroud = new ItemColor() { Alpha = 0, Red = 0, Green = 0, Blue = 0 };
            Points = new List<PointItem>();
            Lines = new List<LineItem>();
            Rectangles = new List<RectangleItem>();
            Ellipses = new List<EllipseItem>();
            Texts = new List<TextItem>();
            Images = new List<ImageItem>();
            Blocks = new List<BlockItem>();
        }
    }
}
