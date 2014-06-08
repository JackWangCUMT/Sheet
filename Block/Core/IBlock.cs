using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Block.Core
{
    public interface IBlock : IElement
    {
        double X { get; set; }
        double Y { get; set; }
        string Name { get; set; }
        double Width { get; set; }
        double Height { get; set; }
        int DataId { get; set; }
        IArgbColor Backgroud { get; set; }
        IList<IPoint> Points { get; set; }
        IList<ILine> Lines { get; set; }
        IList<IRectangle> Rectangles { get; set; }
        IList<IEllipse> Ellipses { get; set; }
        IList<IText> Texts { get; set; }
        IList<IImage> Images { get; set; }
        IList<IBlock> Blocks { get; set; }
        void Init();
        void ReInit();
    }
}
