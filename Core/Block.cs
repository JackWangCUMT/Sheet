using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet
{
    #region Block Core

    public interface IElement
    {
        int Id { get; set; }
        object Native { get; set; }
    }

    public interface IThumb : IElement
    {
    }

    public interface IDependency
    {
        IElement Element { get; set; }
        Action<IElement, IPoint> Update { get; set; }
    }

    public interface IPoint : IElement
    {
        double X { get; set; }
        double Y { get; set; }
        bool IsVisible { get; set; }
        IList<IDependency> Connected { get; set; }
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

    public interface IArgbColor
    {
        byte Alpha { get; set; }
        byte Red { get; set; }
        byte Green { get; set; }
        byte Blue { get; set; }
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
        void Init();
        void ReInit();
    }

    #endregion

    #region Block Model

    public abstract class XElement : IElement
    {
        public int Id { get; set; }
        public object Native { get; set; }
    }

    public class XThumb : XElement, IThumb
    {
        public XThumb(object element)
        {
            Native = element;
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

    #endregion

    #region Block Misc

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

    public struct XImmutablePoint
    {
        public double X { get; private set; }
        public double Y { get; private set; }
        public XImmutablePoint(double x, double y)
            : this()
        {
            X = x;
            Y = y;
        }
    }

    public struct XImmutableRect
    {
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }
        public XImmutableRect(double x, double y, double width, double height)
            : this()
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }

    #endregion

    #region Block Serializer

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
            if (parent.Points != null)
            {
                foreach (var point in parent.Points)
                {
                    point.Id = nextId++;
                }
            }

            if (parent.Lines != null)
            {
                foreach (var line in parent.Lines)
                {
                    line.Id = nextId++;
                }
            }

            if (parent.Rectangles != null)
            {
                foreach (var rectangle in parent.Rectangles)
                {
                    rectangle.Id = nextId++;
                } 
            }

            if (parent.Ellipses != null)
            {
                foreach (var ellipse in parent.Ellipses)
                {
                    ellipse.Id = nextId++;
                } 
            }

            if (parent.Texts != null)
            {
                foreach (var text in parent.Texts)
                {
                    text.Id = nextId++;
                } 
            }

            if (parent.Images != null)
            {
                foreach (var image in parent.Images)
                {
                    image.Id = nextId++;
                } 
            }

            if (parent.Blocks != null)
            {
                foreach (var block in parent.Blocks)
                {
                    block.Id = nextId++;
                    nextId = SetId(block, nextId);
                } 
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
            if (color != null)
            {
                return new ItemColor() 
                { 
                    Alpha = color.Alpha,
                    Red = color.Red,
                    Green = color.Green,
                    Blue = color.Blue
                };
            }
            return null;
        }

        public PointItem Serialize(IPoint point)
        {
            var pointItem = new PointItem();

            pointItem.Id = point.Id;
            pointItem.X = _blockHelper.GetLeft(point);
            pointItem.Y = _blockHelper.GetTop(point);

            return pointItem;
        }

        public LineItem Serialize(ILine line)
        {
            var lineItem = new LineItem();

            lineItem.Id = line.Id;
            lineItem.X1 = _blockHelper.GetX1(line);
            lineItem.Y1 = _blockHelper.GetY1(line);
            lineItem.X2 = _blockHelper.GetX2(line);
            lineItem.Y2 = _blockHelper.GetY2(line);
            lineItem.Stroke = _blockHelper.GetStroke(line);
            lineItem.StartId = line.Start == null ? -1 : line.Start.Id;
            lineItem.EndId = line.End == null ? -1 : line.End.Id;

            return lineItem;
        }

        public RectangleItem Serialize(IRectangle rectangle)
        {
            var rectangleItem = new RectangleItem();

            rectangleItem.Id = rectangle.Id;
            rectangleItem.X = _blockHelper.GetLeft(rectangle);
            rectangleItem.Y = _blockHelper.GetTop(rectangle);
            rectangleItem.Width = _blockHelper.GetWidth(rectangle);
            rectangleItem.Height = _blockHelper.GetHeight(rectangle);
            rectangleItem.IsFilled = _blockHelper.IsTransparent(rectangle);
            rectangleItem.Stroke = _blockHelper.GetStroke(rectangle);
            rectangleItem.Fill = _blockHelper.GetFill(rectangle);

            return rectangleItem;
        }

        public EllipseItem Serialize(IEllipse ellipse)
        {
            var ellipseItem = new EllipseItem();

            ellipseItem.Id = ellipse.Id;
            ellipseItem.X = _blockHelper.GetLeft(ellipse);
            ellipseItem.Y = _blockHelper.GetTop(ellipse);
            ellipseItem.Width = _blockHelper.GetWidth(ellipse);
            ellipseItem.Height = _blockHelper.GetHeight(ellipse);
            ellipseItem.IsFilled = _blockHelper.IsTransparent(ellipse);
            ellipseItem.Stroke = _blockHelper.GetStroke(ellipse);
            ellipseItem.Fill = _blockHelper.GetFill(ellipse);

            return ellipseItem;
        }

        public TextItem Serialize(IText text)
        {
            var textItem = new TextItem();

            textItem.Id = text.Id;
            textItem.X = _blockHelper.GetLeft(text);
            textItem.Y = _blockHelper.GetTop(text);
            textItem.Width = _blockHelper.GetWidth(text);
            textItem.Height = _blockHelper.GetHeight(text);
            textItem.Text = _blockHelper.GetText(text);
            textItem.HAlign = _blockHelper.GetHAlign(text);
            textItem.VAlign = _blockHelper.GetVAlign(text);
            textItem.Size = _blockHelper.GetSize(text);
            textItem.Backgroud = _blockHelper.GetBackground(text);
            textItem.Foreground = _blockHelper.GetForeground(text);

            return textItem;
        }

        public ImageItem Serialize(IImage image)
        {
            var imageItem = new ImageItem();

            imageItem.Id = image.Id;
            imageItem.X = _blockHelper.GetLeft(image);
            imageItem.Y = _blockHelper.GetTop(image);
            imageItem.Width = _blockHelper.GetWidth(image);
            imageItem.Height = _blockHelper.GetHeight(image);
            imageItem.Data = _blockHelper.GetData(image);

            return imageItem;
        }

        public BlockItem Serialize(IBlock parent)
        {
            var blockItem = new BlockItem();
            blockItem.Init(parent.Id, parent.X, parent.Y, parent.Width, parent.Height, parent.DataId, parent.Name);
            blockItem.Backgroud = ToItemColor(parent.Backgroud);

            if (parent.Points != null)
            {
                foreach (var point in parent.Points)
                {
                    blockItem.Points.Add(Serialize(point));
                }
            }

            if (parent.Lines != null)
            {
                foreach (var line in parent.Lines)
                {
                    blockItem.Lines.Add(Serialize(line));
                } 
            }

            if (parent.Rectangles != null)
            {
                foreach (var rectangle in parent.Rectangles)
                {
                    blockItem.Rectangles.Add(Serialize(rectangle));
                } 
            }

            if (parent.Ellipses != null)
            {
                foreach (var ellipse in parent.Ellipses)
                {
                    blockItem.Ellipses.Add(Serialize(ellipse));
                } 
            }

            if (parent.Texts != null)
            {
                foreach (var text in parent.Texts)
                {
                    blockItem.Texts.Add(Serialize(text));
                } 
            }

            if (parent.Images != null)
            {
                foreach (var image in parent.Images)
                {
                    blockItem.Images.Add(Serialize(image));
                } 
            }

            if (parent.Blocks != null)
            {
                foreach (var block in parent.Blocks)
                {
                    blockItem.Blocks.Add(Serialize(block));
                } 
            }

            return blockItem;
        }

        public BlockItem SerializerContents(IBlock parent, int id, double x, double y, double width, double height, int dataId, string name)
        {
            var points = parent.Points;
            var lines = parent.Lines;
            var rectangles = parent.Rectangles;
            var ellipses = parent.Ellipses;
            var texts = parent.Texts;
            var images = parent.Images;
            var blocks = parent.Blocks;
            
            SetId(parent, id + 1);

            var sheet = new BlockItem() { Backgroud = ToItemColor(parent.Backgroud) };
            sheet.Init(id, x, y, width, height, dataId, name);

            if (points != null)
            {
                foreach (var point in points)
                {
                    sheet.Points.Add(Serialize(point));
                }
            }

            if (lines != null)
            {
                foreach (var line in lines)
                {
                    sheet.Lines.Add(Serialize(line));
                }
            }

            if (rectangles != null)
            {
                foreach (var rectangle in rectangles)
                {
                    sheet.Rectangles.Add(Serialize(rectangle));
                }
            }

            if (ellipses != null)
            {
                foreach (var ellipse in ellipses)
                {
                    sheet.Ellipses.Add(Serialize(ellipse));
                }
            }

            if (texts != null)
            {
                foreach (var text in texts)
                {
                    sheet.Texts.Add(Serialize(text));
                }
            }

            if (images != null)
            {
                foreach (var image in images)
                {
                    sheet.Images.Add(Serialize(image));
                }
            }

            if (blocks != null)
            {
                foreach (var block in blocks)
                {
                    sheet.Blocks.Add(Serialize(block));
                }
            }

            return sheet;
        }

        #endregion

        #region Deserialize

        public IPoint Deserialize(ISheet sheet, IBlock parent, PointItem pointItem, double thickness)
        {
            var point = _blockFactory.CreatePoint(thickness, pointItem.X, pointItem.Y, false);
            
            point.Id = pointItem.Id;

            if (parent != null)
            {
                parent.Points.Add(point);
            }

            if (sheet != null)
            {
                sheet.Add(point);
            }

            return point;
        }

        public ILine Deserialize(ISheet sheet, IBlock parent, LineItem lineItem, double thickness)
        {
            var line = _blockFactory.CreateLine(thickness, lineItem.X1, lineItem.Y1, lineItem.X2, lineItem.Y2, lineItem.Stroke);

            line.Id = lineItem.Id;
            line.StartId = lineItem.StartId;
            line.EndId = lineItem.EndId;
            
            if (parent != null)
            {
                parent.Lines.Add(line);
            }

            if (sheet != null)
            {
                sheet.Add(line);
            }

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
            
            if (parent != null)
            {
                parent.Rectangles.Add(rectangle);
            }

            if (sheet != null)
            {
                sheet.Add(rectangle);
            }

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
            
            if (parent != null)
            {
                parent.Ellipses.Add(ellipse);
            }

            if (sheet != null)
            {
                sheet.Add(ellipse);
            }

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
                
            if (parent != null)
            {
                parent.Texts.Add(text);
            }

            if (sheet != null)
            {
                sheet.Add(text);
            }

            return text;
        }

        public IImage Deserialize(ISheet sheet, IBlock parent, ImageItem imageItem)
        {
            var image = _blockFactory.CreateImage(imageItem.X, imageItem.Y, imageItem.Width, imageItem.Height, imageItem.Data);

            image.Id = imageItem.Id;
            
            if (parent != null)
            {
                parent.Images.Add(image);
            }

            if (sheet != null)
            {
                sheet.Add(image);
            }

            return image;
        }

        public IBlock Deserialize(ISheet sheet, IBlock parent, BlockItem blockItem, double thickness)
        {
            var block = _blockFactory.CreateBlock(blockItem.Id, blockItem.X, blockItem.Y, blockItem.Width, blockItem.Height, blockItem.DataId, blockItem.Name, blockItem.Backgroud);
            block.Init();

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

            if (parent != null)
            {
                parent.Blocks.Add(block);
            }

            return block;
        }

        #endregion
    }

    #endregion

    #region Block Controller

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
            if (parent.Points != null)
            {
                foreach (var point in parent.Points)
                {
                    Select(point);
                }
            }

            if (parent.Lines != null)
            {
                foreach (var line in parent.Lines)
                {
                    Select(line);
                }
            }

            if (parent.Rectangles != null)
            {
                foreach (var rectangle in parent.Rectangles)
                {
                    Select(rectangle);
                } 
            }

            if (parent.Ellipses != null)
            {
                foreach (var ellipse in parent.Ellipses)
                {
                    Select(ellipse);
                } 
            }

            if (parent.Texts != null)
            {
                foreach (var text in parent.Texts)
                {
                    Select(text);
                } 
            }

            if (parent.Images != null)
            {
                foreach (var image in parent.Images)
                {
                    Select(image);
                } 
            }

            if (parent.Blocks != null)
            {
                foreach (var block in parent.Blocks)
                {
                    Select(block);
                } 
            }
        }

        public void SelectContent(IBlock selected, IBlock content)
        {
            selected.Init();

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

        public void DeselectContent(IBlock selected)
        {
            if (selected.Points != null)
            {
                foreach (var point in selected.Points)
                {
                    Deselect(point);
                }

                selected.Points = null;
            }

            if (selected.Lines != null)
            {
                foreach (var line in selected.Lines)
                {
                    Deselect(line);
                }

                selected.Lines = null;
            }

            if (selected.Rectangles != null)
            {
                foreach (var rectangle in selected.Rectangles)
                {
                    Deselect(rectangle);
                }

                selected.Rectangles = null;
            }

            if (selected.Ellipses != null)
            {
                foreach (var ellipse in selected.Ellipses)
                {
                    Deselect(ellipse);
                }

                selected.Ellipses = null;
            }

            if (selected.Texts != null)
            {
                foreach (var text in selected.Texts)
                {
                    Deselect(text);
                }

                selected.Texts = null;
            }

            if (selected.Images != null)
            {
                foreach (var image in selected.Images)
                {
                    Deselect(image);
                }

                selected.Images = null;
            }

            if (selected.Blocks != null)
            {
                foreach (var parent in selected.Blocks)
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

                selected.Blocks = null;
            }
        }

        public bool HaveSelected(IBlock selected)
        {
            return (selected.Points != null
                || selected.Lines != null
                || selected.Rectangles != null
                || selected.Ellipses != null
                || selected.Texts != null
                || selected.Images != null
                || selected.Blocks != null);
        }

        public bool HaveOnePointSelected(IBlock selected)
        {
            return (selected.Points != null
                && selected.Points.Count == 1
                && selected.Lines == null
                && selected.Rectangles == null
                && selected.Ellipses == null
                && selected.Texts == null
                && selected.Images == null
                && selected.Blocks == null);
        }

        public bool HaveOneLineSelected(IBlock selected)
        {
            return (selected.Points == null
                && selected.Lines != null
                && selected.Lines.Count == 1
                && selected.Rectangles == null
                && selected.Ellipses == null
                && selected.Texts == null
                && selected.Images == null
                && selected.Blocks == null);
        }

        public bool HaveOneRectangleSelected(IBlock selected)
        {
            return (selected.Points == null
                && selected.Lines == null
                && selected.Rectangles != null
                && selected.Rectangles.Count == 1
                && selected.Ellipses == null
                && selected.Texts == null
                && selected.Images == null
                && selected.Blocks == null);
        }

        public bool HaveOneEllipseSelected(IBlock selected)
        {
            return (selected.Points == null
                && selected.Lines == null
                && selected.Rectangles == null
                && selected.Ellipses != null
                && selected.Ellipses.Count == 1
                && selected.Texts == null
                && selected.Images == null
                && selected.Blocks == null);
        }

        public bool HaveOneTextSelected(IBlock selected)
        {
            return (selected.Points == null
                && selected.Lines == null
                && selected.Rectangles == null
                && selected.Ellipses == null
                && selected.Texts != null
                && selected.Texts.Count == 1
                && selected.Images == null
                && selected.Blocks == null);
        }

        public bool HaveOneImageSelected(IBlock selected)
        {
            return (selected.Points == null
                && selected.Lines == null
                && selected.Rectangles == null
                && selected.Ellipses == null
                && selected.Texts == null
                && selected.Images != null
                && selected.Images.Count == 1
                && selected.Blocks == null);
        }

        public bool HaveOneBlockSelected(IBlock selected)
        {
            return (selected.Points == null
                && selected.Lines == null
                && selected.Rectangles == null
                && selected.Ellipses == null
                && selected.Texts == null
                && selected.Images == null
                && selected.Blocks != null
                && selected.Blocks.Count == 1);
        }

        #endregion

        #region HitTest

        private void HitTestClean(IBlock selected)
        {
            if (selected.Points != null && selected.Points.Count == 0)
            {
                selected.Points = null;
            }

            if (selected.Lines != null && selected.Lines.Count == 0)
            {
                selected.Lines = null;
            }

            if (selected.Rectangles != null && selected.Rectangles.Count == 0)
            {
                selected.Rectangles = null;
            }

            if (selected.Ellipses != null && selected.Ellipses.Count == 0)
            {
                selected.Ellipses = null;
            }

            if (selected.Texts != null && selected.Texts.Count == 0)
            {
                selected.Texts = null;
            }

            if (selected.Images != null && selected.Images.Count == 0)
            {
                selected.Images = null;
            }

            if (selected.Blocks != null && selected.Blocks.Count == 0)
            {
                selected.Blocks = null;
            }
        }

        public bool HitTest(IEnumerable<IPoint> points, IBlock selected, XImmutableRect rect, bool onlyFirst, bool select, object relativeTo)
        {
            foreach (var point in points)
            {
                if (_blockHelper.HitTest(point, rect, relativeTo))
                {
                    if (select)
                    {
                        if (!_blockHelper.GetIsSelected(point))
                        {
                            Select(point);
                            selected.Points.Add(point);
                        }
                        else
                        {
                            Deselect(point);
                            selected.Points.Remove(point);
                        }
                    }

                    if (onlyFirst)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool HitTest(IEnumerable<ILine> lines, IBlock selected, XImmutableRect rect, bool onlyFirst, bool select)
        {
            foreach (var line in lines)
            {
                if (_blockHelper.HitTest(line, rect))
                {
                    if (select)
                    {
                        if (_blockHelper.IsSelected(line))
                        {
                            Select(line);
                            selected.Lines.Add(line);
                        }
                        else
                        {
                            Deselect(line);
                            selected.Lines.Remove(line);
                        }
                    }

                    if (onlyFirst)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool HitTest(IEnumerable<IRectangle> rectangles, IBlock selected, XImmutableRect rect, bool onlyFirst, bool select, object relativeTo)
        {
            foreach (var rectangle in rectangles)
            {
                if (_blockHelper.HitTest(rectangle, rect, relativeTo))
                {
                    if (select)
                    {
                        if (_blockHelper.IsSelected(rectangle))
                        {
                            Select(rectangle);
                            selected.Rectangles.Add(rectangle);
                        }
                        else
                        {
                            Deselect(rectangle);
                            selected.Rectangles.Remove(rectangle);
                        }
                    }

                    if (onlyFirst)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool HitTest(IEnumerable<IEllipse> ellipses, IBlock selected, XImmutableRect rect, bool onlyFirst, bool select, object relativeTo)
        {
            foreach (var ellipse in ellipses)
            {
                if (_blockHelper.HitTest(ellipse, rect, relativeTo))
                {
                    if (select)
                    {
                        if (_blockHelper.IsSelected(ellipse))
                        {
                            Select(ellipse);
                            selected.Ellipses.Add(ellipse);
                        }
                        else
                        {
                            Deselect(ellipse);
                            selected.Ellipses.Remove(ellipse);
                        }
                    }

                    if (onlyFirst)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool HitTest(IEnumerable<IText> texts, IBlock selected, XImmutableRect rect, bool onlyFirst, bool select, object relativeTo)
        {
            foreach (var text in texts)
            {
                if (_blockHelper.HitTest(text, rect, relativeTo))
                {
                    if (select)
                    {
                        if (_blockHelper.IsSelected(text))
                        {
                            Select(text);
                            selected.Texts.Add(text);
                        }
                        else
                        {
                            Deselect(text);
                            selected.Texts.Remove(text);
                        }
                    }

                    if (onlyFirst)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool HitTest(IEnumerable<IImage> images, IBlock selected, XImmutableRect rect, bool onlyFirst, bool select, object relativeTo)
        {
            foreach (var image in images)
            {
                if (_blockHelper.HitTest(image, rect, relativeTo))
                {
                    if (select)
                    {
                        if (_blockHelper.IsSelected(image))
                        {
                            Select(image);
                            selected.Images.Add(image);
                        }
                        else
                        {
                            Deselect(image);
                            selected.Images.Remove(image);
                        }
                    }

                    if (onlyFirst)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool HitTest(IEnumerable<IBlock> blocks, IBlock selected, XImmutableRect rect, bool onlyFirst, bool select, bool selectInsideBlock, object relativeTo)
        {
            foreach (var block in blocks)
            {
                bool result = HitTest(block, selected, rect, true, selectInsideBlock, relativeTo);

                if (result)
                {
                    if (select && !selectInsideBlock)
                    {
                        if (!selected.Blocks.Contains(block))
                        {
                            selected.Blocks.Add(block);
                            Select(block);
                        }
                        else
                        {
                            selected.Blocks.Remove(block);
                            Deselect(block);
                        }
                    }

                    if (onlyFirst)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool HitTest(IBlock parent, IBlock selected, XImmutableRect rect, bool onlyFirst, bool selectInsideBlock, object relativeTo)
        {
            bool result = false;

            result = HitTest(parent.Points, selected, rect, onlyFirst, selectInsideBlock, relativeTo);
            if (result && onlyFirst)
            {
                return true;
            }

            result = HitTest(parent.Texts, selected, rect, onlyFirst, selectInsideBlock, relativeTo);
            if (result && onlyFirst)
            {
                return true;
            }

            result = HitTest(parent.Images, selected, rect, onlyFirst, selectInsideBlock, relativeTo);
            if (result && onlyFirst)
            {
                return true;
            }

            result = HitTest(parent.Lines, selected, rect, onlyFirst, selectInsideBlock);
            if (result && onlyFirst)
            {
                return true;
            }

            result = HitTest(parent.Rectangles, selected, rect, onlyFirst, selectInsideBlock, relativeTo);
            if (result && onlyFirst)
            {
                return true;
            }

            result = HitTest(parent.Ellipses, selected, rect, onlyFirst, selectInsideBlock, relativeTo);
            if (result && onlyFirst)
            {
                return true;
            }

            result = HitTest(parent.Blocks, selected, rect, onlyFirst, false, selectInsideBlock, relativeTo);
            if (result && onlyFirst)
            {
                return true;
            }

            return false;
        }

        public bool HitTestClick(ISheet sheet, IBlock parent, IBlock selected, XImmutablePoint p, double size, bool selectInsideBlock, bool resetSelected)
        {
            if (resetSelected)
            {
                selected.Init();
            }
            else
            {
                selected.ReInit();
            }

            var rect = new XImmutableRect(p.X - size, p.Y - size, 2 * size, 2 * size);

            if (parent.Points != null)
            {
                bool result = HitTest(parent.Points, selected, rect, true, true, sheet.GetParent());
                if (result)
                {
                    HitTestClean(selected);
                    return true;
                }
            }
            
            if (parent.Texts != null)
            {
                bool result = HitTest(parent.Texts, selected, rect, true, true, sheet.GetParent());
                if (result)
                {
                    HitTestClean(selected);
                    return true;
                }
            }

            if (parent.Images != null)
            {
                bool result = HitTest(parent.Images, selected, rect, true, true, sheet.GetParent());
                if (result)
                {
                    HitTestClean(selected);
                    return true;
                }
            }

            if (parent.Lines != null)
            {
                bool result = HitTest(parent.Lines, selected, rect, true, true);
                if (result)
                {
                    HitTestClean(selected);
                    return true;
                }
            }

            if (parent.Rectangles != null)
            {
                bool result = HitTest(parent.Rectangles, selected, rect, true, true, sheet.GetParent());
                if (result)
                {
                    HitTestClean(selected);
                    return true;
                }
            }

            if (parent.Ellipses != null)
            {
                bool result = HitTest(parent.Ellipses, selected, rect, true, true, sheet.GetParent());
                if (result)
                {
                    HitTestClean(selected);
                    return true;
                }
            }

            if (parent.Blocks != null)
            {
                bool result = HitTest(parent.Blocks, selected, rect, true, true, selectInsideBlock, sheet.GetParent());
                if (result)
                {
                    HitTestClean(selected);
                    return true;
                }
            }

            HitTestClean(selected);
            return false;
        }

        public bool HitTestForBlocks(ISheet sheet, IBlock parent, IBlock selected, XImmutablePoint p, double size)
        {
            selected.Init();

            var rect = new XImmutableRect(p.X - size, p.Y - size, 2 * size, 2 * size);

            if (parent.Blocks != null)
            {
                bool result = HitTest(parent.Blocks, selected, rect, true, true, false, sheet.GetParent());
                if (result)
                {
                    HitTestClean(selected);
                    return true;
                }
            }

            HitTestClean(selected);
            return false;
        }

        public void HitTestSelectionRect(ISheet sheet, IBlock parent, IBlock selected, XImmutableRect rect, bool resetSelected)
        {
            if (resetSelected)
            {
                selected.Init();
            }
            else
            {
                selected.ReInit();
            }

            if (parent.Points != null)
            {
                HitTest(parent.Points, selected, rect, false, true, sheet.GetParent());
            }

            if (parent.Lines != null)
            {
                HitTest(parent.Lines, selected, rect, false, true);
            }

            if (parent.Rectangles != null)
            {
                HitTest(parent.Rectangles, selected, rect, false, true, sheet.GetParent());
            }

            if (parent.Ellipses != null)
            {
                HitTest(parent.Ellipses, selected, rect, false, true, sheet.GetParent());
            }

            if (parent.Texts != null)
            {
                HitTest(parent.Texts, selected, rect, false, true, sheet.GetParent());
            }

            if (parent.Images != null)
            {
                HitTest(parent.Images, selected, rect, false, true, sheet.GetParent());
            }

            if (parent.Blocks != null)
            {
                HitTest(parent.Blocks, selected, rect, false, true, false, sheet.GetParent());
            }

            HitTestClean(selected);
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

    #endregion
}
