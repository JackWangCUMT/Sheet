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
        public XBlock(int id, double x, double y, double width, double height, int dataId, string name)
        {
            Id = id;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            DataId = dataId;
            Name = name;
        }
        public void Init()
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
        public void ReInit()
        {
            if (Points == null)
            {
                Points = new List<IPoint>();
            }

            if (Lines == null)
            {
                Lines = new List<ILine>();
            }

            if (Rectangles == null)
            {
                Rectangles = new List<IRectangle>();
            }

            if (Ellipses == null)
            {
                Ellipses = new List<IEllipse>();
            }

            if (Texts == null)
            {
                Texts = new List<IText>();
            }

            if (Images == null)
            {
                Images = new List<IImage>();
            }

            if (Blocks == null)
            {
                Blocks = new List<IBlock>();
            }
        }
    }
}
