using Sheet.Controller;
using Sheet.Item;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Sheet.Block
{
    public struct ImmutablePoint
    {
        public double X { get; private set; }
        public double Y { get; private set; }
        public ImmutablePoint(double x, double y)
            : this()
        {
            X = x;
            Y = y;
        }
    }

    public struct ImmutableRect
    {
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }
        public ImmutableRect(double x, double y, double width, double height)
            : this()
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }

    public interface IArgbColor
    {
        byte Alpha { get; set; }
        byte Red { get; set; }
        byte Green { get; set; }
        byte Blue { get; set; }
    }

    public interface IDependency
    {
        IElement Element { get; set; }
        Action<IElement, IPoint> Update { get; set; }
    }

    public interface IElement
    {
        int Id { get; set; }
        object Native { get; set; }
    }

    public interface IPoint : IElement
    {
        double X { get; set; }
        double Y { get; set; }
        bool IsVisible { get; set; }
        IList<IDependency> Connected { get; set; }
    }

    public interface IThumb : IElement
    {
    }

    public interface ILine : IElement
    {
        int StartId { get; set; }
        int EndId { get; set; }
        IPoint Start { get; set; }
        IPoint End { get; set; }
    }

    public interface IRectangle : IElement
    {
    }

    public interface IEllipse : IElement
    {
    }

    public interface IText : IElement
    {
    }

    public interface IImage : IElement
    {
    }

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
    }

    public enum XHorizontalAlignment
    {
        Left = 0,
        Center = 1,
        Right = 2
    }

    public enum XVerticalAlignment
    {
        Top = 0,
        Center = 1,
        Bottom = 2
    }

    public class XArgbColor : IArgbColor
    {
        public byte Alpha { get; set; }
        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }
        public XArgbColor(byte alpha, byte red, byte green, byte blue)
        {
            Alpha = alpha;
            Red = red;
            Green = green;
            Blue = blue;
        }
    }

    public class XDependency : IDependency
    {
        public IElement Element { get; set; }
        public Action<IElement, IPoint> Update { get; set; }
        public XDependency(IElement element, Action<IElement, IPoint> update)
        {
            Element = element;
            Update = update;
        }
    }

    public abstract class XElement : IElement
    {
        public int Id { get; set; }
        public object Native { get; set; }
    }

    public class XPoint : XElement, IPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public bool IsVisible { get; set; }
        public IList<IDependency> Connected { get; set; }
        public XPoint(object element, double x, double y, bool isVisible)
        {
            Native = element;
            X = x;
            Y = y;
            IsVisible = isVisible;
            Connected = new List<IDependency>();
        }
    }

    public class XThumb : XElement, IThumb
    {
        public XThumb(object element)
        {
            Native = element;
        }
    }

    public class XLine : XElement, ILine
    {
        public int StartId { get; set; }
        public int EndId { get; set; }
        public IPoint Start { get; set; }
        public IPoint End { get; set; }
        public XLine(object element)
        {
            Native = element;
        }
        public XLine(object element, int startId, int endId)
        {
            StartId = startId;
            EndId = endId;
            Native = element;
        }
        public XLine(object element, IPoint start, IPoint end)
        {
            Start = start;
            End = end;
            Native = element;
        }
    }

    public class XRectangle : XElement, IRectangle
    {
        public XRectangle(object element)
        {
            Native = element;
        }
    }

    public class XEllipse : XElement, IEllipse
    {
        public XEllipse(object element)
        {
            Native = element;
        }
    }

    public class XText : XElement, IText
    {
        public XText(object element)
        {
            Native = element;
        }
    }

    public class XImage : XElement, IImage
    {
        public XImage(object element)
        {
            Native = element;
        }
    }

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
        public XBlock(int id, double x, double y, double width, double height, int dataId, string name)
            : this()
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

    public interface IBlockSerializer
    {
        PointItem Serialize(IPoint point);
        LineItem Serialize(ILine line);
        RectangleItem Serialize(IRectangle rectangle);
        EllipseItem Serialize(IEllipse ellipse);
        TextItem Serialize(IText text);
        ImageItem Serialize(IImage image);
        BlockItem Serialize(IBlock parent);
        BlockItem SerializerAndSetId(IBlock parent, int id, double x, double y, double width, double height, int dataId, string name);
        IPoint Deserialize(ISheet sheet, IBlock parent, PointItem pointItem, double thickness);
        ILine Deserialize(ISheet sheet, IBlock parent, LineItem lineItem, double thickness);
        IRectangle Deserialize(ISheet sheet, IBlock parent, RectangleItem rectangleItem, double thickness);
        IEllipse Deserialize(ISheet sheet, IBlock parent, EllipseItem ellipseItem, double thickness);
        IText Deserialize(ISheet sheet, IBlock parent, TextItem textItem);
        IImage Deserialize(ISheet sheet, IBlock parent, ImageItem imageItem);
        IBlock Deserialize(ISheet sheet, IBlock parent, BlockItem blockItem, double thickness);
    }

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

    public interface IPointController
    {
        void ConnectStart(IPoint point, ILine line);
        void ConnectEnd(IPoint point, ILine line);
        void UpdateDependencies(List<IBlock> blocks, List<IPoint> points, List<ILine> lines);
    }

    public interface IBlockHelper
    {
        bool HitTest(IElement element, ImmutableRect rect);
        bool HitTest(IElement element, ImmutableRect rect, object relativeTo);

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

    public class BlockSerializer : IBlockSerializer
    {
        #region IoC

        private readonly IServiceLocator _serviceLocator;
        private readonly IBlockHelper _blockHelper;
        private readonly IBlockFactory _blockFactory;

        public BlockSerializer(IServiceLocator serviceLocator)
        {
            this._serviceLocator = serviceLocator;
            this._blockHelper = serviceLocator.GetInstance<IBlockHelper>();
            this._blockFactory = serviceLocator.GetInstance<IBlockFactory>();
        }

        #endregion

        #region Ids

        private int SetId(IBlock parent, int nextId)
        {
            foreach (var point in parent.Points)
            {
                point.Id = nextId++;
            }

            foreach (var line in parent.Lines)
            {
                line.Id = nextId++;
            }

            foreach (var rectangle in parent.Rectangles)
            {
                rectangle.Id = nextId++;
            }

            foreach (var ellipse in parent.Ellipses)
            {
                ellipse.Id = nextId++;
            }

            foreach (var text in parent.Texts)
            {
                text.Id = nextId++;
            }

            foreach (var image in parent.Images)
            {
                image.Id = nextId++;
            }

            foreach (var block in parent.Blocks)
            {
                block.Id = nextId++;
                nextId = SetId(block, nextId);
            }

            return nextId;
        }

        #endregion

        #region Serialize

        private ItemColor ToItemColor(byte a, byte r, byte g, byte b)
        {
            return new ItemColor()
            {
                Alpha = a,
                Red = r,
                Green = g,
                Blue = b
            };
        }

        private ItemColor ToItemColor(IArgbColor color)
        {
            return new ItemColor()
            {
                Alpha = color.Alpha,
                Red = color.Red,
                Green = color.Green,
                Blue = color.Blue
            };
        }

        public PointItem Serialize(IPoint point)
        {
            return new PointItem()
            {
                Id = point.Id,
                X = _blockHelper.GetLeft(point),
                Y = _blockHelper.GetTop(point)
            };
        }

        public LineItem Serialize(ILine line)
        {
            return new LineItem()
            {
                Id = line.Id,
                X1 = _blockHelper.GetX1(line),
                Y1 = _blockHelper.GetY1(line),
                X2 = _blockHelper.GetX2(line),
                Y2 = _blockHelper.GetY2(line),
                Stroke = _blockHelper.GetStroke(line),
                StartId = line.Start == null ? -1 : line.Start.Id,
                EndId = line.End == null ? -1 : line.End.Id
            };
        }

        public RectangleItem Serialize(IRectangle rectangle)
        {
            return new RectangleItem()
            {
                Id = rectangle.Id,
                X = _blockHelper.GetLeft(rectangle),
                Y = _blockHelper.GetTop(rectangle),
                Width = _blockHelper.GetWidth(rectangle),
                Height = _blockHelper.GetHeight(rectangle),
                IsFilled = _blockHelper.IsTransparent(rectangle),
                Stroke = _blockHelper.GetStroke(rectangle),
                Fill = _blockHelper.GetFill(rectangle)
            };
        }

        public EllipseItem Serialize(IEllipse ellipse)
        {
            return new EllipseItem()
            {
                Id = ellipse.Id,
                X = _blockHelper.GetLeft(ellipse),
                Y = _blockHelper.GetTop(ellipse),
                Width = _blockHelper.GetWidth(ellipse),
                Height = _blockHelper.GetHeight(ellipse),
                IsFilled = _blockHelper.IsTransparent(ellipse),
                Stroke = _blockHelper.GetStroke(ellipse),
                Fill = _blockHelper.GetFill(ellipse)
            };
        }

        public TextItem Serialize(IText text)
        {
            return new TextItem()
            {
                Id = text.Id,
                X = _blockHelper.GetLeft(text),
                Y = _blockHelper.GetTop(text),
                Width = _blockHelper.GetWidth(text),
                Height = _blockHelper.GetHeight(text),
                Text = _blockHelper.GetText(text),
                HAlign = _blockHelper.GetHAlign(text),
                VAlign = _blockHelper.GetVAlign(text),
                Size = _blockHelper.GetSize(text),
                Backgroud = _blockHelper.GetBackground(text),
                Foreground = _blockHelper.GetForeground(text)
            };
        }

        public ImageItem Serialize(IImage image)
        {
            return new ImageItem()
            {
                Id = image.Id,
                X = _blockHelper.GetLeft(image),
                Y = _blockHelper.GetTop(image),
                Width = _blockHelper.GetWidth(image),
                Height = _blockHelper.GetHeight(image),
                Data = _blockHelper.GetData(image)
            };
        }

        private BlockItem Serialize(IBlock parent, int id, double x, double y, double width, double height, int dataId, string name, IArgbColor backgroud)
        {
            var blockItem = new BlockItem(id, x, y, width, height, dataId, name, ToItemColor(backgroud));

            foreach (var point in parent.Points)
            {
                blockItem.Points.Add(Serialize(point));
            }

            foreach (var line in parent.Lines)
            {
                blockItem.Lines.Add(Serialize(line));
            }

            foreach (var rectangle in parent.Rectangles)
            {
                blockItem.Rectangles.Add(Serialize(rectangle));
            }

            foreach (var ellipse in parent.Ellipses)
            {
                blockItem.Ellipses.Add(Serialize(ellipse));
            }

            foreach (var text in parent.Texts)
            {
                blockItem.Texts.Add(Serialize(text));
            }

            foreach (var image in parent.Images)
            {
                blockItem.Images.Add(Serialize(image));
            }

            foreach (var block in parent.Blocks)
            {
                blockItem.Blocks.Add(Serialize(block));
            }

            return blockItem;
        }

        public BlockItem Serialize(IBlock parent)
        {
            return Serialize(parent, parent.Id, parent.X, parent.Y, parent.Width, parent.Height, parent.DataId, parent.Name, parent.Backgroud);
        }

        public BlockItem SerializerAndSetId(IBlock parent, int id, double x, double y, double width, double height, int dataId, string name)
        {
            SetId(parent, id + 1);
            return Serialize(parent, id, x, y, width, height, dataId, name, parent.Backgroud);
        }

        #endregion

        #region Deserialize

        public IPoint Deserialize(ISheet sheet, IBlock parent, PointItem pointItem, double thickness)
        {
            var point = _blockFactory.CreatePoint(thickness, pointItem.X, pointItem.Y, false);

            point.Id = pointItem.Id;

            parent.Points.Add(point);
            sheet.Add(point);

            return point;
        }

        public ILine Deserialize(ISheet sheet, IBlock parent, LineItem lineItem, double thickness)
        {
            var line = _blockFactory.CreateLine(thickness, lineItem.X1, lineItem.Y1, lineItem.X2, lineItem.Y2, lineItem.Stroke);

            line.Id = lineItem.Id;
            line.StartId = lineItem.StartId;
            line.EndId = lineItem.EndId;

            parent.Lines.Add(line);
            sheet.Add(line);

            return line;
        }

        public IRectangle Deserialize(ISheet sheet, IBlock parent, RectangleItem rectangleItem, double thickness)
        {
            var rectangle = _blockFactory.CreateRectangle(thickness,
                rectangleItem.X,
                rectangleItem.Y,
                rectangleItem.Width,
                rectangleItem.Height,
                rectangleItem.IsFilled,
                rectangleItem.Stroke,
                rectangleItem.Fill);

            rectangle.Id = rectangleItem.Id;

            parent.Rectangles.Add(rectangle);
            sheet.Add(rectangle);

            return rectangle;
        }

        public IEllipse Deserialize(ISheet sheet, IBlock parent, EllipseItem ellipseItem, double thickness)
        {
            var ellipse = _blockFactory.CreateEllipse(thickness,
                ellipseItem.X,
                ellipseItem.Y,
                ellipseItem.Width,
                ellipseItem.Height,
                ellipseItem.IsFilled,
                ellipseItem.Stroke,
                ellipseItem.Fill);

            ellipse.Id = ellipseItem.Id;

            parent.Ellipses.Add(ellipse);
            sheet.Add(ellipse);

            return ellipse;
        }

        public IText Deserialize(ISheet sheet, IBlock parent, TextItem textItem)
        {
            var text = _blockFactory.CreateText(textItem.Text,
                textItem.X, textItem.Y,
                textItem.Width, textItem.Height,
                textItem.HAlign,
                textItem.VAlign,
                textItem.Size,
                textItem.Backgroud,
                textItem.Foreground);

            text.Id = textItem.Id;

            parent.Texts.Add(text);
            sheet.Add(text);

            return text;
        }

        public IImage Deserialize(ISheet sheet, IBlock parent, ImageItem imageItem)
        {
            var image = _blockFactory.CreateImage(imageItem.X,
                imageItem.Y,
                imageItem.Width,
                imageItem.Height,
                imageItem.Data);

            image.Id = imageItem.Id;

            parent.Images.Add(image);
            sheet.Add(image);

            return image;
        }

        public IBlock Deserialize(ISheet sheet, IBlock parent, BlockItem blockItem, double thickness)
        {
            var block = _blockFactory.CreateBlock(blockItem.Id,
                blockItem.X,
                blockItem.Y,
                blockItem.Width,
                blockItem.Height,
                blockItem.DataId,
                blockItem.Name,
                blockItem.Backgroud);

            foreach (var textItem in blockItem.Texts)
            {
                Deserialize(sheet, block, textItem);
            }

            foreach (var imageItem in blockItem.Images)
            {
                Deserialize(sheet, block, imageItem);
            }

            foreach (var lineItem in blockItem.Lines)
            {
                Deserialize(sheet, block, lineItem, thickness);
            }

            foreach (var rectangleItem in blockItem.Rectangles)
            {
                Deserialize(sheet, block, rectangleItem, thickness);
            }

            foreach (var ellipseItem in blockItem.Ellipses)
            {
                Deserialize(sheet, block, ellipseItem, thickness);
            }

            foreach (var childBlockItem in blockItem.Blocks)
            {
                Deserialize(sheet, block, childBlockItem, thickness);
            }

            foreach (var pointItem in blockItem.Points)
            {
                Deserialize(sheet, block, pointItem, thickness);
            }

            parent.Blocks.Add(block);

            return block;
        }

        #endregion
    }

    public class BlockController : IBlockController
    {
        #region IoC

        private readonly IServiceLocator _serviceLocator;
        private readonly IBlockHelper _blockHelper;
        private readonly IBlockSerializer _blockSerializer;
        private readonly IPointController _pointController;

        public BlockController(IServiceLocator serviceLocator)
        {
            this._serviceLocator = serviceLocator;
            this._blockHelper = serviceLocator.GetInstance<IBlockHelper>();
            this._blockSerializer = serviceLocator.GetInstance<IBlockSerializer>();
            this._pointController = serviceLocator.GetInstance<IPointController>();
        }

        #endregion

        #region Add

        public List<IPoint> Add(ISheet sheet, IEnumerable<PointItem> pointItems, IBlock parent, IBlock selected, bool select, double thickness)
        {
            var points = new List<IPoint>();

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
            foreach (var point in points)
            {
                sheet.Remove(point);
            }
        }

        public void Remove(ISheet sheet, IEnumerable<ILine> lines)
        {
            foreach (var line in lines)
            {
                sheet.Remove(line);
            }
        }

        public void Remove(ISheet sheet, IEnumerable<IRectangle> rectangles)
        {
            foreach (var rectangle in rectangles)
            {
                sheet.Remove(rectangle);
            }
        }

        public void Remove(ISheet sheet, IEnumerable<IEllipse> ellipses)
        {
            foreach (var ellipse in ellipses)
            {
                sheet.Remove(ellipse);
            }
        }

        public void Remove(ISheet sheet, IEnumerable<IText> texts)
        {
            foreach (var text in texts)
            {
                sheet.Remove(text);
            }
        }

        public void Remove(ISheet sheet, IEnumerable<IImage> images)
        {
            foreach (var image in images)
            {
                sheet.Remove(image);
            }
        }

        public void Remove(ISheet sheet, IEnumerable<IBlock> blocks)
        {
            foreach (var block in blocks)
            {
                Remove(sheet, block);
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
            Remove(sheet, selected.Points);
            foreach (var point in selected.Points)
            {
                parent.Points.Remove(point);
            }

            Remove(sheet, selected.Lines);
            foreach (var line in selected.Lines)
            {
                parent.Lines.Remove(line);
            }

            Remove(sheet, selected.Rectangles);
            foreach (var rectangle in selected.Rectangles)
            {
                parent.Rectangles.Remove(rectangle);
            }

            Remove(sheet, selected.Ellipses);
            foreach (var ellipse in selected.Ellipses)
            {
                parent.Ellipses.Remove(ellipse);
            }

            Remove(sheet, selected.Texts);
            foreach (var text in selected.Texts)
            {
                parent.Texts.Remove(text);
            }

            Remove(sheet, selected.Images);
            foreach (var image in selected.Images)
            {
                parent.Images.Remove(image);
            }

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
            MoveDelta(dx, dy, block.Points);
            MoveDelta(dx, dy, block.Lines);
            MoveDelta(dx, dy, block.Rectangles);
            MoveDelta(dx, dy, block.Ellipses);
            MoveDelta(dx, dy, block.Texts);
            MoveDelta(dx, dy, block.Images);
            MoveDelta(dx, dy, block.Blocks);
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
            foreach (var point in parent.Points)
            {
                Deselect(point);
            }

            foreach (var line in parent.Lines)
            {
                Deselect(line);
            }

            foreach (var rectangle in parent.Rectangles)
            {
                Deselect(rectangle);
            }

            foreach (var ellipse in parent.Ellipses)
            {
                Deselect(ellipse);
            }

            foreach (var text in parent.Texts)
            {
                Deselect(text);
            }

            foreach (var image in parent.Images)
            {
                Deselect(image);
            }

            foreach (var block in parent.Blocks)
            {
                Deselect(block);
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

        public bool HaveOnlyOnePointSelected(IBlock selected)
        {
            return (selected.Points.Count == 1
                && selected.Lines.Count == 0
                && selected.Rectangles.Count == 0
                && selected.Ellipses.Count == 0
                && selected.Texts.Count == 0
                && selected.Images.Count == 0
                && selected.Blocks.Count == 0);
        }

        public bool HaveOnlyOneLineSelected(IBlock selected)
        {
            return (selected.Points.Count == 0
                && selected.Lines.Count == 1
                && selected.Rectangles.Count == 0
                && selected.Ellipses.Count == 0
                && selected.Texts.Count == 0
                && selected.Images.Count == 0
                && selected.Blocks.Count == 0);
        }

        public bool HaveOnlyOneRectangleSelected(IBlock selected)
        {
            return (selected.Points.Count == 0
                && selected.Lines.Count == 0
                && selected.Rectangles.Count == 1
                && selected.Ellipses.Count == 0
                && selected.Texts.Count == 0
                && selected.Images.Count == 0
                && selected.Blocks.Count == 0);
        }

        public bool HaveOnlyOneEllipseSelected(IBlock selected)
        {
            return (selected.Points.Count == 0
                && selected.Lines.Count == 0
                && selected.Rectangles.Count == 0
                && selected.Ellipses.Count == 1
                && selected.Texts.Count == 0
                && selected.Images.Count == 0
                && selected.Blocks.Count == 0);
        }

        public bool HaveOnlyOneTextSelected(IBlock selected)
        {
            return (selected.Points.Count == 0
                && selected.Lines.Count == 0
                && selected.Rectangles.Count == 0
                && selected.Ellipses.Count == 0
                && selected.Texts.Count == 1
                && selected.Images.Count == 0
                && selected.Blocks.Count == 0);
        }

        public bool HaveOnlyOneImageSelected(IBlock selected)
        {
            return (selected.Points.Count == 0
                && selected.Lines.Count == 0
                && selected.Rectangles.Count == 0
                && selected.Ellipses.Count == 0
                && selected.Texts.Count == 0
                && selected.Images.Count == 1
                && selected.Blocks.Count == 0);
        }

        public bool HaveOnlyOneBlockSelected(IBlock selected)
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

        public bool HitTest(IEnumerable<IBlock> blocks, ImmutableRect rect, object relativeTo, IBlock selected, bool findOnlyOne, bool findInsideBlock)
        {
            foreach (var block in blocks)
            {
                if (HitTest(block, rect, relativeTo, findInsideBlock ? selected : null, true, false))
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

        public bool HitTest(IBlock block, ImmutableRect rect, object relativeTo, IBlock selected, bool findOnlyOne, bool findInsideBlock)
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

            if (HitTest(block.Blocks, rect, relativeTo, selected, findOnlyOne, findInsideBlock))
            {
                if (findOnlyOne)
                {
                    return true;
                }
            }

            return false;
        }

        public bool HitTest(ISheet sheet, IBlock block, ImmutableRect rect, IBlock selected, bool findOnlyOne, bool findInsideBlock)
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

            if (HitTest(block.Blocks, rect, sheet.GetParent(), selected, findOnlyOne, findInsideBlock))
            {
                if (findOnlyOne)
                {
                    return true;
                }
            }

            return false;
        }

        public bool HitTest(ISheet sheet, IBlock block, ImmutablePoint p, double size, IBlock selected, bool findOnlyOne, bool findInsideBlock)
        {
            return HitTest(sheet, block, new ImmutableRect(p.X - size, p.Y - size, 2 * size, 2 * size), selected, findOnlyOne, findInsideBlock);
        }

        #endregion

        #region Toggle Fill

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

        #region Shallow Copy

        public void ShallowCopy(IBlock original, IBlock copy)
        {
            copy.Backgroud = original.Backgroud;
            copy.Points = new List<IPoint>(original.Points);
            copy.Lines = new List<ILine>(original.Lines);
            copy.Rectangles = new List<IRectangle>(original.Rectangles);
            copy.Ellipses = new List<IEllipse>(original.Ellipses);
            copy.Texts = new List<IText>(original.Texts);
            copy.Images = new List<IImage>(original.Images);
            copy.Blocks = new List<IBlock>(original.Blocks);
            copy.Points = new List<IPoint>(original.Points);
        }

        #endregion
    }

    public class PointController : IPointController
    {
        #region IoC

        private readonly IServiceLocator _serviceLocator;
        private readonly IBlockHelper _blockHelper;

        public PointController(IServiceLocator serviceLocator)
        {
            this._serviceLocator = serviceLocator;
            this._blockHelper = _serviceLocator.GetInstance<IBlockHelper>();
        }

        #endregion

        #region Get

        private IEnumerable<KeyValuePair<int, IPoint>> GetAllPoints(IList<IBlock> blocks)
        {
            foreach (var block in blocks)
            {
                foreach (var point in block.Points)
                {
                    yield return new KeyValuePair<int, IPoint>(point.Id, point);
                }

                foreach (var kvp in GetAllPoints(block.Blocks))
                {
                    yield return kvp;
                }
            }
        }

        private IEnumerable<ILine> GetAllLines(IList<IBlock> blocks)
        {
            foreach (var block in blocks)
            {
                foreach (var line in block.Lines)
                {
                    yield return line;
                }

                foreach (var line in GetAllLines(block.Blocks))
                {
                    yield return line;
                }
            }
        }

        #endregion

        #region Connect

        public void ConnectStart(IPoint point, ILine line)
        {
            Action<IElement, IPoint> update = (element, p) =>
            {
                _blockHelper.SetX1(element as ILine, p.X);
                _blockHelper.SetY1(element as ILine, p.Y);
            };
            point.Connected.Add(new XDependency(line, update));
        }

        public void ConnectEnd(IPoint point, ILine line)
        {
            Action<IElement, IPoint> update = (element, p) =>
            {
                _blockHelper.SetX2(element as ILine, p.X);
                _blockHelper.SetY2(element as ILine, p.Y);
            };
            point.Connected.Add(new XDependency(line, update));
        }

        #endregion

        #region Dependencies

        public void UpdateDependencies(List<IBlock> blocks, List<IPoint> points, List<ILine> lines)
        {
            var ps = GetAllPoints(blocks).ToDictionary(x => x.Key, x => x.Value);

            foreach (var point in points)
            {
                ps.Add(point.Id, point);
            }

            var ls = GetAllLines(blocks).ToList();

            foreach (var line in lines)
            {
                ls.Add(line);
            }

            foreach (var line in ls)
            {
                if (line.StartId >= 0)
                {
                    IPoint point;
                    if (ps.TryGetValue(line.StartId, out point))
                    {
                        line.Start = point;
                        ConnectStart(line.Start, line);
                    }
                    else
                    {
                        Debug.Print("Invalid line Start point Id.");
                    }
                }

                if (line.EndId >= 0)
                {
                    IPoint point;
                    if (ps.TryGetValue(line.EndId, out point))
                    {
                        line.End = point;
                        ConnectEnd(line.End, line);
                    }
                    else
                    {
                        Debug.Print("Invalid line End point Id.");
                    }
                }
            }
        }

        #endregion
    }
}
