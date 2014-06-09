using Sheet.Block.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Block.Model
{
    public class XBlock : XElement, IBlock
    {
        public double X { get; set; }
        public double Y { get; set; }
        public string Name { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public int DataId { get; set; }
        public IArgbColor Backgroud { get; set; }
        public IList<IPoint> Points { get; set; }
        public IList<ILine> Lines { get; set; }
        public IList<IRectangle> Rectangles { get; set; }
        public IList<IEllipse> Ellipses { get; set; }
        public IList<IText> Texts { get; set; }
        public IList<IImage> Images { get; set; }
        public IList<IBlock> Blocks { get; set; }
        public XBlock()
        {
            Backgroud = new XArgbColor(0, 0, 0, 0);
            Points = new List<IPoint>();
            Lines = new List<ILine>();
            Rectangles = new List<IRectangle>();
            Ellipses = new List<IEllipse>();
            Texts = new List<IText>();
            Images = new List<IImage>();
            Blocks = new List<IBlock>();
        }
        public XBlock(int id, double x, double y, double width, double height, int dataId, string name) : this()
        {
            Id = id;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            DataId = dataId;
            Name = name;
        }
    }
}
