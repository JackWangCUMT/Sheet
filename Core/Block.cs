using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Sheet
{
    #region Block Model

    public abstract class XElement
    {
        public int Id { get; set; }
        public object Element { get; set; }
    }

    public class XThumb : XElement
    {
        public XThumb(object element)
        {
            Element = element;
        }
    }

    public class XDependency
    {
        public XElement Element { get; set; }
        public Action<XElement, XPoint> Update { get; set; }
        public XDependency(XElement element, Action<XElement, XPoint> update)
        {
            Element = element;
            Update = update;
        }
    }

    public class XPoint : XElement
    {
        public double X { get; set; }
        public double Y { get; set; }
        public bool IsVisible { get; set; }
        public List<XDependency> Connected { get; set; }
        public XPoint(object element, double x, double y, bool isVisible)
        {
            Element = element;
            X = x;
            Y = y;
            IsVisible = isVisible;
            Connected = new List<XDependency>();
        }
    }

    public class XLine : XElement
    {
        public int StartId { get; set; }
        public int EndId { get; set; }
        public XPoint Start { get; set; }
        public XPoint End { get; set; }
        public XLine(object element)
        {
            Element = element;
        }
        public XLine(object element, int startId, int endId)
        {
            StartId = startId;
            EndId = endId;
            Element = element;
        }
        public XLine(object element, XPoint start, XPoint end)
        {
            Start = start;
            End = end;
            Element = element;
        }
    }

    public class XRectangle : XElement
    {
        public XRectangle(object element)
        {
            Element = element;
        }
    }

    public class XEllipse : XElement
    {
        public XEllipse(object element)
        {
            Element = element;
        }
    }

    public class XText : XElement
    {
        public XText(object element)
        {
            Element = element;
        }
    }

    public class XImage : XElement
    {
        public XImage(object element)
        {
            Element = element;
        }
    }

    public class XBlock
    {
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public string Name { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public int DataId { get; set; }
        public Color Backgroud { get; set; }
        public List<XPoint> Points { get; set; }
        public List<XLine> Lines { get; set; }
        public List<XRectangle> Rectangles { get; set; }
        public List<XEllipse> Ellipses { get; set; }
        public List<XText> Texts { get; set; }
        public List<XImage> Images { get; set; }
        public List<XBlock> Blocks { get; set; }
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
            Points = new List<XPoint>();
            Lines = new List<XLine>();
            Rectangles = new List<XRectangle>();
            Ellipses = new List<XEllipse>();
            Texts = new List<XText>();
            Images = new List<XImage>();
            Blocks = new List<XBlock>();
        }
        public void ReInit()
        {
            if (Points == null)
            {
                Points = new List<XPoint>();
            }

            if (Lines == null)
            {
                Lines = new List<XLine>();
            }

            if (Rectangles == null)
            {
                Rectangles = new List<XRectangle>();
            }

            if (Ellipses == null)
            {
                Ellipses = new List<XEllipse>();
            }

            if (Texts == null)
            {
                Texts = new List<XText>();
            }

            if (Images == null)
            {
                Images = new List<XImage>();
            }

            if (Blocks == null)
            {
                Blocks = new List<XBlock>();
            }
        }
    }

    public interface ISheet
    {
        object GetParent();
        void Add(XElement element);
        void Remove(XElement element);
        void Capture();
        void ReleaseCapture();
        bool IsCaptured { get; }
    }

    public enum SheetMode
    {
        None,
        Selection,
        Insert,
        Pan,
        Move,
        Edit,
        Point,
        Line,
        Rectangle,
        Ellipse,
        Text,
        Image,
        TextEditor
    }

    public class SheetOptions
    {
        public double PageOriginX { get; set; }
        public double PageOriginY { get; set; }
        public double PageWidth { get; set; }
        public double PageHeight { get; set; }
        public double SnapSize { get; set; }
        public double GridSize { get; set; }
        public double FrameThickness { get; set; }
        public double GridThickness { get; set; }
        public double SelectionThickness { get; set; }
        public double LineThickness { get; set; }
        public double HitTestSize { get; set; }
        public int DefaultZoomIndex { get; set; }
        public int MaxZoomIndex { get; set; }
        public double[] ZoomFactors { get; set; }
    }

    #endregion

    #region Block Serializer

    public static class BlockSerializer
    {
        #region Id
        
        public static int SetId(XBlock parent, int nextId)
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

        private static ItemColor ToItemColor(Brush brush)
        {
            var color = (brush as SolidColorBrush).Color;
            return ToItemColor(color);
        }

        private static ItemColor ToItemColor(Color color)
        {
            return new ItemColor()
            {
                Alpha = color.A,
                Red = color.R,
                Green = color.G,
                Blue = color.B
            };
        }

        public static PointItem Serialize(XPoint point)
        {
            var pointItem = new PointItem();

            pointItem.Id = point.Id;
            pointItem.X = Canvas.GetLeft(point.Element as FrameworkElement);
            pointItem.Y = Canvas.GetTop(point.Element as FrameworkElement);

            return pointItem;
        }

        public static LineItem Serialize(XLine line)
        {
            var lineItem = new LineItem();

            lineItem.Id = line.Id;
            lineItem.X1 = (line.Element as Line).X1;
            lineItem.Y1 = (line.Element as Line).Y1;
            lineItem.X2 = (line.Element as Line).X2;
            lineItem.Y2 = (line.Element as Line).Y2;
            lineItem.Stroke = ToItemColor((line.Element as Line).Stroke);
            lineItem.StartId = line.Start == null ? -1 : line.Start.Id;
            lineItem.EndId = line.End == null ? -1 : line.End.Id;

            return lineItem;
        }

        public static RectangleItem Serialize(XRectangle rectangle)
        {
            var rectangleItem = new RectangleItem();

            rectangleItem.Id = rectangle.Id;
            rectangleItem.X = Canvas.GetLeft(rectangle.Element as Rectangle);
            rectangleItem.Y = Canvas.GetTop(rectangle.Element as Rectangle);
            rectangleItem.Width = (rectangle.Element as Rectangle).Width;
            rectangleItem.Height = (rectangle.Element as Rectangle).Height;
            rectangleItem.IsFilled = (rectangle.Element as Rectangle).Fill == BlockFactory.TransparentBrush ? false : true;
            rectangleItem.Stroke = ToItemColor((rectangle.Element as Rectangle).Stroke);
            rectangleItem.Fill = ToItemColor((rectangle.Element as Rectangle).Fill);

            return rectangleItem;
        }

        public static EllipseItem Serialize(XEllipse ellipse)
        {
            var ellipseItem = new EllipseItem();

            ellipseItem.Id = ellipse.Id;
            ellipseItem.X = Canvas.GetLeft(ellipse.Element as Ellipse);
            ellipseItem.Y = Canvas.GetTop(ellipse.Element as Ellipse);
            ellipseItem.Width = (ellipse.Element as Ellipse).Width;
            ellipseItem.Height = (ellipse.Element as Ellipse).Height;
            ellipseItem.IsFilled = (ellipse.Element as Ellipse).Fill == BlockFactory.TransparentBrush ? false : true;
            ellipseItem.Stroke = ToItemColor((ellipse.Element as Ellipse).Stroke);
            ellipseItem.Fill = ToItemColor((ellipse.Element as Ellipse).Fill);

            return ellipseItem;
        }

        public static TextItem Serialize(XText text)
        {
            var textItem = new TextItem();

            textItem.Id = text.Id;
            textItem.X = Canvas.GetLeft(text.Element as Grid);
            textItem.Y = Canvas.GetTop(text.Element as Grid);
            textItem.Width = (text.Element as Grid).Width;
            textItem.Height = (text.Element as Grid).Height;

            var tb = BlockFactory.GetTextBlock(text);
            textItem.Text = tb.Text;
            textItem.HAlign = (int)tb.HorizontalAlignment;
            textItem.VAlign = (int)tb.VerticalAlignment;
            textItem.Size = tb.FontSize;
            textItem.Foreground = ToItemColor(tb.Foreground);
            textItem.Backgroud = ToItemColor(tb.Background);

            return textItem;
        }

        public static ImageItem Serialize(XImage image)
        {
            var imageItem = new ImageItem();

            imageItem.Id = image.Id;
            imageItem.X = Canvas.GetLeft(image.Element as Image);
            imageItem.Y = Canvas.GetTop(image.Element as Image);
            imageItem.Width = (image.Element as Image).Width;
            imageItem.Height = (image.Element as Image).Height;
            imageItem.Data = (image.Element as Image).Tag as byte[];

            return imageItem;
        }

        public static BlockItem Serialize(XBlock parent)
        {
            var blockItem = new BlockItem();
            blockItem.Init(parent.Id, parent.X, parent.Y, parent.Width, parent.Height, parent.DataId, parent.Name);
            blockItem.Backgroud = ToItemColor(parent.Backgroud);

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

        public static BlockItem SerializerContents(XBlock parent, int id, double x, double y, double width, double height, int dataId, string name)
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

        public static XPoint Deserialize(ISheet sheet, XBlock parent, PointItem pointItem, double thickness)
        {
            var point = BlockFactory.CreatePoint(thickness, pointItem.X, pointItem.Y, false);
            
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

        public static XLine Deserialize(ISheet sheet, XBlock parent, LineItem lineItem, double thickness)
        {
            var line = BlockFactory.CreateLine(thickness, lineItem.X1, lineItem.Y1, lineItem.X2, lineItem.Y2, lineItem.Stroke);

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

        public static XRectangle Deserialize(ISheet sheet, XBlock parent, RectangleItem rectangleItem, double thickness)
        {
            var rectangle = BlockFactory.CreateRectangle(thickness, rectangleItem.X, rectangleItem.Y, rectangleItem.Width, rectangleItem.Height, rectangleItem.IsFilled);

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

        public static XEllipse Deserialize(ISheet sheet, XBlock parent, EllipseItem ellipseItem, double thickness)
        {
            var ellipse = BlockFactory.CreateEllipse(thickness, ellipseItem.X, ellipseItem.Y, ellipseItem.Width, ellipseItem.Height, ellipseItem.IsFilled);

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

        public static XText Deserialize(ISheet sheet, XBlock parent, TextItem textItem)
        {
            var text = BlockFactory.CreateText(textItem.Text,
                textItem.X, textItem.Y,
                textItem.Width, textItem.Height,
                (HorizontalAlignment)textItem.HAlign,
                (VerticalAlignment)textItem.VAlign,
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

        public static XImage Deserialize(ISheet sheet, XBlock parent, ImageItem imageItem)
        {
            var image = BlockFactory.CreateImage(imageItem.X, imageItem.Y, imageItem.Width, imageItem.Height, imageItem.Data);

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

        public static XBlock Deserialize(ISheet sheet, XBlock parent, BlockItem blockItem, bool select, double thickness)
        {
            var block = new XBlock(blockItem.Id, blockItem.X, blockItem.Y, blockItem.Width, blockItem.Height, blockItem.DataId, blockItem.Name);
            block.Init();

            foreach (var textItem in blockItem.Texts)
            {
                var text = Deserialize(sheet, block, textItem);

                if (select)
                {
                    BlockController.Select(text);
                }
            }

            foreach (var imageItem in blockItem.Images)
            {
                var image = Deserialize(sheet, block, imageItem);

                if (select)
                {
                    BlockController.Select(image);
                }
            }

            foreach (var lineItem in blockItem.Lines)
            {
                var line = Deserialize(sheet, block, lineItem, thickness);

                if (select)
                {
                    BlockController.Select(line);
                }
            }

            foreach (var rectangleItem in blockItem.Rectangles)
            {
                var rectangle = Deserialize(sheet, block, rectangleItem, thickness);

                if (select)
                {
                    BlockController.Select(rectangle);
                }
            }

            foreach (var ellipseItem in blockItem.Ellipses)
            {
                var ellipse = Deserialize(sheet, block, ellipseItem, thickness);

                if (select)
                {
                    BlockController.Select(ellipse);
                }
            }

            foreach (var childBlockItem in blockItem.Blocks)
            {
                Deserialize(sheet, block, childBlockItem, select, thickness);
            }

            foreach (var pointItem in blockItem.Points)
            {
                var point = Deserialize(sheet, block, pointItem, thickness);

                if (select)
                {
                    BlockController.Select(point);
                }
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

    public static class BlockController
    {
        #region Get

        public static IEnumerable<KeyValuePair<int, XPoint>> GetAllPoints(List<XBlock> blocks)
        {
            foreach (var block in blocks)
            {
                if (block.Points != null)
                {
                    foreach (var point in block.Points)
                    {
                        yield return new KeyValuePair<int, XPoint>(point.Id, point);
                    }
                }

                if (block.Blocks != null)
                {
                    foreach (var kvp in GetAllPoints(block.Blocks))
                    {
                        yield return kvp;
                    }
                }
            }
        }

        public static IEnumerable<XLine> GetAllLines(List<XBlock> blocks)
        {
            foreach (var block in blocks)
            {
                if (block.Lines != null)
                {
                    foreach (var line in block.Lines)
                    {
                        yield return line;
                    }
                }

                if (block.Blocks != null)
                {
                    foreach (var line in GetAllLines(block.Blocks))
                    {
                        yield return line;
                    }
                }
            }
        }

        #endregion

        #region Add

        public static List<XPoint> Add(ISheet sheet, IEnumerable<PointItem> pointItems, XBlock parent, XBlock selected, bool select, double thickness)
        {
            var points = new List<XPoint>();
        
            if (select)
            {
                selected.Points = new List<XPoint>();
            }

            foreach (var pointItem in pointItems)
            {
                var point = BlockSerializer.Deserialize(sheet, parent, pointItem, thickness);

                points.Add(point);
                
                if (select)
                {
                    Select(point);
                    selected.Points.Add(point);
                }
            }
            
            return points;
        }

        public static List<XLine> Add(ISheet sheet, IEnumerable<LineItem> lineItems, XBlock parent, XBlock selected, bool select, double thickness)
        {
            var lines = new List<XLine>();
        
            if (select)
            {
                selected.Lines = new List<XLine>();
            }

            foreach (var lineItem in lineItems)
            {
                var line = BlockSerializer.Deserialize(sheet, parent, lineItem, thickness);

                lines.Add(line);
                
                if (select)
                {
                    Select(line);
                    selected.Lines.Add(line);
                }
            }
            
            return lines;
        }

        public static List<XRectangle> Add(ISheet sheet, IEnumerable<RectangleItem> rectangleItems, XBlock parent, XBlock selected, bool select, double thickness)
        {
            var rectangles = new List<XRectangle>();
        
            if (select)
            {
                selected.Rectangles = new List<XRectangle>();
            }

            foreach (var rectangleItem in rectangleItems)
            {
                var rectangle = BlockSerializer.Deserialize(sheet, parent, rectangleItem, thickness);

                rectangles.Add(rectangle);
                
                if (select)
                {
                    Select(rectangle);
                    selected.Rectangles.Add(rectangle);
                }
            }
            
            return rectangles;
        }

        public static List<XEllipse> Add(ISheet sheet, IEnumerable<EllipseItem> ellipseItems, XBlock parent, XBlock selected, bool select, double thickness)
        {
            var ellipses = new List<XEllipse>();
        
            if (select)
            {
                selected.Ellipses = new List<XEllipse>();
            }

            foreach (var ellipseItem in ellipseItems)
            {
                var ellipse = BlockSerializer.Deserialize(sheet, parent, ellipseItem, thickness);

                ellipses.Add(ellipse);
                
                if (select)
                {
                    Select(ellipse);
                    selected.Ellipses.Add(ellipse);
                }
            }
            
            return ellipses;
        }

        public static List<XText> Add(ISheet sheet, IEnumerable<TextItem> textItems, XBlock parent, XBlock selected, bool select, double thickness)
        {
            var texts = new List<XText>();
        
            if (select)
            {
                selected.Texts = new List<XText>();
            }

            foreach (var textItem in textItems)
            {
                var text = BlockSerializer.Deserialize(sheet, parent, textItem);

                texts.Add(text);
                
                if (select)
                {
                    Select(text);
                    selected.Texts.Add(text);
                }
            }
            
            return texts;
        }

        public static List<XImage> Add(ISheet sheet, IEnumerable<ImageItem> imageItems, XBlock parent, XBlock selected, bool select, double thickness)
        {
            var images = new List<XImage>();
        
            if (select)
            {
                selected.Images = new List<XImage>();
            }

            foreach (var imageItem in imageItems)
            {
                var image = BlockSerializer.Deserialize(sheet, parent, imageItem);

                images.Add(image);
                
                if (select)
                {
                    Select(image);
                    selected.Images.Add(image);
                }
            }
            
            return images;
        }

        public static List<XBlock> Add(ISheet sheet, IEnumerable<BlockItem> blockItems, XBlock parent, XBlock selected, bool select, double thickness)
        {
            var blocks = new List<XBlock>();
        
            if (select)
            {
                selected.Blocks = new List<XBlock>();
            }

            foreach (var blockItem in blockItems)
            {
                var block = BlockSerializer.Deserialize(sheet, parent, blockItem, select, thickness);
                
                blocks.Add(block);

                if (select)
                {
                    selected.Blocks.Add(block);
                }
            }
            
            return blocks;
        }

        public static void AddContents(ISheet sheet, BlockItem blockItem, XBlock content, XBlock selected, bool select, double thickness)
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
                
                // get all points
                var ps = GetAllPoints(blocks).ToDictionary(x => x.Key, x => x.Value);
                
                foreach(var point in points)
                {
                    ps.Add(point.Id, point);
                }

                // get all lines
                var ls = GetAllLines(blocks).ToList();
                
                foreach(var line in lines)
                {
                	ls.Add(line);
                }

                // update point dependencies
                foreach(var line in ls)
                {
                    if (line.StartId >= 0)
                    {
                        var point = ps[line.StartId];
                        line.Start = point;
                        PointController.ConnectStart(line.Start, line);
                    }
                    
                    if (line.EndId >= 0)
                    {
                        var point = ps[line.EndId];
                        line.End = point;
                        PointController.ConnectEnd(line.End, line);
                    }
                }
            }
        }

        public static void AddBroken(ISheet sheet, BlockItem blockItem, XBlock content, XBlock selected, bool select, double thickness)
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

        public static void Remove(ISheet sheet, IEnumerable<XPoint> points)
        {
            if (points != null)
            {
                foreach (var point in points)
                {
                    sheet.Remove(point);
                }
            }
        }

        public static void Remove(ISheet sheet, IEnumerable<XLine> lines)
        {
            if (lines != null)
            {
                foreach (var line in lines)
                {
                    sheet.Remove(line);
                }
            }
        }

        public static void Remove(ISheet sheet, IEnumerable<XRectangle> rectangles)
        {
            if (rectangles != null)
            {
                foreach (var rectangle in rectangles)
                {
                    sheet.Remove(rectangle);
                }
            }
        }

        public static void Remove(ISheet sheet, IEnumerable<XEllipse> ellipses)
        {
            if (ellipses != null)
            {
                foreach (var ellipse in ellipses)
                {
                    sheet.Remove(ellipse);
                }
            }
        }

        public static void Remove(ISheet sheet, IEnumerable<XText> texts)
        {
            if (texts != null)
            {
                foreach (var text in texts)
                {
                    sheet.Remove(text);
                }
            }
        }

        public static void Remove(ISheet sheet, IEnumerable<XImage> images)
        {
            if (images != null)
            {
                foreach (var image in images)
                {
                    sheet.Remove(image);
                }
            }
        }

        public static void Remove(ISheet sheet, IEnumerable<XBlock> blocks)
        {
            if (blocks != null)
            {
                foreach (var block in blocks)
                {
                    Remove(sheet, block);
                }
            }
        }

        public static void Remove(ISheet sheet, XBlock block)
        {
            Remove(sheet, block.Points);
            Remove(sheet, block.Lines);
            Remove(sheet, block.Rectangles);
            Remove(sheet, block.Ellipses);
            Remove(sheet, block.Texts);
            Remove(sheet, block.Images);
            Remove(sheet, block.Blocks);
        }

        public static void RemoveSelected(ISheet sheet, XBlock parent, XBlock selected)
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

        #region Move Points

        public static void Move(double x, double y, XPoint point)
        {
            if (point.Element != null)
            {
                point.X = Canvas.GetLeft(point.Element as Ellipse) + x;
                point.Y = Canvas.GetTop(point.Element as Ellipse) + y;
                Canvas.SetLeft(point.Element as Ellipse, point.X);
                Canvas.SetTop(point.Element as Ellipse, point.Y);
            }
            else
            {
                point.X += x;
                point.Y += y;
            }

            foreach (var dependency in point.Connected)
            {
                dependency.Update(dependency.Element, point);
            }
        }

        public static void Move(double x, double y, IEnumerable<XPoint> points)
        {
            foreach (var point in points)
            {
                Move(x, y, point);
            }
        }

        #endregion

        #region Move Lines
		 
        public static void Move(double x, double y, IEnumerable<XLine> lines)
        {
            foreach (var line in lines)
            {
                if (line.Start == null)
                {
                    MoveStart(x, y, line);
                }

                if (line.End == null)
                {
                    MoveEnd(x, y, line);
                }
            }
        }

        public static void MoveStart(double x, double y, XLine line)
        {
            (line.Element as Line).X1 += x;
            (line.Element as Line).Y1 += y;
        }

        public static void MoveEnd(double x, double y, XLine line)
        {
            (line.Element as Line).X2 += x;
            (line.Element as Line).Y2 += y;
        } 

	    #endregion

        #region Move Rectangles

        public static void Move(double x, double y, XRectangle rectangle)
        {
            Canvas.SetLeft(rectangle.Element as Rectangle, Canvas.GetLeft(rectangle.Element as Rectangle) + x);
            Canvas.SetTop(rectangle.Element as Rectangle, Canvas.GetTop(rectangle.Element as Rectangle) + y);
        }

        public static void Move(double x, double y, IEnumerable<XRectangle> rectangles)
        {
            foreach (var rectangle in rectangles)
            {
                Move(x, y, rectangle);
            }
        } 

        #endregion

        #region Move Ellipses

        public static void Move(double x, double y, XEllipse ellipse)
        {
            Canvas.SetLeft(ellipse.Element as Ellipse, Canvas.GetLeft(ellipse.Element as Ellipse) + x);
            Canvas.SetTop(ellipse.Element as Ellipse, Canvas.GetTop(ellipse.Element as Ellipse) + y);
        }

        public static void Move(double x, double y, IEnumerable<XEllipse> ellipses)
        {
            foreach (var ellipse in ellipses)
            {
                Move(x, y, ellipse);
            }
        }

        #endregion

        #region Move Texts

        public static void Move(double x, double y, XText text)
        {
            Canvas.SetLeft(text.Element as Grid, Canvas.GetLeft(text.Element as Grid) + x);
            Canvas.SetTop(text.Element as Grid, Canvas.GetTop(text.Element as Grid) + y);
        }

        public static void Move(double x, double y, IEnumerable<XText> texts)
        {
            foreach (var text in texts)
            {
                Move(x, y, text);
            }
        }

        #endregion

        #region Move Images

        public static void Move(double x, double y, XImage image)
        {
            Canvas.SetLeft(image.Element as Image, Canvas.GetLeft(image.Element as Image) + x);
            Canvas.SetTop(image.Element as Image, Canvas.GetTop(image.Element as Image) + y);
        }

        public static void Move(double x, double y, IEnumerable<XImage> images)
        {
            foreach (var image in images)
            {
                Move(x, y, image);
            }
        }

        #endregion

        #region Move Blocks

        public static void Move(double x, double y, XBlock block)
        {
            if (block.Points != null)
            {
                Move(x, y, block.Points);
            }

            if (block.Lines != null)
            {
                Move(x, y, block.Lines);
            }

            if (block.Rectangles != null)
            {
                Move(x, y, block.Rectangles);
            }

            if (block.Ellipses != null)
            {
                Move(x, y, block.Ellipses);
            }

            if (block.Texts != null)
            {
                Move(x, y, block.Texts);
            }

            if (block.Images != null)
            {
                Move(x, y, block.Images);
            }

            if (block.Blocks != null)
            {
                Move(x, y, block.Blocks);
            }
        }

        public static void Move(double x, double y, IEnumerable<XBlock> blocks)
        {
            foreach (var block in blocks)
            {
                Move(x, y, block.Points);
                Move(x, y, block.Lines);
                Move(x, y, block.Rectangles);
                Move(x, y, block.Ellipses);
                Move(x, y, block.Texts);
                Move(x, y, block.Images);
                Move(x, y, block.Blocks);
            }
        }

        #endregion

        #region Select

        public static int DeselectedZIndex = 0;
        public static int SelectedZIndex = 1;

        public static void Deselect(XPoint point)
        {
            //(point.Element as Ellipse).Stroke = BlockFactory.NormalBrush;
            //(point.Element as Ellipse).Fill = (point.Element as Ellipse).Fill == BlockFactory.TransparentBrush ? BlockFactory.TransparentBrush : BlockFactory.NormalBrush;
            FrameworkElementProperties.SetIsSelected(point.Element as Ellipse, false);
            //Panel.SetZIndex(point.Element as Ellipse, DeselectedZIndex);
        }

        public static void Deselect(XLine line)
        {
            (line.Element as Line).Stroke = BlockFactory.NormalBrush;
            Panel.SetZIndex((line.Element as Line), DeselectedZIndex);
        }

        public static void Deselect(XRectangle rectangle)
        {
            (rectangle.Element as Rectangle).Stroke = BlockFactory.NormalBrush;
            (rectangle.Element as Rectangle).Fill = (rectangle.Element as Rectangle).Fill == BlockFactory.TransparentBrush ? BlockFactory.TransparentBrush : BlockFactory.NormalBrush;
            Panel.SetZIndex(rectangle.Element as Rectangle, DeselectedZIndex);
        }

        public static void Deselect(XEllipse ellipse)
        {
            (ellipse.Element as Ellipse).Stroke = BlockFactory.NormalBrush;
            (ellipse.Element as Ellipse).Fill = (ellipse.Element as Ellipse).Fill == BlockFactory.TransparentBrush ? BlockFactory.TransparentBrush : BlockFactory.NormalBrush;
            Panel.SetZIndex(ellipse.Element as Ellipse, DeselectedZIndex);
        }

        public static void Deselect(XText text)
        {
            BlockFactory.GetTextBlock(text).Foreground = BlockFactory.NormalBrush;
            Panel.SetZIndex(text.Element as Grid, DeselectedZIndex);
        }

        public static void Deselect(XImage image)
        {
            (image.Element as Image).OpacityMask = BlockFactory.NormalBrush;
            Panel.SetZIndex(image.Element as Image, DeselectedZIndex);
        }

        public static void Deselect(XBlock parent)
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

        public static void Select(XPoint point)
        {
            //(point.Element as Ellipse).Stroke = BlockFactory.SelectedBrush;
            //(point.Element as Ellipse).Fill = (point.Element as Ellipse).Fill == BlockFactory.TransparentBrush ? BlockFactory.TransparentBrush : BlockFactory.SelectedBrush;
            FrameworkElementProperties.SetIsSelected(point.Element as Ellipse, true);
            //Panel.SetZIndex(point.Element as Ellipse, SelectedZIndex);
        }

        public static void Select(XLine line)
        {
            (line.Element as Line).Stroke = BlockFactory.SelectedBrush;
            Panel.SetZIndex(line.Element as Line, SelectedZIndex);
        }

        public static void Select(XRectangle rectangle)
        {
            (rectangle.Element as Rectangle).Stroke = BlockFactory.SelectedBrush;
            (rectangle.Element as Rectangle).Fill = (rectangle.Element as Rectangle).Fill == BlockFactory.TransparentBrush ? BlockFactory.TransparentBrush : BlockFactory.SelectedBrush;
            Panel.SetZIndex(rectangle.Element as Rectangle, SelectedZIndex);
        }

        public static void Select(XEllipse ellipse)
        {
            (ellipse.Element as Ellipse).Stroke = BlockFactory.SelectedBrush;
            (ellipse.Element as Ellipse).Fill = (ellipse.Element as Ellipse).Fill == BlockFactory.TransparentBrush ? BlockFactory.TransparentBrush : BlockFactory.SelectedBrush;
            Panel.SetZIndex(ellipse.Element as Ellipse, SelectedZIndex);
        }

        public static void Select(XText text)
        {
            BlockFactory.GetTextBlock(text).Foreground = BlockFactory.SelectedBrush;
            Panel.SetZIndex(text.Element as Grid, SelectedZIndex);
        }

        public static void Select(XImage image)
        {
            (image.Element as Image).OpacityMask = BlockFactory.SelectedBrush;
            Panel.SetZIndex(image.Element as Image, SelectedZIndex);
        }

        public static void Select(XBlock parent)
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

        public static void SelectContent(XBlock selected, XBlock content)
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

        public static void DeselectContent(XBlock selected)
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

        public static bool HaveSelected(XBlock selected)
        {
            return (selected.Points != null
                || selected.Lines != null
                || selected.Rectangles != null
                || selected.Ellipses != null
                || selected.Texts != null
                || selected.Images != null
                || selected.Blocks != null);
        }

        public static bool HaveOnePointSelected(XBlock selected)
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

        public static bool HaveOneLineSelected(XBlock selected)
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

        public static bool HaveOneRectangleSelected(XBlock selected)
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

        public static bool HaveOneEllipseSelected(XBlock selected)
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

        public static bool HaveOneTextSelected(XBlock selected)
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

        public static bool HaveOneImageSelected(XBlock selected)
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

        public static bool HaveOneBlockSelected(XBlock selected)
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

        public static bool HitTest(IEnumerable<XPoint> points, XBlock selected, Rect rect, bool onlyFirst, bool select, object relativeTo)
        {
            foreach (var point in points)
            {
                var bounds = WpfVisualHelper.GetContentBounds(point, relativeTo);
                if (rect.IntersectsWith(bounds))
                {
                    if (select)
                    {
                        //if ((point.Element as Ellipse).Stroke != BlockFactory.SelectedBrush)
                        if (!FrameworkElementProperties.GetIsSelected((point.Element as Ellipse)))
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

        public static bool HitTest(IEnumerable<XLine> lines, XBlock selected, Rect rect, bool onlyFirst, bool select)
        {
            foreach (var line in lines)
            {
                var bounds = WpfVisualHelper.GetContentBounds(line);
                if (rect.IntersectsWith(bounds))
                {
                    if (select)
                    {
                        if ((line.Element as Line).Stroke != BlockFactory.SelectedBrush)
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

        public static bool HitTest(IEnumerable<XRectangle> rectangles, XBlock selected, Rect rect, bool onlyFirst, bool select, object relativeTo)
        {
            foreach (var rectangle in rectangles)
            {
                var bounds = WpfVisualHelper.GetContentBounds(rectangle, relativeTo);
                if (rect.IntersectsWith(bounds))
                {
                    if (select)
                    {
                        if ((rectangle.Element as Rectangle).Stroke != BlockFactory.SelectedBrush)
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

        public static bool HitTest(IEnumerable<XEllipse> ellipses, XBlock selected, Rect rect, bool onlyFirst, bool select, object relativeTo)
        {
            foreach (var ellipse in ellipses)
            {
                var bounds = WpfVisualHelper.GetContentBounds(ellipse, relativeTo);
                if (rect.IntersectsWith(bounds))
                {
                    if (select)
                    {
                        if ((ellipse.Element as Ellipse).Stroke != BlockFactory.SelectedBrush)
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

        public static bool HitTest(IEnumerable<XText> texts, XBlock selected, Rect rect, bool onlyFirst, bool select, object relativeTo)
        {
            foreach (var text in texts)
            {
                var bounds = WpfVisualHelper.GetContentBounds(text, relativeTo);
                if (rect.IntersectsWith(bounds))
                {
                    if (select)
                    {
                        var tb = BlockFactory.GetTextBlock(text);
                        if (tb.Foreground != BlockFactory.SelectedBrush)
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

        public static bool HitTest(IEnumerable<XImage> images, XBlock selected, Rect rect, bool onlyFirst, bool select, object relativeTo)
        {
            foreach (var image in images)
            {
                var bounds = WpfVisualHelper.GetContentBounds(image, relativeTo);
                if (rect.IntersectsWith(bounds))
                {
                    if (select)
                    {
                        if ((image.Element as Image).OpacityMask != BlockFactory.SelectedBrush)
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

        public static bool HitTest(IEnumerable<XBlock> blocks, XBlock selected, Rect rect, bool onlyFirst, bool select, bool selectInsideBlock, object relativeTo)
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

        public static bool HitTest(XBlock parent, XBlock selected, Rect rect, bool onlyFirst, bool selectInsideBlock, object relativeTo)
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

        public static bool HitTestClick(ISheet sheet, XBlock parent, XBlock selected, Point p, double size, bool selectInsideBlock, bool resetSelected)
        {
            if (resetSelected)
            {
                selected.Init();
            }
            else
            {
                selected.ReInit();
            }

            var rect = new Rect(p.X - size, p.Y - size, 2 * size, 2 * size);

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

        public static bool HitTestForBlocks(ISheet sheet, XBlock parent, XBlock selected, Point p, double size)
        {
            selected.Init();

            var rect = new Rect(p.X - size, p.Y - size, 2 * size, 2 * size);

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

        public static void HitTestSelectionRect(ISheet sheet, XBlock parent, XBlock selected, Rect rect, bool resetSelected)
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

        private static void HitTestClean(XBlock selected)
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

        #endregion

        #region Fill

        public static void ToggleFill(XRectangle rectangle)
        {
            (rectangle.Element as Rectangle).Fill = (rectangle.Element as Rectangle).Fill == BlockFactory.TransparentBrush ? BlockFactory.NormalBrush : BlockFactory.TransparentBrush;
        }

        public static void ToggleFill(XEllipse ellipse)
        {
            (ellipse.Element as Ellipse).Fill = (ellipse.Element as Ellipse).Fill == BlockFactory.TransparentBrush ? BlockFactory.NormalBrush : BlockFactory.TransparentBrush;
        }

        public static void ToggleFill(XPoint point)
        {
            (point.Element as Ellipse).Fill = (point.Element as Ellipse).Fill == BlockFactory.TransparentBrush ? BlockFactory.NormalBrush : BlockFactory.TransparentBrush;
        }

        #endregion

        #region Copy

        public static XBlock ShallowCopy(XBlock original)
        {
            var copy = new XBlock(original.Id, original.X, original.Y, original.Width, original.Height, original.DataId, original.Name);

            if (original.Points != null)
            {
                copy.Points = new List<XPoint>(original.Points);
            }

            if (original.Lines != null)
            {
                copy.Lines = new List<XLine>(original.Lines);
            }

            if (original.Rectangles != null)
            {
                copy.Rectangles = new List<XRectangle>(original.Rectangles);
            }

            if (original.Ellipses != null)
            {
                copy.Ellipses = new List<XEllipse>(original.Ellipses);
            }

            if (original.Texts != null)
            {
                copy.Texts = new List<XText>(original.Texts);
            }

            if (original.Images != null)
            {
                copy.Images = new List<XImage>(original.Images);
            }

            if (original.Blocks != null)
            {
                copy.Blocks = new List<XBlock>(original.Blocks);
            }

            if (original.Points != null)
            {
                copy.Points = new List<XPoint>(original.Points);
            }

            return copy;
        }

        #endregion
    }

    #endregion

    #region Block Factory

    public static class BlockFactory
    {
        #region Brushes

        public static SolidColorBrush NormalBrush = Brushes.Black;
        public static SolidColorBrush SelectedBrush = Brushes.Red;
        public static SolidColorBrush HoverBrush = Brushes.Yellow;
        public static SolidColorBrush TransparentBrush = Brushes.Transparent;

        #endregion

        #region Styles

        private static void SetStyle(Ellipse ellipse, bool isVisible)
        {
            var style = new Style(typeof(Ellipse));
            style.Setters.Add(new Setter(Ellipse.FillProperty, isVisible ? NormalBrush : TransparentBrush));
            style.Setters.Add(new Setter(Ellipse.StrokeProperty, isVisible ? NormalBrush : TransparentBrush));

            var isSelectedTrigger = new Trigger() { Property = FrameworkElementProperties.IsSelectedProperty, Value = true };
            isSelectedTrigger.Setters.Add(new Setter(Ellipse.FillProperty, SelectedBrush));
            isSelectedTrigger.Setters.Add(new Setter(Ellipse.StrokeProperty, SelectedBrush));
            style.Triggers.Add(isSelectedTrigger);

            var isMouseOverTrigger = new Trigger() { Property = Ellipse.IsMouseOverProperty, Value = true };
            isMouseOverTrigger.Setters.Add(new Setter(Ellipse.FillProperty, HoverBrush));
            isMouseOverTrigger.Setters.Add(new Setter(Ellipse.StrokeProperty, HoverBrush));
            style.Triggers.Add(isMouseOverTrigger);

            ellipse.Style = style;

            FrameworkElementProperties.SetIsSelected(ellipse, false);
        }

        #endregion

        #region Create

        public static XPoint CreatePoint(double thickness, double x, double y, bool isVisible)
        {
            var ellipse = new Ellipse()
            {
                StrokeThickness = thickness,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                Width = 8.0,
                Height = 8.0,
                Margin = new Thickness(-4.0, -4.0, 0.0, 0.0),
            };

            SetStyle(ellipse, isVisible);
            Panel.SetZIndex(ellipse, BlockController.SelectedZIndex);

            Canvas.SetLeft(ellipse, x);
            Canvas.SetTop(ellipse, y);

            var xpoint = new XPoint(ellipse, x, y, isVisible);

            return xpoint;
        }

        public static XLine CreateLine(double thickness, double x1, double y1, double x2, double y2, ItemColor stroke)
        {
            var strokeBrush = new SolidColorBrush(Color.FromArgb(stroke.Alpha, stroke.Red, stroke.Green, stroke.Blue));

            strokeBrush.Freeze();

            var line = new Line()
            {
                Stroke = strokeBrush,
                StrokeThickness = thickness,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2
            };

            var xline = new XLine(line);

            return xline;
        }

        public static XLine CreateLine(double thickness, XPoint start, XPoint end, ItemColor stroke)
        {
            var xline = CreateLine(thickness, start.X, start.Y, end.X, end.Y, stroke);
            xline.Start = start;
            xline.End = end;
            return xline;
        }

        public static XRectangle CreateRectangle(double thickness, double x, double y, double width, double height, bool isFilled)
        {
            var rectangle = new Rectangle()
            {
                Fill = isFilled ? NormalBrush : TransparentBrush,
                Stroke = NormalBrush,
                StrokeThickness = thickness,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                Width = width,
                Height = height
            };

            Canvas.SetLeft(rectangle, x);
            Canvas.SetTop(rectangle, y);

            var xrectangle = new XRectangle(rectangle);

            return xrectangle;
        }

        public static XEllipse CreateEllipse(double thickness, double x, double y, double width, double height, bool isFilled)
        {
            var ellipse = new Ellipse()
            {
                Fill = isFilled ? NormalBrush : TransparentBrush,
                Stroke = NormalBrush,
                StrokeThickness = thickness,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                Width = width,
                Height = height
            };

            Canvas.SetLeft(ellipse, x);
            Canvas.SetTop(ellipse, y);

            var xellipse = new XEllipse(ellipse);

            return xellipse;
        }

        public static TextBlock GetTextBlock(XText text)
        {
            return (text.Element as Grid).Children[0] as TextBlock;
        }

        public static XText CreateText(string text,
            double x, double y, 
            double width, double height,
            HorizontalAlignment halign, VerticalAlignment valign,
            double fontSize,
            ItemColor backgroud, ItemColor foreground)
        {
            var backgroundBrush = new SolidColorBrush(Color.FromArgb(backgroud.Alpha, backgroud.Red, backgroud.Green, backgroud.Blue));
            var foregroundBrush = new SolidColorBrush(Color.FromArgb(foreground.Alpha, foreground.Red, foreground.Green, foreground.Blue));

            backgroundBrush.Freeze();
            foregroundBrush.Freeze();

            var grid = new Grid();
            grid.Background = backgroundBrush;
            grid.Width = width;
            grid.Height = height;
            Canvas.SetLeft(grid, x);
            Canvas.SetTop(grid, y);

            var tb = new TextBlock();
            tb.HorizontalAlignment = halign;
            tb.VerticalAlignment = valign;
            tb.Background = backgroundBrush;
            tb.Foreground = foregroundBrush;
            tb.FontSize = fontSize;
            tb.FontFamily = new FontFamily("Calibri");
            tb.Text = text;

            grid.Children.Add(tb);

            var xtext = new XText(grid);

            return xtext;
        }

        public static XImage CreateImage(double x, double y, double width, double height, byte[] data)
        {
            Image image = new Image();

            // enable high quality image scaling
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);

            // store original image data is Tag property
            image.Tag = data;

            // opacity mask is used for determining selection state
            image.OpacityMask = NormalBrush;

            //using(var ms = new System.IO.MemoryStream(data))
            //{
            //    image = Image.FromStream(ms);
            //}
            using (var ms = new System.IO.MemoryStream(data))
            {
                IBitmap profileImage = BitmapLoader.Current.Load(ms, null, null).Result;
                image.Source = profileImage.ToNative();
            }

            image.Width = width;
            image.Height = height;

            Canvas.SetLeft(image, x);
            Canvas.SetTop(image, y);

            var ximage = new XImage(image);

            return ximage;
        }

        #endregion
    }

    #endregion
    
    #region PointController

    public static class PointController
    {
         public static void ConnectStart(XPoint point, XLine line)
        {
            var dependecy = new XDependency(line, (element, p) => { (element.Element as Line).X1 = p.X; (element.Element as Line).Y1 = p.Y; });
            point.Connected.Add(dependecy);
        }

        public static void ConnectEnd(XPoint point, XLine line)
        {
            var dependecy = new XDependency(line, (element, p) => { (element.Element as Line).X2 = p.X; (element.Element as Line).Y2 = p.Y; });
            point.Connected.Add(dependecy);
        }
    }
    
    #endregion
}
