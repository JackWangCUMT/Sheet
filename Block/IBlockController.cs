using Sheet.Block.Core;
using Sheet.Block.Model;
using Sheet.Item.Model;
using Sheet.Controller.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Block
{
    public interface IBlockController
    {
        List<IPoint> Add(ISheet sheet, IEnumerable<PointItem> pointItems, IBlock parent, IBlock selected, bool select, double thickness);
        List<ILine> Add(ISheet sheet, IEnumerable<LineItem> lineItems, IBlock parent, IBlock selected, bool select, double thickness);
        List<IRectangle> Add(ISheet sheet, IEnumerable<RectangleItem> rectangleItems, IBlock parent, IBlock selected, bool select, double thickness);
        List<IEllipse> Add(ISheet sheet, IEnumerable<EllipseItem> ellipseItems, IBlock parent, IBlock selected, bool select, double thickness);
        List<IText> Add(ISheet sheet, IEnumerable<TextItem> textItems, IBlock parent, IBlock selected, bool select, double thickness);
        List<IImage> Add(ISheet sheet, IEnumerable<ImageItem> imageItems, IBlock parent, IBlock selected, bool select, double thickness);
        List<IBlock> Add(ISheet sheet, IEnumerable<BlockItem> blockItems, IBlock parent, IBlock selected, bool select, double thickness);
        void AddContents(ISheet sheet, BlockItem blockItem, IBlock content, IBlock selected, bool select, double thickness);
        void AddBroken(ISheet sheet, BlockItem blockItem, IBlock content, IBlock selected, bool select, double thickness);

        void Remove(ISheet sheet, IEnumerable<IPoint> points);
        void Remove(ISheet sheet, IEnumerable<ILine> lines);
        void Remove(ISheet sheet, IEnumerable<IRectangle> rectangles);
        void Remove(ISheet sheet, IEnumerable<IEllipse> ellipses);
        void Remove(ISheet sheet, IEnumerable<IText> texts);
        void Remove(ISheet sheet, IEnumerable<IImage> images);
        void Remove(ISheet sheet, IEnumerable<IBlock> blocks);
        void Remove(ISheet sheet, IBlock block);
        void RemoveSelected(ISheet sheet, IBlock parent, IBlock selected);

        void MoveDelta(double dx, double dy, IPoint point);
        void MoveDelta(double dx, double dy, IEnumerable<IPoint> points);
        void MoveDelta(double dx, double dy, IEnumerable<ILine> lines);
        void MoveDeltaStart(double dx, double dy, ILine line);
        void MoveDeltaEnd(double dx, double dy, ILine line);
        void MoveDelta(double dx, double dy, IRectangle rectangle);
        void MoveDelta(double dx, double dy, IEnumerable<IRectangle> rectangles);
        void MoveDelta(double dx, double dy, IEllipse ellipse);
        void MoveDelta(double dx, double dy, IEnumerable<IEllipse> ellipses);
        void MoveDelta(double dx, double dy, IText text);
        void MoveDelta(double dx, double dy, IEnumerable<IText> texts);
        void MoveDelta(double dx, double dy, IImage image);
        void MoveDelta(double dx, double dy, IEnumerable<IImage> images);
        void MoveDelta(double dx, double dy, IBlock block);
        void MoveDelta(double dx, double dy, IEnumerable<IBlock> blocks);

        void Deselect(IPoint point);
        void Deselect(ILine line);
        void Deselect(IRectangle rectangle);
        void Deselect(IEllipse ellipse);
        void Deselect(IText text);
        void Deselect(IImage image);
        void Deselect(IBlock parent);

        void Select(IPoint point);
        void Select(ILine line);
        void Select(IRectangle rectangle);
        void Select(IEllipse ellipse);
        void Select(IText text);
        void Select(IImage image);
        void Select(IBlock parent);

        bool HaveSelected(IBlock selected);
        bool HaveOnlyOnePointSelected(IBlock selected);
        bool HaveOnlyOneLineSelected(IBlock selected);
        bool HaveOnlyOneRectangleSelected(IBlock selected);
        bool HaveOnlyOneEllipseSelected(IBlock selected);
        bool HaveOnlyOneTextSelected(IBlock selected);
        bool HaveOnlyOneImageSelected(IBlock selected);
        bool HaveOnlyOneBlockSelected(IBlock selected);

        bool HitTest(IEnumerable<IPoint> points, ImmutableRect rect, object relativeTo, IBlock selected, bool findOnlyOne);
        bool HitTest(IEnumerable<ILine> lines, ImmutableRect rect, IBlock selected, bool findOnlyOne);
        bool HitTest(IEnumerable<IRectangle> rectangles, ImmutableRect rect, object relativeTo, IBlock selected, bool findOnlyOne);
        bool HitTest(IEnumerable<IEllipse> ellipses, ImmutableRect rect, object relativeTo, IBlock selected, bool findOnlyOne);
        bool HitTest(IEnumerable<IText> texts, ImmutableRect rect, object relativeTo, IBlock selected, bool findOnlyOne);
        bool HitTest(IEnumerable<IImage> images, ImmutableRect rect, object relativeTo, IBlock selected, bool findOnlyOne);
        bool HitTest(IEnumerable<IBlock> blocks, ImmutableRect rect, object relativeTo, IBlock selected, bool findOnlyOne, bool findInsideBlock);
        bool HitTest(IBlock block, ImmutableRect rect, object relativeTo, IBlock selected, bool findOnlyOne, bool findInsideBlock);
        bool HitTest(ISheet sheet, IBlock block, ImmutableRect rect, IBlock selected, bool findOnlyOne, bool findInsideBlock);
        bool HitTest(ISheet sheet, IBlock block, ImmutablePoint p, double size, IBlock selected, bool findOnlyOne, bool findInsideBlock);

        void ToggleFill(IRectangle rectangle);
        void ToggleFill(IEllipse ellipse);
        void ToggleFill(IPoint point);

        void ShallowCopy(IBlock original, IBlock copy);
    }
}
