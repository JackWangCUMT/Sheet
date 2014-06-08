using Sheet.Block.Core;
using Sheet.Block.Model;
using Sheet.Item.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Block
{
    public interface IBlockHelper
    {
        bool HitTest(IElement element, XImmutableRect rect);
        bool HitTest(IElement element, XImmutableRect rect, object relativeTo);

        void SetIsSelected(IElement element, bool value);
        bool GetIsSelected(IElement element);

        bool IsSelected(IPoint point);
        bool IsSelected(ILine line);
        bool IsSelected(IRectangle rectangle);
        bool IsSelected(IEllipse ellipse);
        bool IsSelected(IText text);
        bool IsSelected(IImage image);

        void Deselect(IPoint point);
        void Deselect(ILine line);
        void Deselect(IRectangle rectangle);
        void Deselect(IEllipse ellipse);
        void Deselect(IText text);
        void Deselect(IImage image);

        void Select(IPoint point);
        void Select(ILine line);
        void Select(IRectangle rectangle);
        void Select(IEllipse ellipse);
        void Select(IText text);
        void Select(IImage image);

        void SetZIndex(IElement element, int index);

        void ToggleFill(IRectangle rectangle);
        void ToggleFill(IEllipse ellipse);
        void ToggleFill(IPoint point);

        double GetLeft(IElement element);
        double GetTop(IElement element);
        double GetWidth(IElement element);
        double GetHeight(IElement element);
        void SetLeft(IElement element, double left);
        void SetTop(IElement element, double top);
        void SetWidth(IElement element, double width);
        void SetHeight(IElement element, double height);

        double GetX1(ILine line);
        double GetY1(ILine line);
        double GetX2(ILine line);
        double GetY2(ILine line);
        ItemColor GetStroke(ILine line);
        void SetX1(ILine line, double x1);
        void SetY1(ILine line, double y1);
        void SetX2(ILine line, double x2);
        void SetY2(ILine line, double y2);
        void SetStrokeThickness(ILine line, double thickness);
        double GetStrokeThickness(ILine line);

        ItemColor GetStroke(IRectangle rectangle);
        ItemColor GetFill(IRectangle rectangle);
        bool IsTransparent(IRectangle rectangle);
        void SetStrokeThickness(IRectangle rectangle, double thickness);
        double GetStrokeThickness(IRectangle rectangle);

        ItemColor GetStroke(IEllipse ellipse);
        ItemColor GetFill(IEllipse ellipse);
        bool IsTransparent(IEllipse ellipse);
        void SetStrokeThickness(IEllipse ellipse, double thickness);
        double GetStrokeThickness(IEllipse ellipse);

        ItemColor GetBackground(IText text);
        ItemColor GetForeground(IText text);

        string GetText(IText text);
        int GetHAlign(IText text);
        int GetVAlign(IText text);
        double GetSize(IText text);

        byte[] GetData(IImage image);
    }
}
