using Sheet.Block.Core;
using Sheet.Item.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Block
{
    public interface IBlockSerializer
    {
        PointItem Serialize(IPoint point);
        LineItem Serialize(ILine line);
        RectangleItem Serialize(IRectangle rectangle);
        EllipseItem Serialize(IEllipse ellipse);
        TextItem Serialize(IText text);
        ImageItem Serialize(IImage image);
        BlockItem Serialize(IBlock parent);
        BlockItem SerializerContents(IBlock parent, int id, double x, double y, double width, double height, int dataId, string name);
        IPoint Deserialize(ISheet sheet, IBlock parent, PointItem pointItem, double thickness);
        ILine Deserialize(ISheet sheet, IBlock parent, LineItem lineItem, double thickness);
        IRectangle Deserialize(ISheet sheet, IBlock parent, RectangleItem rectangleItem, double thickness);
        IEllipse Deserialize(ISheet sheet, IBlock parent, EllipseItem ellipseItem, double thickness);
        IText Deserialize(ISheet sheet, IBlock parent, TextItem textItem);
        IImage Deserialize(ISheet sheet, IBlock parent, ImageItem imageItem);
        IBlock Deserialize(ISheet sheet, IBlock parent, BlockItem blockItem, double thickness);
    }
}
