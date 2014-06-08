using Sheet.Block.Core;
using Sheet.Item.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Block
{
    public interface IBlockFactory
    {
        IThumb CreateThumb(double x, double y);
        IThumb CreateThumb(double x, double y, ILine line, Action<ILine, IThumb, double, double> drag);
        IThumb CreateThumb(double x, double y, IElement element, Action<IElement, IThumb, double, double> drag);
        IPoint CreatePoint(double thickness, double x, double y, bool isVisible);
        ILine CreateLine(double thickness, double x1, double y1, double x2, double y2, ItemColor stroke);
        ILine CreateLine(double thickness, IPoint start, IPoint end, ItemColor stroke);
        IRectangle CreateRectangle(double thickness, double x, double y, double width, double height, bool isFilled, ItemColor stroke, ItemColor fill);
        IEllipse CreateEllipse(double thickness, double x, double y, double width, double height, bool isFilled, ItemColor stroke, ItemColor fill);
        IText CreateText(string text, double x, double y, double width, double height, int halign, int valign, double fontSize, ItemColor backgroud, ItemColor foreground);
        IImage CreateImage(double x, double y, double width, double height, byte[] data);
        IBlock CreateBlock(int id, double x, double y, double width, double height, int dataId, string name, ItemColor backgroud);
    }
}
