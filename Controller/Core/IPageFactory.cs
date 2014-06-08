using Sheet.Block.Core;
using Sheet.Item.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Controller.Core
{
    public interface IPageFactory
    {
        void CreateLine(ISheet sheet, IList<ILine> lines, double thickness, double x1, double y1, double x2, double y2, ItemColor stroke);
        void CreateText(ISheet sheet, IList<IText> texts, string content, double x, double y, double width, double height, int halign, int valign, double size, ItemColor foreground);
        void CreateFrame(ISheet sheet, IBlock block, double size, double thickness, ItemColor stroke);
        void CreateGrid(ISheet sheet, IBlock block, double startX, double startY, double width, double height, double size, double thickness, ItemColor stroke);

        IRectangle CreateSelectionRectangle(double thickness, double x, double y, double width, double height);
    }
}
