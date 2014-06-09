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
    public class BlockController : IBlockController
    {
        #region IoC

        private readonly IServiceLocator _serviceLocator;
        private readonly IBlockHelper _blockHelper;
        private readonly IBlockSerializer _blockSerializer;
        private readonly IBlockFactory _blockFactory;
        private readonly IPointController _pointController;

        public BlockController(IServiceLocator serviceLocator)
        {
            this._serviceLocator = serviceLocator;
            this._blockHelper = serviceLocator.GetInstance<IBlockHelper>();
            this._blockSerializer = serviceLocator.GetInstance<IBlockSerializer>();
            this._blockFactory = serviceLocator.GetInstance<IBlockFactory>();
            this._pointController = serviceLocator.GetInstance<IPointController>();
        }

        #endregion

        #region Add

        public List<IPoint> Add(ISheet sheet, IEnumerable<PointItem> pointItems, IBlock parent, IBlock selected, bool select, double thickness)
        {
            var points = new List<IPoint>();

            if (select)
            {
                selected.Points = new List<IPoint>();
            }

            foreach (var pointItem in pointItems)
            {
                var point = _blockSerializer.Deserialize(sheet, parent, pointItem, thickness);

                points.Add(point);

                if (select)
                {
                    Select(point);
                    selected.Points.Add(point);
                }
            }

            return points;
        }

        public List<ILine> Add(ISheet sheet, IEnumerable<LineItem> lineItems, IBlock parent, IBlock selected, bool select, double thickness)
        {
            var lines = new List<ILine>();

            if (select)
            {
                selected.Lines = new List<ILine>();
            }

            foreach (var lineItem in lineItems)
            {
                var line = _blockSerializer.Deserialize(sheet, parent, lineItem, thickness);

                lines.Add(line);

                if (select)
                {
                    Select(line);
                    selected.Lines.Add(line);
                }
            }

            return lines;
        }

        public List<IRectangle> Add(ISheet sheet, IEnumerable<RectangleItem> rectangleItems, IBlock parent, IBlock selected, bool select, double thickness)
        {
            var rectangles = new List<IRectangle>();

            if (select)
            {
                selected.Rectangles = new List<IRectangle>();
            }

            foreach (var rectangleItem in rectangleItems)
            {
                var rectangle = _blockSerializer.Deserialize(sheet, parent, rectangleItem, thickness);

                rectangles.Add(rectangle);

                if (select)
                {
                    Select(rectangle);
                    selected.Rectangles.Add(rectangle);
                }
            }

            return rectangles;
        }

        public List<IEllipse> Add(ISheet sheet, IEnumerable<EllipseItem> ellipseItems, IBlock parent, IBlock selected, bool select, double thickness)
        {
            var ellipses = new List<IEllipse>();

            if (select)
            {
                selected.Ellipses = new List<IEllipse>();
            }

            foreach (var ellipseItem in ellipseItems)
            {
                var ellipse = _blockSerializer.Deserialize(sheet, parent, ellipseItem, thickness);

                ellipses.Add(ellipse);

                if (select)
                {
                    Select(ellipse);
                    selected.Ellipses.Add(ellipse);
                }
            }

            return ellipses;
        }

        public List<IText> Add(ISheet sheet, IEnumerable<TextItem> textItems, IBlock parent, IBlock selected, bool select, double thickness)
        {
            var texts = new List<IText>();

            if (select)
            {
                selected.Texts = new List<IText>();
            }

            foreach (var textItem in textItems)
            {
                var text = _blockSerializer.Deserialize(sheet, parent, textItem);

                texts.Add(text);

                if (select)
                {
                    Select(text);
                    selected.Texts.Add(text);
                }
            }

            return texts;
        }

        public List<IImage> Add(ISheet sheet, IEnumerable<ImageItem> imageItems, IBlock parent, IBlock selected, bool select, double thickness)
        {
            var images = new List<IImage>();

            if (select)
            {
                selected.Images = new List<IImage>();
            }

            foreach (var imageItem in imageItems)
            {
                var image = _blockSerializer.Deserialize(sheet, parent, imageItem);

                images.Add(image);

                if (select)
                {
                    Select(image);
                    selected.Images.Add(image);
                }
            }

            return images;
        }

        public List<IBlock> Add(ISheet sheet, IEnumerable<BlockItem> blockItems, IBlock parent, IBlock selected, bool select, double thickness)
        {
            var blocks = new List<IBlock>();

            if (select)
            {
                selected.Blocks = new List<IBlock>();
            }

            foreach (var blockItem in blockItems)
            {
                var block = _blockSerializer.Deserialize(sheet, parent, blockItem, thickness);

                blocks.Add(block);

                if (select)
                {
                    Select(block);

                    selected.Blocks.Add(block);
                }
            }

            return blocks;
        }

        public void AddContents(ISheet sheet, BlockItem blockItem, IBlock content, IBlock selected, bool select, double thickness)
        {
            if (blockItem != null)
            {
                var texts = Add(sheet, blockItem.Texts, content, selected, select, thickness);
                var images = Add(sheet, blockItem.Images, content, selected, select, thickness);
                var lines = Add(sheet, blockItem.Lines, content, selected, select, thickness);
                var rectangles = Add(sheet, blockItem.Rectangles, content, selected, select, thickness);
                var ellipses = Add(sheet, blockItem.Ellipses, content, selected, select, thickness);
                var blocks = Add(sheet, blockItem.Blocks, content, selected, select, thickness);
                var points = Add(sheet, blockItem.Points, content, selected, select, thickness);

                _pointController.UpdateDependencies(blocks, points, lines);
            }
        }

        public void AddBroken(ISheet sheet, BlockItem blockItem, IBlock content, IBlock selected, bool select, double thickness)
        {
            Add(sheet, blockItem.Texts, content, selected, select, thickness);
            Add(sheet, blockItem.Images, content, selected, select, thickness);
            Add(sheet, blockItem.Lines, content, selected, select, thickness);
            Add(sheet, blockItem.Rectangles, content, selected, select, thickness);
            Add(sheet, blockItem.Ellipses, content, selected, select, thickness);

            foreach (var block in blockItem.Blocks)
            {
                Add(sheet, block.Texts, content, selected, select, thickness);
                Add(sheet, block.Images, content, selected, select, thickness);
                Add(sheet, block.Lines, content, selected, select, thickness);
                Add(sheet, block.Rectangles, content, selected, select, thickness);
                Add(sheet, block.Ellipses, content, selected, select, thickness);
                Add(sheet, block.Blocks, content, selected, select, thickness);
                Add(sheet, block.Points, content, selected, select, thickness);
            }

            Add(sheet, blockItem.Points, content, selected, select, thickness);
        }

        #endregion

        #region Remove

        public void Remove(ISheet sheet, IEnumerable<IPoint> points)
        {
            if (points != null)
            {
                foreach (var point in points)
                {
                    sheet.Remove(point);
                }
            }
        }

        public void Remove(ISheet sheet, IEnumerable<ILine> lines)
        {
            if (lines != null)
            {
                foreach (var line in lines)
                {
                    sheet.Remove(line);
                }
            }
        }

        public void Remove(ISheet sheet, IEnumerable<IRectangle> rectangles)
        {
            if (rectangles != null)
            {
                foreach (var rectangle in rectangles)
                {
                    sheet.Remove(rectangle);
                }
            }
        }

        public void Remove(ISheet sheet, IEnumerable<IEllipse> ellipses)
        {
            if (ellipses != null)
            {
                foreach (var ellipse in ellipses)
                {
                    sheet.Remove(ellipse);
                }
            }
        }

        public void Remove(ISheet sheet, IEnumerable<IText> texts)
        {
            if (texts != null)
            {
                foreach (var text in texts)
                {
                    sheet.Remove(text);
                }
            }
        }

        public void Remove(ISheet sheet, IEnumerable<IImage> images)
        {
            if (images != null)
            {
                foreach (var image in images)
                {
                    sheet.Remove(image);
                }
            }
        }

        public void Remove(ISheet sheet, IEnumerable<IBlock> blocks)
        {
            if (blocks != null)
            {
                foreach (var block in blocks)
                {
                    Remove(sheet, block);
                }
            }
        }

        public void Remove(ISheet sheet, IBlock block)
        {
            Remove(sheet, block.Points);
            Remove(sheet, block.Lines);
            Remove(sheet, block.Rectangles);
            Remove(sheet, block.Ellipses);
            Remove(sheet, block.Texts);
            Remove(sheet, block.Images);
            Remove(sheet, block.Blocks);
        }

        public void RemoveSelected(ISheet sheet, IBlock parent, IBlock selected)
        {
            if (selected.Points != null)
            {
                Remove(sheet, selected.Points);

                foreach (var point in selected.Points)
                {
                    parent.Points.Remove(point);
                }

                selected.Points = null;
            }

            if (selected.Lines != null)
            {
                Remove(sheet, selected.Lines);

                foreach (var line in selected.Lines)
                {
                    parent.Lines.Remove(line);
                }

                selected.Lines = null;
            }

            if (selected.Rectangles != null)
            {
                Remove(sheet, selected.Rectangles);

                foreach (var rectangle in selected.Rectangles)
                {
                    parent.Rectangles.Remove(rectangle);
                }

                selected.Rectangles = null;
            }

            if (selected.Ellipses != null)
            {
                Remove(sheet, selected.Ellipses);

                foreach (var ellipse in selected.Ellipses)
                {
                    parent.Ellipses.Remove(ellipse);
                }

                selected.Ellipses = null;
            }

            if (selected.Texts != null)
            {
                Remove(sheet, selected.Texts);

                foreach (var text in selected.Texts)
                {
                    parent.Texts.Remove(text);
                }

                selected.Texts = null;
            }

            if (selected.Images != null)
            {
                Remove(sheet, selected.Images);

                foreach (var image in selected.Images)
                {
                    parent.Images.Remove(image);
                }

                selected.Images = null;
            }

            if (selected.Blocks != null)
            {
                foreach (var block in selected.Blocks)
                {
                    Remove(sheet, block.Points);
                    Remove(sheet, block.Lines);
                    Remove(sheet, block.Rectangles);
                    Remove(sheet, block.Ellipses);
                    Remove(sheet, block.Texts);
                    Remove(sheet, block.Images);
                    Remove(sheet, block.Blocks);

                    parent.Blocks.Remove(block);
                }

                selected.Blocks = null;
            }
        }

        #endregion

        #region Move

        public void MoveDelta(double dx, double dy, IPoint point)
        {
            if (point.Native != null)
            {
                point.X = _blockHelper.GetLeft(point) + dx;
                point.Y = _blockHelper.GetTop(point) + dy;

                _blockHelper.SetLeft(point, point.X);
                _blockHelper.SetTop(point, point.Y);
            }
            else
            {
                point.X += dx;
                point.Y += dy;
            }

            foreach (var dependency in point.Connected)
            {
                dependency.Update(dependency.Element, point);
            }
        }

        public void MoveDelta(double dx, double dy, IEnumerable<IPoint> points)
        {
            foreach (var point in points)
            {
                MoveDelta(dx, dy, point);
            }
        }

        public void MoveDelta(double dx, double dy, IEnumerable<ILine> lines)
        {
            foreach (var line in lines)
            {
                if (line.Start == null)
                {
                    MoveDeltaStart(dx, dy, line);
                }

                if (line.End == null)
                {
                    MoveDeltaEnd(dx, dy, line);
                }
            }
        }

        public void MoveDeltaStart(double dx, double dy, ILine line)
        {
            double oldx = _blockHelper.GetX1(line);
            double oldy = _blockHelper.GetY1(line);
            _blockHelper.SetX1(line, oldx + dx);
            _blockHelper.SetY1(line, oldy + dy);
        }

        public void MoveDeltaEnd(double dx, double dy, ILine line)
        {
            double oldx = _blockHelper.GetX2(line);
            double oldy = _blockHelper.GetY2(line);
            _blockHelper.SetX2(line, oldx + dx);
            _blockHelper.SetY2(line, oldy + dy);
        }

        public void MoveDelta(double dx, double dy, IRectangle rectangle)
        {
            double left = _blockHelper.GetLeft(rectangle) + dx;
            double top = _blockHelper.GetTop(rectangle) + dy;
            _blockHelper.SetLeft(rectangle, left);
            _blockHelper.SetTop(rectangle, top);
        }

        public void MoveDelta(double dx, double dy, IEnumerable<IRectangle> rectangles)
        {
            foreach (var rectangle in rectangles)
            {
                MoveDelta(dx, dy, rectangle);
            }
        }

        public void MoveDelta(double dx, double dy, IEllipse ellipse)
        {
            double left = _blockHelper.GetLeft(ellipse) + dx;
            double top = _blockHelper.GetTop(ellipse) + dy;
            _blockHelper.SetLeft(ellipse, left);
            _blockHelper.SetTop(ellipse, top);
        }

        public void MoveDelta(double dx, double dy, IEnumerable<IEllipse> ellipses)
        {
            foreach (var ellipse in ellipses)
            {
                MoveDelta(dx, dy, ellipse);
            }
        }

        public void MoveDelta(double dx, double dy, IText text)
        {
            double left = _blockHelper.GetLeft(text) + dx;
            double top = _blockHelper.GetTop(text) + dy;
            _blockHelper.SetLeft(text, left);
            _blockHelper.SetTop(text, top);
        }

        public void MoveDelta(double dx, double dy, IEnumerable<IText> texts)
        {
            foreach (var text in texts)
            {
                MoveDelta(dx, dy, text);
            }
        }

        public void MoveDelta(double dx, double dy, IImage image)
        {
            double left = _blockHelper.GetLeft(image) + dx;
            double top = _blockHelper.GetTop(image) + dy;
            _blockHelper.SetLeft(image, left);
            _blockHelper.SetTop(image, top);
        }

        public void MoveDelta(double dx, double dy, IEnumerable<IImage> images)
        {
            foreach (var image in images)
            {
                MoveDelta(dx, dy, image);
            }
        }

        public void MoveDelta(double dx, double dy, IBlock block)
        {
            if (block.Points != null)
            {
                MoveDelta(dx, dy, block.Points);
            }

            if (block.Lines != null)
            {
                MoveDelta(dx, dy, block.Lines);
            }

            if (block.Rectangles != null)
            {
                MoveDelta(dx, dy, block.Rectangles);
            }

            if (block.Ellipses != null)
            {
                MoveDelta(dx, dy, block.Ellipses);
            }

            if (block.Texts != null)
            {
                MoveDelta(dx, dy, block.Texts);
            }

            if (block.Images != null)
            {
                MoveDelta(dx, dy, block.Images);
            }

            if (block.Blocks != null)
            {
                MoveDelta(dx, dy, block.Blocks);
            }
        }

        public void MoveDelta(double dx, double dy, IEnumerable<IBlock> blocks)
        {
            foreach (var block in blocks)
            {
                MoveDelta(dx, dy, block.Points);
                MoveDelta(dx, dy, block.Lines);
                MoveDelta(dx, dy, block.Rectangles);
                MoveDelta(dx, dy, block.Ellipses);
                MoveDelta(dx, dy, block.Texts);
                MoveDelta(dx, dy, block.Images);
                MoveDelta(dx, dy, block.Blocks);
            }
        }

        #endregion

        #region Select

        private int DeselectedZIndex = 0;
        private int SelectedZIndex = 1;

        public void Deselect(IPoint point)
        {
            _blockHelper.SetIsSelected(point, false);
        }

        public void Deselect(ILine line)
        {
            _blockHelper.Deselect(line);
            _blockHelper.SetZIndex(line, DeselectedZIndex);
        }

        public void Deselect(IRectangle rectangle)
        {
            _blockHelper.Deselect(rectangle);
            _blockHelper.SetZIndex(rectangle, DeselectedZIndex);
        }

        public void Deselect(IEllipse ellipse)
        {
            _blockHelper.Deselect(ellipse);
            _blockHelper.SetZIndex(ellipse, DeselectedZIndex);
        }

        public void Deselect(IText text)
        {
            _blockHelper.Deselect(text);
            _blockHelper.SetZIndex(text, DeselectedZIndex);
        }

        public void Deselect(IImage image)
        {
            _blockHelper.Deselect(image);
            _blockHelper.SetZIndex(image, DeselectedZIndex);
        }

        public void Deselect(IBlock parent)
        {
            if (parent.Points != null)
            {
                foreach (var point in parent.Points)
                {
                    Deselect(point);
                }
            }

            if (parent.Lines != null)
            {
                foreach (var line in parent.Lines)
                {
                    Deselect(line);
                }
            }

            if (parent.Rectangles != null)
            {
                foreach (var rectangle in parent.Rectangles)
                {
                    Deselect(rectangle);
                }
            }

            if (parent.Ellipses != null)
            {
                foreach (var ellipse in parent.Ellipses)
                {
                    Deselect(ellipse);
                }
            }

            if (parent.Texts != null)
            {
                foreach (var text in parent.Texts)
                {
                    Deselect(text);
                }
            }

            if (parent.Images != null)
            {
                foreach (var image in parent.Images)
                {
                    Deselect(image);
                }
            }

            if (parent.Blocks != null)
            {
                foreach (var block in parent.Blocks)
                {
                    Deselect(block);
                }
            }
        }

        public void Select(IPoint point)
        {
            _blockHelper.SetIsSelected(point, true);
        }

        public void Select(ILine line)
        {
            _blockHelper.Select(line);
            _blockHelper.SetZIndex(line, SelectedZIndex);
        }

        public void Select(IRectangle rectangle)
        {
            _blockHelper.Select(rectangle);
            _blockHelper.SetZIndex(rectangle, SelectedZIndex);
        }

        public void Select(IEllipse ellipse)
        {
            _blockHelper.Select(ellipse);
            _blockHelper.SetZIndex(ellipse, SelectedZIndex);
        }

        public void Select(IText text)
        {
            _blockHelper.Select(text);
            _blockHelper.SetZIndex(text, SelectedZIndex);
        }

        public void Select(IImage image)
        {
            _blockHelper.Select(image);
            _blockHelper.SetZIndex(image, SelectedZIndex);
        }

        public void Select(IBlock parent)
        {
            foreach (var point in parent.Points)
            {
                Select(point);
            }

            foreach (var line in parent.Lines)
            {
                Select(line);
            }

            foreach (var rectangle in parent.Rectangles)
            {
                Select(rectangle);
            }

            foreach (var ellipse in parent.Ellipses)
            {
                Select(ellipse);
            }

            foreach (var text in parent.Texts)
            {
                Select(text);
            }

            foreach (var image in parent.Images)
            {
                Select(image);
            }

            foreach (var block in parent.Blocks)
            {
                Select(block);
            }
        }

        public void SelectAndAdd(IBlock content, IBlock selected)
        {
            foreach (var point in content.Points)
            {
                Select(point);
                selected.Points.Add(point);
            }

            foreach (var line in content.Lines)
            {
                Select(line);
                selected.Lines.Add(line);
            }

            foreach (var rectangle in content.Rectangles)
            {
                Select(rectangle);
                selected.Rectangles.Add(rectangle);
            }

            foreach (var ellipse in content.Ellipses)
            {
                Select(ellipse);
                selected.Ellipses.Add(ellipse);
            }

            foreach (var text in content.Texts)
            {
                Select(text);
                selected.Texts.Add(text);
            }

            foreach (var image in content.Images)
            {
                Select(image);
                selected.Images.Add(image);
            }

            foreach (var parent in content.Blocks)
            {
                foreach (var point in parent.Points)
                {
                    Select(point);
                }

                foreach (var line in parent.Lines)
                {
                    Select(line);
                }

                foreach (var rectangle in parent.Rectangles)
                {
                    Select(rectangle);
                }

                foreach (var ellipse in parent.Ellipses)
                {
                    Select(ellipse);
                }

                foreach (var text in parent.Texts)
                {
                    Select(text);
                }

                foreach (var image in parent.Images)
                {
                    Select(image);
                }

                foreach (var block in parent.Blocks)
                {
                    Select(block);
                }

                selected.Blocks.Add(parent);
            }
        }

        #endregion

        #region HaveSelected

        public bool HaveSelected(IBlock selected)
        {
            return (selected.Points.Count > 0
                || selected.Lines.Count > 0
                || selected.Rectangles.Count > 0
                || selected.Ellipses.Count > 0
                || selected.Texts.Count > 0
                || selected.Images.Count > 0
                || selected.Blocks.Count > 0);
        }

        public bool HaveOnePointSelected(IBlock selected)
        {
            return (selected.Points.Count == 1
                && selected.Lines.Count == 0
                && selected.Rectangles.Count == 0
                && selected.Ellipses.Count == 0
                && selected.Texts.Count == 0
                && selected.Images.Count == 0
                && selected.Blocks.Count == 0);
        }

        public bool HaveOneLineSelected(IBlock selected)
        {
            return (selected.Points.Count == 0
                && selected.Lines.Count == 1
                && selected.Rectangles.Count == 0
                && selected.Ellipses.Count == 0
                && selected.Texts.Count == 0
                && selected.Images.Count == 0
                && selected.Blocks.Count == 0);
        }

        public bool HaveOneRectangleSelected(IBlock selected)
        {
            return (selected.Points.Count == 0
                && selected.Lines.Count == 0
                && selected.Rectangles.Count == 1
                && selected.Ellipses.Count == 0
                && selected.Texts.Count == 0
                && selected.Images.Count == 0
                && selected.Blocks.Count == 0);
        }

        public bool HaveOneEllipseSelected(IBlock selected)
        {
            return (selected.Points.Count == 0
                && selected.Lines.Count == 0
                && selected.Rectangles.Count == 0
                && selected.Ellipses.Count == 1
                && selected.Texts.Count == 0
                && selected.Images.Count == 0
                && selected.Blocks.Count == 0);
        }

        public bool HaveOneTextSelected(IBlock selected)
        {
            return (selected.Points.Count == 0
                && selected.Lines.Count == 0
                && selected.Rectangles.Count == 0
                && selected.Ellipses.Count == 0
                && selected.Texts.Count == 1
                && selected.Images.Count == 0
                && selected.Blocks.Count == 0);
        }

        public bool HaveOneImageSelected(IBlock selected)
        {
            return (selected.Points.Count == 0
                && selected.Lines.Count == 0
                && selected.Rectangles.Count == 0
                && selected.Ellipses.Count == 0
                && selected.Texts.Count == 0
                && selected.Images.Count == 1
                && selected.Blocks.Count == 0);
        }

        public bool HaveOneBlockSelected(IBlock selected)
        {
            return (selected.Points.Count == 0
                && selected.Lines.Count == 0
                && selected.Rectangles.Count == 0
                && selected.Ellipses.Count == 0
                && selected.Texts.Count == 0
                && selected.Images.Count == 0
                && selected.Blocks.Count == 1);
        }

        #endregion

        #region HitTest

        public bool HitTest(IEnumerable<IPoint> points, ImmutableRect rect, object relativeTo, IBlock selected, bool findOnlyOne)
        {
            foreach (var point in points)
            {
                if (_blockHelper.HitTest(point, rect, relativeTo))
                {
                    if (selected != null)
                    {
                        selected.Points.Add(point);
                    }

                    if (findOnlyOne)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool HitTest(IEnumerable<ILine> lines, ImmutableRect rect, IBlock selected, bool findOnlyOne)
        {
            foreach (var line in lines)
            {
                if (_blockHelper.HitTest(line, rect))
                {
                    if (selected != null)
                    {
                        selected.Lines.Add(line);
                    }

                    if (findOnlyOne)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool HitTest(IEnumerable<IRectangle> rectangles, ImmutableRect rect, object relativeTo, IBlock selected, bool findOnlyOne)
        {
            foreach (var rectangle in rectangles)
            {
                if (_blockHelper.HitTest(rectangle, rect, relativeTo))
                {
                    if (selected != null)
                    {
                        selected.Rectangles.Add(rectangle);
                    }

                    if (findOnlyOne)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool HitTest(IEnumerable<IEllipse> ellipses, ImmutableRect rect, object relativeTo, IBlock selected, bool findOnlyOne)
        {
            foreach (var ellipse in ellipses)
            {
                if (_blockHelper.HitTest(ellipse, rect, relativeTo))
                {
                    if (selected != null)
                    {
                        selected.Ellipses.Add(ellipse);
                    }

                    if (findOnlyOne)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool HitTest(IEnumerable<IText> texts, ImmutableRect rect, object relativeTo, IBlock selected, bool findOnlyOne)
        {
            foreach (var text in texts)
            {
                if (_blockHelper.HitTest(text, rect, relativeTo))
                {
                    if (selected != null)
                    {
                        selected.Texts.Add(text);
                    }

                    if (findOnlyOne)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool HitTest(IEnumerable<IImage> images, ImmutableRect rect, object relativeTo, IBlock selected, bool findOnlyOne)
        {
            foreach (var image in images)
            {
                if (_blockHelper.HitTest(image, rect, relativeTo))
                {
                    if (selected != null)
                    {
                        selected.Images.Add(image);
                    }

                    if (findOnlyOne)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool HitTest(IEnumerable<IBlock> blocks, ImmutableRect rect, object relativeTo, IBlock selected, bool findOnlyOne)
        {
            foreach (var block in blocks)
            {
                if (HitTest(block, rect, relativeTo, null, true))
                {
                    if (selected != null)
                    {
                        selected.Blocks.Add(block);
                    }

                    if (findOnlyOne)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool HitTest(IBlock block, ImmutableRect rect, object relativeTo, IBlock selected, bool findOnlyOne)
        {
            if (HitTest(block.Points, rect, relativeTo, selected, findOnlyOne))
            {
                if (findOnlyOne)
                {
                    return true;
                }
            }

            if (HitTest(block.Texts, rect, relativeTo, selected, findOnlyOne))
            {
                if (findOnlyOne)
                {
                    return true;
                }
            }

            if (HitTest(block.Images, rect, relativeTo, selected, findOnlyOne))
            {
                if (findOnlyOne)
                {
                    return true;
                }
            }

            if (HitTest(block.Lines, rect, selected, findOnlyOne))
            {
                if (findOnlyOne)
                {
                    return true;
                }
            }

            if (HitTest(block.Rectangles, rect, relativeTo, selected, findOnlyOne))
            {
                if (findOnlyOne)
                {
                    return true;
                }
            }

            if (HitTest(block.Ellipses, rect, relativeTo, selected, findOnlyOne))
            {
                if (findOnlyOne)
                {
                    return true;
                }
            }

            if (HitTest(block.Blocks, rect, relativeTo, selected, findOnlyOne))
            {
                if (findOnlyOne)
                {
                    return true;
                }
            }

            return false;
        }

        public bool HitTest(ISheet sheet, IBlock block, ImmutableRect rect, IBlock selected, bool findOnlyOne)
        {
            if (HitTest(block.Points, rect, sheet.GetParent(), selected, findOnlyOne))
            {
                if (findOnlyOne)
                {
                    return true;
                }
            }

            if (HitTest(block.Texts, rect, sheet.GetParent(), selected, findOnlyOne))
            {
                if (findOnlyOne)
                {
                    return true;
                }
            }

            if (HitTest(block.Images, rect, sheet.GetParent(), selected, findOnlyOne))
            {
                if (findOnlyOne)
                {
                    return true;
                }
            }

            if (HitTest(block.Lines, rect, selected, findOnlyOne))
            {
                if (findOnlyOne)
                {
                    return true;
                }
            }

            if (HitTest(block.Rectangles, rect, sheet.GetParent(), selected, findOnlyOne))
            {
                if (findOnlyOne)
                {
                    return true;
                }
            }

            if (HitTest(block.Ellipses, rect, sheet.GetParent(), selected, findOnlyOne))
            {
                if (findOnlyOne)
                {
                    return true;
                }
            }

            if (HitTest(block.Blocks, rect, sheet.GetParent(), selected, findOnlyOne))
            {
                if (findOnlyOne)
                {
                    return true;
                }
            }

            return false;
        }

        public bool HitTest(ISheet sheet, IBlock block, ImmutablePoint p, double size, IBlock selected, bool findOnlyOne)
        {
            return HitTest(sheet, block, new ImmutableRect(p.X - size, p.Y - size, 2 * size, 2 * size), selected, findOnlyOne);
        }

        #endregion

        #region Fill

        public void ToggleFill(IRectangle rectangle)
        {
            _blockHelper.ToggleFill(rectangle);
        }

        public void ToggleFill(IEllipse ellipse)
        {
            _blockHelper.ToggleFill(ellipse);
        }

        public void ToggleFill(IPoint point)
        {
            _blockHelper.ToggleFill(point);
        }

        #endregion

        #region Copy

        public IBlock ShallowCopy(IBlock original)
        {
            var copy = _blockFactory.CreateBlock(original.Id, original.X, original.Y, original.Width, original.Height, original.DataId, original.Name, null);

            copy.Backgroud = original.Backgroud;

            if (original.Points != null)
            {
                copy.Points = new List<IPoint>(original.Points);
            }

            if (original.Lines != null)
            {
                copy.Lines = new List<ILine>(original.Lines);
            }

            if (original.Rectangles != null)
            {
                copy.Rectangles = new List<IRectangle>(original.Rectangles);
            }

            if (original.Ellipses != null)
            {
                copy.Ellipses = new List<IEllipse>(original.Ellipses);
            }

            if (original.Texts != null)
            {
                copy.Texts = new List<IText>(original.Texts);
            }

            if (original.Images != null)
            {
                copy.Images = new List<IImage>(original.Images);
            }

            if (original.Blocks != null)
            {
                copy.Blocks = new List<IBlock>(original.Blocks);
            }

            if (original.Points != null)
            {
                copy.Points = new List<IPoint>(original.Points);
            }

            return copy;
        }

        #endregion
    }
}
