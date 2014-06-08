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

        void SelectContent(IBlock selected, IBlock content);
        void DeselectContent(IBlock selected);

        bool HaveSelected(IBlock selected);
        bool HaveOnePointSelected(IBlock selected);
        bool HaveOneLineSelected(IBlock selected);
        bool HaveOneRectangleSelected(IBlock selected);
        bool HaveOneEllipseSelected(IBlock selected);
        bool HaveOneTextSelected(IBlock selected);
        bool HaveOneImageSelected(IBlock selected);
        bool HaveOneBlockSelected(IBlock selected);

        bool HitTest(IEnumerable<IPoint> points, IBlock selected, XImmutableRect rect, bool onlyFirst, bool select, object relativeTo);
        bool HitTest(IEnumerable<ILine> lines, IBlock selected, XImmutableRect rect, bool onlyFirst, bool select);
        bool HitTest(IEnumerable<IRectangle> rectangles, IBlock selected, XImmutableRect rect, bool onlyFirst, bool select, object relativeTo);
        bool HitTest(IEnumerable<IEllipse> ellipses, IBlock selected, XImmutableRect rect, bool onlyFirst, bool select, object relativeTo);
        bool HitTest(IEnumerable<IText> texts, IBlock selected, XImmutableRect rect, bool onlyFirst, bool select, object relativeTo);
        bool HitTest(IEnumerable<IImage> images, IBlock selected, XImmutableRect rect, bool onlyFirst, bool select, object relativeTo);
        bool HitTest(IEnumerable<IBlock> blocks, IBlock selected, XImmutableRect rect, bool onlyFirst, bool select, bool selectInsideBlock, object relativeTo);
        bool HitTest(IBlock parent, IBlock selected, XImmutableRect rect, bool onlyFirst, bool selectInsideBlock, object relativeTo);

        bool HitTestClick(ISheet sheet, IBlock parent, IBlock selected, XImmutablePoint p, double size, bool selectInsideBlock, bool resetSelected);
        bool HitTestForBlocks(ISheet sheet, IBlock parent, IBlock selected, XImmutablePoint p, double size);
        void HitTestSelectionRect(ISheet sheet, IBlock parent, IBlock selected, XImmutableRect rect, bool resetSelected);

        void ToggleFill(IRectangle rectangle);
        void ToggleFill(IEllipse ellipse);
        void ToggleFill(IPoint point);

        IBlock ShallowCopy(IBlock original);
    }
}
