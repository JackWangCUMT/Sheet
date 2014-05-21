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
        public object Element { get; set; }
    }

    public class XPoint : XElement
    {
        public double X { get; set; }
        public double Y { get; set; }
        public bool IsVisible { get; set; }
        public XPoint(object element, double x, double y, bool isVisible)
        {
            X = x;
            Y = y;
            IsVisible = isVisible;
        }
    }

    public class XThumb : XElement
    {
        public XThumb(object element)
        {
            Element = element;
        }
    }

    public class XLine : XElement
    {
        public XPoint Start { get; set; }
        public XPoint End { get; set; }
        public XLine(object element)
        {
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

        public static PointItem SerializePoint(XPoint point)
        {
            var pointItem = new PointItem();

            pointItem.Id = 0;
            pointItem.X = Canvas.GetLeft(point.Element as FrameworkElement);
            pointItem.Y = Canvas.GetTop(point.Element as FrameworkElement);

            return pointItem;
        }

        public static LineItem SerializeLine(XLine line)
        {
            var lineItem = new LineItem();

            lineItem.Id = 0;
            lineItem.X1 = (line.Element as Line).X1;
            lineItem.Y1 = (line.Element as Line).Y1;
            lineItem.X2 = (line.Element as Line).X2;
            lineItem.Y2 = (line.Element as Line).Y2;
            lineItem.Stroke = ToItemColor((line.Element as Line).Stroke);

            return lineItem;
        }

        public static RectangleItem SerializeRectangle(XRectangle rectangle)
        {
            var rectangleItem = new RectangleItem();

            rectangleItem.Id = 0;
            rectangleItem.X = Canvas.GetLeft(rectangle.Element as Rectangle);
            rectangleItem.Y = Canvas.GetTop(rectangle.Element as Rectangle);
            rectangleItem.Width = (rectangle.Element as Rectangle).Width;
            rectangleItem.Height = (rectangle.Element as Rectangle).Height;
            rectangleItem.IsFilled = (rectangle.Element as Rectangle).Fill == BlockFactory.TransparentBrush ? false : true;
            rectangleItem.Stroke = ToItemColor((rectangle.Element as Rectangle).Stroke);
            rectangleItem.Fill = ToItemColor((rectangle.Element as Rectangle).Fill);

            return rectangleItem;
        }

        public static EllipseItem SerializeEllipse(XEllipse ellipse)
        {
            var ellipseItem = new EllipseItem();

            ellipseItem.Id = 0;
            ellipseItem.X = Canvas.GetLeft(ellipse.Element as Ellipse);
            ellipseItem.Y = Canvas.GetTop(ellipse.Element as Ellipse);
            ellipseItem.Width = (ellipse.Element as Ellipse).Width;
            ellipseItem.Height = (ellipse.Element as Ellipse).Height;
            ellipseItem.IsFilled = (ellipse.Element as Ellipse).Fill == BlockFactory.TransparentBrush ? false : true;
            ellipseItem.Stroke = ToItemColor((ellipse.Element as Ellipse).Stroke);
            ellipseItem.Fill = ToItemColor((ellipse.Element as Ellipse).Fill);

            return ellipseItem;
        }

        public static TextItem SerializeText(XText text)
        {
            var textItem = new TextItem();

            textItem.Id = 0;
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

        public static ImageItem SerializeImage(XImage image)
        {
            var imageItem = new ImageItem();

            imageItem.Id = 0;
            imageItem.X = Canvas.GetLeft(image.Element as Image);
            imageItem.Y = Canvas.GetTop(image.Element as Image);
            imageItem.Width = (image.Element as Image).Width;
            imageItem.Height = (image.Element as Image).Height;
            imageItem.Data = (image.Element as Image).Tag as byte[];

            return imageItem;
        }

        public static BlockItem SerializeBlock(XBlock parent)
        {
            var blockItem = new BlockItem();
            blockItem.Init(0, parent.X, parent.Y, parent.Width, parent.Height, parent.DataId, parent.Name);
            blockItem.Backgroud = ToItemColor(parent.Backgroud);

            foreach (var point in parent.Points)
            {
                blockItem.Points.Add(SerializePoint(point));
            }

            foreach (var line in parent.Lines)
            {
                blockItem.Lines.Add(SerializeLine(line));
            }

            foreach (var rectangle in parent.Rectangles)
            {
                blockItem.Rectangles.Add(SerializeRectangle(rectangle));
            }

            foreach (var ellipse in parent.Ellipses)
            {
                blockItem.Ellipses.Add(SerializeEllipse(ellipse));
            }

            foreach (var text in parent.Texts)
            {
                blockItem.Texts.Add(SerializeText(text));
            }

            foreach (var image in parent.Images)
            {
                blockItem.Images.Add(SerializeImage(image));
            }

            foreach (var block in parent.Blocks)
            {
                blockItem.Blocks.Add(SerializeBlock(block));
            }

            return blockItem;
        }

        public static BlockItem SerializerBlockContents(XBlock parent, int id, double x, double y, double width, double height, int dataId, string name)
        {
            var points = parent.Points;
            var lines = parent.Lines;
            var rectangles = parent.Rectangles;
            var ellipses = parent.Ellipses;
            var texts = parent.Texts;
            var images = parent.Images;
            var blocks = parent.Blocks;

            var sheet = new BlockItem() { Backgroud = new ItemColor() };
            sheet.Init(id, x, y, width, height, dataId, name);

            if (points != null)
            {
                foreach (var point in points)
                {
                    sheet.Points.Add(SerializePoint(point));
                }
            }

            if (lines != null)
            {
                foreach (var line in lines)
                {
                    sheet.Lines.Add(SerializeLine(line));
                }
            }

            if (rectangles != null)
            {
                foreach (var rectangle in rectangles)
                {
                    sheet.Rectangles.Add(SerializeRectangle(rectangle));
                }
            }

            if (ellipses != null)
            {
                foreach (var ellipse in ellipses)
                {
                    sheet.Ellipses.Add(SerializeEllipse(ellipse));
                }
            }

            if (texts != null)
            {
                foreach (var text in texts)
                {
                    sheet.Texts.Add(SerializeText(text));
                }
            }

            if (images != null)
            {
                foreach (var image in images)
                {
                    sheet.Images.Add(SerializeImage(image));
                }
            }

            if (blocks != null)
            {
                foreach (var block in blocks)
                {
                    sheet.Blocks.Add(SerializeBlock(block));
                }
            }

            return sheet;
        }

        #endregion

        #region Deserialize

        public static XPoint DeserializePointItem(ISheet sheet, XBlock parent, PointItem pointItem, double thickness)
        {
            var point = BlockFactory.CreatePoint(thickness, pointItem.X, pointItem.Y, false);

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

        public static XLine DeserializeLineItem(ISheet sheet, XBlock parent, LineItem lineItem, double thickness)
        {
            var line = BlockFactory.CreateLine(thickness, lineItem.X1, lineItem.Y1, lineItem.X2, lineItem.Y2, lineItem.Stroke);

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

        public static XRectangle DeserializeRectangleItem(ISheet sheet, XBlock parent, RectangleItem rectangleItem, double thickness)
        {
            var rectangle = BlockFactory.CreateRectangle(thickness, rectangleItem.X, rectangleItem.Y, rectangleItem.Width, rectangleItem.Height, rectangleItem.IsFilled);

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

        public static XEllipse DeserializeEllipseItem(ISheet sheet, XBlock parent, EllipseItem ellipseItem, double thickness)
        {
            var ellipse = BlockFactory.CreateEllipse(thickness, ellipseItem.X, ellipseItem.Y, ellipseItem.Width, ellipseItem.Height, ellipseItem.IsFilled);

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

        public static XText DeserializeTextItem(ISheet sheet, XBlock parent, TextItem textItem)
        {
            var text = BlockFactory.CreateText(textItem.Text,
                textItem.X, textItem.Y,
                textItem.Width, textItem.Height,
                (HorizontalAlignment)textItem.HAlign,
                (VerticalAlignment)textItem.VAlign,
                textItem.Size,
                textItem.Backgroud,
                textItem.Foreground);

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

        public static XImage DeserializeImageItem(ISheet sheet, XBlock parent, ImageItem imageItem)
        {
            var image = BlockFactory.CreateImage(imageItem.X, imageItem.Y, imageItem.Width, imageItem.Height, imageItem.Data);

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

        public static XBlock DeserializeBlockItem(ISheet sheet, XBlock parent, BlockItem blockItem, bool select, double thickness)
        {
            var block = new XBlock(blockItem.Id, blockItem.X, blockItem.Y, blockItem.Width, blockItem.Height, blockItem.DataId, blockItem.Name);
            block.Init();

            foreach (var textItem in blockItem.Texts)
            {
                var text = DeserializeTextItem(sheet, block, textItem);

                if (select)
                {
                    BlockController.SelectText(text);
                }
            }

            foreach (var imageItem in blockItem.Images)
            {
                var image = DeserializeImageItem(sheet, block, imageItem);

                if (select)
                {
                    BlockController.SelectImage(image);
                }
            }

            foreach (var lineItem in blockItem.Lines)
            {
                var line = DeserializeLineItem(sheet, block, lineItem, thickness);

                if (select)
                {
                    BlockController.SelectLine(line);
                }
            }

            foreach (var rectangleItem in blockItem.Rectangles)
            {
                var rectangle = DeserializeRectangleItem(sheet, block, rectangleItem, thickness);

                if (select)
                {
                    BlockController.SelectRectangle(rectangle);
                }
            }

            foreach (var ellipseItem in blockItem.Ellipses)
            {
                var ellipse = DeserializeEllipseItem(sheet, block, ellipseItem, thickness);

                if (select)
                {
                    BlockController.SelectEllipse(ellipse);
                }
            }

            foreach (var childBlockItem in blockItem.Blocks)
            {
                DeserializeBlockItem(sheet, block, childBlockItem, select, thickness);
            }

            foreach (var pointItem in blockItem.Points)
            {
                var point = DeserializePointItem(sheet, block, pointItem, thickness);

                if (select)
                {
                    BlockController.SelectPoint(point);
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
        #region Add


        public static void AddLines(ISheet sheet, IEnumerable<LineItem> lineItems, XBlock parent, XBlock selected, bool select, double thickness)
        {
            if (select)
            {
                selected.Lines = new List<XLine>();
            }

            foreach (var lineItem in lineItems)
            {
                var line = BlockSerializer.DeserializeLineItem(sheet, parent, lineItem, thickness);

                if (select)
                {
                    SelectLine(line);
                    selected.Lines.Add(line);
                }
            }
        }

        public static void AddRectangles(ISheet sheet, IEnumerable<RectangleItem> rectangleItems, XBlock parent, XBlock selected, bool select, double thickness)
        {
            if (select)
            {
                selected.Rectangles = new List<XRectangle>();
            }

            foreach (var rectangleItem in rectangleItems)
            {
                var rectangle = BlockSerializer.DeserializeRectangleItem(sheet, parent, rectangleItem, thickness);

                if (select)
                {
                    SelectRectangle(rectangle);
                    selected.Rectangles.Add(rectangle);
                }
            }
        }

        public static void AddEllipses(ISheet sheet, IEnumerable<EllipseItem> ellipseItems, XBlock parent, XBlock selected, bool select, double thickness)
        {
            if (select)
            {
                selected.Ellipses = new List<XEllipse>();
            }

            foreach (var ellipseItem in ellipseItems)
            {
                var ellipse = BlockSerializer.DeserializeEllipseItem(sheet, parent, ellipseItem, thickness);

                if (select)
                {
                    SelectEllipse(ellipse);
                    selected.Ellipses.Add(ellipse);
                }
            }
        }

        public static void AddTexts(ISheet sheet, IEnumerable<TextItem> textItems, XBlock parent, XBlock selected, bool select, double thickness)
        {
            if (select)
            {
                selected.Texts = new List<XText>();
            }

            foreach (var textItem in textItems)
            {
                var text = BlockSerializer.DeserializeTextItem(sheet, parent, textItem);

                if (select)
                {
                    SelectText(text);
                    selected.Texts.Add(text);
                }
            }
        }

        public static void AddImages(ISheet sheet, IEnumerable<ImageItem> imageItems, XBlock parent, XBlock selected, bool select, double thickness)
        {
            if (select)
            {
                selected.Images = new List<XImage>();
            }

            foreach (var imageItem in imageItems)
            {
                var image = BlockSerializer.DeserializeImageItem(sheet, parent, imageItem);

                if (select)
                {
                    SelectImage(image);
                    selected.Images.Add(image);
                }
            }
        }

        public static void AddBlocks(ISheet sheet, IEnumerable<BlockItem> blockItems, XBlock parent, XBlock selected, bool select, double thickness)
        {
            if (select)
            {
                selected.Blocks = new List<XBlock>();
            }

            foreach (var blockItem in blockItems)
            {
                var block = BlockSerializer.DeserializeBlockItem(sheet, parent, blockItem, select, thickness);

                if (select)
                {
                    selected.Blocks.Add(block);
                }
            }
        }

        public static void AddBlockContents(ISheet sheet, BlockItem blockItem, XBlock content, XBlock selected, bool select, double thickness)
        {
            if (blockItem != null)
            {
                AddTexts(sheet, blockItem.Texts, content, selected, select, thickness);
                AddImages(sheet, blockItem.Images, content, selected, select, thickness);
                AddLines(sheet, blockItem.Lines, content, selected, select, thickness);
                AddRectangles(sheet, blockItem.Rectangles, content, selected, select, thickness);
                AddEllipses(sheet, blockItem.Ellipses, content, selected, select, thickness);
                AddBlocks(sheet, blockItem.Blocks, content, selected, select, thickness);
            }
        }

        public static void AddBrokenBlock(ISheet sheet, BlockItem blockItem, XBlock content, XBlock selected, bool select, double thickness)
        {
            AddTexts(sheet, blockItem.Texts, content, selected, select, thickness);
            AddImages(sheet, blockItem.Images, content, selected, select, thickness);
            AddLines(sheet, blockItem.Lines, content, selected, select, thickness);
            AddRectangles(sheet, blockItem.Rectangles, content, selected, select, thickness);
            AddEllipses(sheet, blockItem.Ellipses, content, selected, select, thickness);

            foreach (var block in blockItem.Blocks)
            {
                AddTexts(sheet, block.Texts, content, selected, select, thickness);
                AddImages(sheet, block.Images, content, selected, select, thickness);
                AddLines(sheet, block.Lines, content, selected, select, thickness);
                AddRectangles(sheet, block.Rectangles, content, selected, select, thickness);
                AddEllipses(sheet, block.Ellipses, content, selected, select, thickness);
                AddBlocks(sheet, block.Blocks, content, selected, select, thickness);
            }
        }

        #endregion

        #region Remove

        public static void RemoveLines(ISheet sheet, IEnumerable<XLine> lines)
        {
            if (lines != null)
            {
                foreach (var line in lines)
                {
                    sheet.Remove(line);
                }
            }
        }

        public static void RemoveRectangles(ISheet sheet, IEnumerable<XRectangle> rectangles)
        {
            if (rectangles != null)
            {
                foreach (var rectangle in rectangles)
                {
                    sheet.Remove(rectangle);
                }
            }
        }

        public static void RemoveEllipses(ISheet sheet, IEnumerable<XEllipse> ellipses)
        {
            if (ellipses != null)
            {
                foreach (var ellipse in ellipses)
                {
                    sheet.Remove(ellipse);
                }
            }
        }

        public static void RemoveTexts(ISheet sheet, IEnumerable<XText> texts)
        {
            if (texts != null)
            {
                foreach (var text in texts)
                {
                    sheet.Remove(text);
                }
            }
        }

        public static void RemoveImages(ISheet sheet, IEnumerable<XImage> images)
        {
            if (images != null)
            {
                foreach (var image in images)
                {
                    sheet.Remove(image);
                }
            }
        }

        public static void RemoveBlocks(ISheet sheet, IEnumerable<XBlock> blocks)
        {
            if (blocks != null)
            {
                foreach (var block in blocks)
                {
                    RemoveBlock(sheet, block);
                }
            }
        }

        public static void RemoveBlock(ISheet sheet, XBlock block)
        {
            RemoveLines(sheet, block.Lines);
            RemoveRectangles(sheet, block.Rectangles);
            RemoveEllipses(sheet, block.Ellipses);
            RemoveTexts(sheet, block.Texts);
            RemoveImages(sheet, block.Images);
            RemoveBlocks(sheet, block.Blocks);
        }

        public static void RemoveSelectedFromBlock(ISheet sheet, XBlock parent, XBlock selected)
        {
            if (selected.Lines != null)
            {
                RemoveLines(sheet, selected.Lines);

                foreach (var line in selected.Lines)
                {
                    parent.Lines.Remove(line);
                }

                selected.Lines = null;
            }

            if (selected.Rectangles != null)
            {
                RemoveRectangles(sheet, selected.Rectangles);

                foreach (var rectangle in selected.Rectangles)
                {
                    parent.Rectangles.Remove(rectangle);
                }

                selected.Rectangles = null;
            }

            if (selected.Ellipses != null)
            {
                RemoveEllipses(sheet, selected.Ellipses);

                foreach (var ellipse in selected.Ellipses)
                {
                    parent.Ellipses.Remove(ellipse);
                }

                selected.Ellipses = null;
            }

            if (selected.Texts != null)
            {
                RemoveTexts(sheet, selected.Texts);

                foreach (var text in selected.Texts)
                {
                    parent.Texts.Remove(text);
                }

                selected.Texts = null;
            }

            if (selected.Images != null)
            {
                RemoveImages(sheet, selected.Images);

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
                    RemoveLines(sheet, block.Lines);
                    RemoveRectangles(sheet, block.Rectangles);
                    RemoveEllipses(sheet, block.Ellipses);
                    RemoveTexts(sheet, block.Texts);
                    RemoveImages(sheet, block.Images);
                    RemoveBlocks(sheet, block.Blocks);

                    parent.Blocks.Remove(block);
                }

                selected.Blocks = null;
            }
        }

        #endregion

        #region Move

        public static void MoveLines(double x, double y, IEnumerable<XLine> lines)
        {
            foreach (var line in lines)
            {
                (line.Element as Line).X1 += x;
                (line.Element as Line).Y1 += y;
                (line.Element as Line).X2 += x;
                (line.Element as Line).Y2 += y;
            }
        }

        public static void MoveRectangles(double x, double y, IEnumerable<XRectangle> rectangles)
        {
            foreach (var rectangle in rectangles)
            {
                Canvas.SetLeft(rectangle.Element as Rectangle, Canvas.GetLeft(rectangle.Element as Rectangle) + x);
                Canvas.SetTop(rectangle.Element as Rectangle, Canvas.GetTop(rectangle.Element as Rectangle) + y);
            }
        }

        public static void MoveEllipses(double x, double y, IEnumerable<XEllipse> ellipses)
        {
            foreach (var ellipse in ellipses)
            {
                Canvas.SetLeft(ellipse.Element as Ellipse, Canvas.GetLeft(ellipse.Element as Ellipse) + x);
                Canvas.SetTop(ellipse.Element as Ellipse, Canvas.GetTop(ellipse.Element as Ellipse) + y);
            }
        }

        public static void MoveTexts(double x, double y, IEnumerable<XText> texts)
        {
            foreach (var text in texts)
            {
                Canvas.SetLeft(text.Element as Grid, Canvas.GetLeft(text.Element as Grid) + x);
                Canvas.SetTop(text.Element as Grid, Canvas.GetTop(text.Element as Grid) + y);
            }
        }

        public static void MoveImages(double x, double y, IEnumerable<XImage> images)
        {
            foreach (var image in images)
            {
                Canvas.SetLeft(image.Element as Image, Canvas.GetLeft(image.Element as Image) + x);
                Canvas.SetTop(image.Element as Image, Canvas.GetTop(image.Element as Image) + y);
            }
        }

        public static void MoveBlocks(double x, double y, IEnumerable<XBlock> blocks)
        {
            foreach (var block in blocks)
            {
                MoveLines(x, y, block.Lines);
                MoveRectangles(x, y, block.Rectangles);
                MoveEllipses(x, y, block.Ellipses);
                MoveTexts(x, y, block.Texts);
                MoveImages(x, y, block.Images);
                MoveBlocks(x, y, block.Blocks);
            }
        }

        public static void Move(double x, double y, XBlock block)
        {
            if (block.Lines != null)
            {
                MoveLines(x, y, block.Lines);
            }

            if (block.Rectangles != null)
            {
                MoveRectangles(x, y, block.Rectangles);
            }

            if (block.Ellipses != null)
            {
                MoveEllipses(x, y, block.Ellipses);
            }

            if (block.Texts != null)
            {
                MoveTexts(x, y, block.Texts);
            }

            if (block.Images != null)
            {
                MoveImages(x, y, block.Images);
            }

            if (block.Blocks != null)
            {
                MoveBlocks(x, y, block.Blocks);
            }
        }

        #endregion

        #region Select

        private static int DeselectedZIndex = 0;
        private static int SelectedZIndex = 1;

        public static void DeselectPoint(XPoint point)
        {
            (point.Element as Ellipse).Stroke = BlockFactory.NormalBrush;
            (point.Element as Ellipse).Fill = (point.Element as Ellipse).Fill == BlockFactory.TransparentBrush ? BlockFactory.TransparentBrush : BlockFactory.NormalBrush;
            Panel.SetZIndex(point.Element as Ellipse, DeselectedZIndex);
        }

        public static void DeselectLine(XLine line)
        {
            (line.Element as Line).Stroke = BlockFactory.NormalBrush;
            Panel.SetZIndex((line.Element as Line), DeselectedZIndex);
        }

        public static void DeselectRectangle(XRectangle rectangle)
        {
            (rectangle.Element as Rectangle).Stroke = BlockFactory.NormalBrush;
            (rectangle.Element as Rectangle).Fill = (rectangle.Element as Rectangle).Fill == BlockFactory.TransparentBrush ? BlockFactory.TransparentBrush : BlockFactory.NormalBrush;
            Panel.SetZIndex(rectangle.Element as Rectangle, DeselectedZIndex);
        }

        public static void DeselectEllipse(XEllipse ellipse)
        {
            (ellipse.Element as Ellipse).Stroke = BlockFactory.NormalBrush;
            (ellipse.Element as Ellipse).Fill = (ellipse.Element as Ellipse).Fill == BlockFactory.TransparentBrush ? BlockFactory.TransparentBrush : BlockFactory.NormalBrush;
            Panel.SetZIndex(ellipse.Element as Ellipse, DeselectedZIndex);
        }

        public static void DeselectText(XText text)
        {
            BlockFactory.GetTextBlock(text).Foreground = BlockFactory.NormalBrush;
            Panel.SetZIndex(text.Element as Grid, DeselectedZIndex);
        }

        public static void DeselectImage(XImage image)
        {
            (image.Element as Image).OpacityMask = BlockFactory.NormalBrush;
            Panel.SetZIndex(image.Element as Image, DeselectedZIndex);
        }

        public static void DeselectBlock(XBlock parent)
        {
            if (parent.Points != null)
            {
                foreach (var point in parent.Points)
                {
                    DeselectPoint(point);
                }
            }

            if (parent.Lines != null)
            {
                foreach (var line in parent.Lines)
                {
                    DeselectLine(line);
                }
            }

            if (parent.Rectangles != null)
            {
                foreach (var rectangle in parent.Rectangles)
                {
                    DeselectRectangle(rectangle);
                }
            }

            if (parent.Ellipses != null)
            {
                foreach (var ellipse in parent.Ellipses)
                {
                    DeselectEllipse(ellipse);
                }
            }

            if (parent.Texts != null)
            {
                foreach (var text in parent.Texts)
                {
                    DeselectText(text);
                }
            }

            if (parent.Images != null)
            {
                foreach (var image in parent.Images)
                {
                    DeselectImage(image);
                }
            }

            if (parent.Blocks != null)
            {
                foreach (var block in parent.Blocks)
                {
                    DeselectBlock(block);
                }
            }
        }

        public static void SelectPoint(XPoint point)
        {
            (point.Element as Ellipse).Stroke = BlockFactory.SelectedBrush;
            (point.Element as Ellipse).Fill = (point.Element as Ellipse).Fill == BlockFactory.TransparentBrush ? BlockFactory.TransparentBrush : BlockFactory.SelectedBrush;
            Panel.SetZIndex(point.Element as Ellipse, SelectedZIndex);
        }

        public static void SelectLine(XLine line)
        {
            (line.Element as Line).Stroke = BlockFactory.SelectedBrush;
            Panel.SetZIndex(line.Element as Line, SelectedZIndex);
        }

        public static void SelectRectangle(XRectangle rectangle)
        {
            (rectangle.Element as Rectangle).Stroke = BlockFactory.SelectedBrush;
            (rectangle.Element as Rectangle).Fill = (rectangle.Element as Rectangle).Fill == BlockFactory.TransparentBrush ? BlockFactory.TransparentBrush : BlockFactory.SelectedBrush;
            Panel.SetZIndex(rectangle.Element as Rectangle, SelectedZIndex);
        }

        public static void SelectEllipse(XEllipse ellipse)
        {
            (ellipse.Element as Ellipse).Stroke = BlockFactory.SelectedBrush;
            (ellipse.Element as Ellipse).Fill = (ellipse.Element as Ellipse).Fill == BlockFactory.TransparentBrush ? BlockFactory.TransparentBrush : BlockFactory.SelectedBrush;
            Panel.SetZIndex(ellipse.Element as Ellipse, SelectedZIndex);
        }

        public static void SelectText(XText text)
        {
            BlockFactory.GetTextBlock(text).Foreground = BlockFactory.SelectedBrush;
            Panel.SetZIndex(text.Element as Grid, SelectedZIndex);
        }

        public static void SelectImage(XImage image)
        {
            (image.Element as Image).OpacityMask = BlockFactory.SelectedBrush;
            Panel.SetZIndex(image.Element as Image, SelectedZIndex);
        }

        public static void SelectBlock(XBlock parent)
        {
            if (parent.Points != null)
            {
                foreach (var point in parent.Points)
                {
                    SelectPoint(point);
                }
            }

            if (parent.Lines != null)
            {
                foreach (var line in parent.Lines)
                {
                    SelectLine(line);
                }
            }

            if (parent.Rectangles != null)
            {
                foreach (var rectangle in parent.Rectangles)
                {
                    SelectRectangle(rectangle);
                } 
            }

            if (parent.Ellipses != null)
            {
                foreach (var ellipse in parent.Ellipses)
                {
                    SelectEllipse(ellipse);
                } 
            }

            if (parent.Texts != null)
            {
                foreach (var text in parent.Texts)
                {
                    SelectText(text);
                } 
            }

            if (parent.Images != null)
            {
                foreach (var image in parent.Images)
                {
                    SelectImage(image);
                } 
            }

            if (parent.Blocks != null)
            {
                foreach (var block in parent.Blocks)
                {
                    SelectBlock(block);
                } 
            }
        }

        public static void SelectAll(XBlock selected, XBlock content)
        {
            selected.Init();

            foreach (var point in content.Points)
            {
                SelectPoint(point);
                selected.Points.Add(point);
            }

            foreach (var line in content.Lines)
            {
                SelectLine(line);
                selected.Lines.Add(line);
            }

            foreach (var rectangle in content.Rectangles)
            {
                SelectRectangle(rectangle);
                selected.Rectangles.Add(rectangle);
            }

            foreach (var ellipse in content.Ellipses)
            {
                SelectEllipse(ellipse);
                selected.Ellipses.Add(ellipse);
            }

            foreach (var text in content.Texts)
            {
                SelectText(text);
                selected.Texts.Add(text);
            }

            foreach (var image in content.Images)
            {
                SelectImage(image);
                selected.Images.Add(image);
            }

            foreach (var parent in content.Blocks)
            {
                foreach (var point in parent.Points)
                {
                    SelectPoint(point);
                }

                foreach (var line in parent.Lines)
                {
                    SelectLine(line);
                }

                foreach (var rectangle in parent.Rectangles)
                {
                    SelectRectangle(rectangle);
                }

                foreach (var ellipse in parent.Ellipses)
                {
                    SelectEllipse(ellipse);
                }

                foreach (var text in parent.Texts)
                {
                    SelectText(text);
                }

                foreach (var image in parent.Images)
                {
                    SelectImage(image);
                }

                foreach (var block in parent.Blocks)
                {
                    SelectBlock(block);
                }

                selected.Blocks.Add(parent);
            }
        }

        public static void DeselectAll(XBlock selected)
        {
            if (selected.Points != null)
            {
                foreach (var point in selected.Points)
                {
                    DeselectPoint(point);
                }

                selected.Points = null;
            }

            if (selected.Lines != null)
            {
                foreach (var line in selected.Lines)
                {
                    DeselectLine(line);
                }

                selected.Lines = null;
            }

            if (selected.Rectangles != null)
            {
                foreach (var rectangle in selected.Rectangles)
                {
                    DeselectRectangle(rectangle);
                }

                selected.Rectangles = null;
            }

            if (selected.Ellipses != null)
            {
                foreach (var ellipse in selected.Ellipses)
                {
                    DeselectEllipse(ellipse);
                }

                selected.Ellipses = null;
            }

            if (selected.Texts != null)
            {
                foreach (var text in selected.Texts)
                {
                    DeselectText(text);
                }

                selected.Texts = null;
            }

            if (selected.Images != null)
            {
                foreach (var image in selected.Images)
                {
                    DeselectImage(image);
                }

                selected.Images = null;
            }

            if (selected.Blocks != null)
            {
                foreach (var parent in selected.Blocks)
                {
                    foreach (var point in parent.Points)
                    {
                        DeselectPoint(point);
                    }

                    foreach (var line in parent.Lines)
                    {
                        DeselectLine(line);
                    }

                    foreach (var rectangle in parent.Rectangles)
                    {
                        DeselectRectangle(rectangle);
                    }

                    foreach (var ellipse in parent.Ellipses)
                    {
                        DeselectEllipse(ellipse);
                    }

                    foreach (var text in parent.Texts)
                    {
                        DeselectText(text);
                    }

                    foreach (var image in parent.Images)
                    {
                        DeselectImage(image);
                    }

                    foreach (var block in parent.Blocks)
                    {
                        DeselectBlock(block);
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

        public static bool HitTestLines(IEnumerable<XLine> lines, XBlock selected, Rect rect, bool onlyFirst, bool select)
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
                            SelectLine(line);
                            selected.Lines.Add(line);
                        }
                        else
                        {
                            DeselectLine(line);
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

        public static bool HitTestRectangles(IEnumerable<XRectangle> rectangles, XBlock selected, Rect rect, bool onlyFirst, bool select, object relativeTo)
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
                            SelectRectangle(rectangle);
                            selected.Rectangles.Add(rectangle);
                        }
                        else
                        {
                            DeselectRectangle(rectangle);
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

        public static bool HitTestEllipses(IEnumerable<XEllipse> ellipses, XBlock selected, Rect rect, bool onlyFirst, bool select, object relativeTo)
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
                            SelectEllipse(ellipse);
                            selected.Ellipses.Add(ellipse);
                        }
                        else
                        {
                            DeselectEllipse(ellipse);
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

        public static bool HitTestTexts(IEnumerable<XText> texts, XBlock selected, Rect rect, bool onlyFirst, bool select, object relativeTo)
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
                            SelectText(text);
                            selected.Texts.Add(text);
                        }
                        else
                        {
                            DeselectText(text);
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

        public static bool HitTestImages(IEnumerable<XImage> images, XBlock selected, Rect rect, bool onlyFirst, bool select, object relativeTo)
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
                            SelectImage(image);
                            selected.Images.Add(image);
                        }
                        else
                        {
                            DeselectImage(image);
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

        public static bool HitTestBlocks(IEnumerable<XBlock> blocks, XBlock selected, Rect rect, bool onlyFirst, bool select, bool selectInsideBlock, object relativeTo)
        {
            foreach (var block in blocks)
            {
                bool result = HitTestBlock(block, selected, rect, true, selectInsideBlock, relativeTo);

                if (result)
                {
                    if (select && !selectInsideBlock)
                    {
                        if (!selected.Blocks.Contains(block))
                        {
                            selected.Blocks.Add(block);
                            SelectBlock(block);
                        }
                        else
                        {
                            selected.Blocks.Remove(block);
                            DeselectBlock(block);
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

        public static bool HitTestBlock(XBlock parent, XBlock selected, Rect rect, bool onlyFirst, bool selectInsideBlock, object relativeTo)
        {
            bool result = false;

            result = HitTestTexts(parent.Texts, selected, rect, onlyFirst, selectInsideBlock, relativeTo);
            if (result && onlyFirst)
            {
                return true;
            }

            result = HitTestImages(parent.Images, selected, rect, onlyFirst, selectInsideBlock, relativeTo);
            if (result && onlyFirst)
            {
                return true;
            }

            result = HitTestLines(parent.Lines, selected, rect, onlyFirst, selectInsideBlock);
            if (result && onlyFirst)
            {
                return true;
            }

            result = HitTestRectangles(parent.Rectangles, selected, rect, onlyFirst, selectInsideBlock, relativeTo);
            if (result && onlyFirst)
            {
                return true;
            }

            result = HitTestEllipses(parent.Ellipses, selected, rect, onlyFirst, selectInsideBlock, relativeTo);
            if (result && onlyFirst)
            {
                return true;
            }

            result = HitTestBlocks(parent.Blocks, selected, rect, onlyFirst, false, selectInsideBlock, relativeTo);
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

            if (parent.Texts != null)
            {
                bool result = HitTestTexts(parent.Texts, selected, rect, true, true, sheet.GetParent());
                if (result)
                {
                    HitTestClean(selected);
                    return true;
                }
            }

            if (parent.Images != null)
            {
                bool result = HitTestImages(parent.Images, selected, rect, true, true, sheet.GetParent());
                if (result)
                {
                    HitTestClean(selected);
                    return true;
                }
            }

            if (parent.Lines != null)
            {
                bool result = HitTestLines(parent.Lines, selected, rect, true, true);
                if (result)
                {
                    HitTestClean(selected);
                    return true;
                }
            }

            if (parent.Rectangles != null)
            {
                bool result = HitTestRectangles(parent.Rectangles, selected, rect, true, true, sheet.GetParent());
                if (result)
                {
                    HitTestClean(selected);
                    return true;
                }
            }

            if (parent.Ellipses != null)
            {
                bool result = HitTestEllipses(parent.Ellipses, selected, rect, true, true, sheet.GetParent());
                if (result)
                {
                    HitTestClean(selected);
                    return true;
                }
            }

            if (parent.Blocks != null)
            {
                bool result = HitTestBlocks(parent.Blocks, selected, rect, true, true, selectInsideBlock, sheet.GetParent());
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
                bool result = HitTestBlocks(parent.Blocks, selected, rect, true, true, false, sheet.GetParent());
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

            if (parent.Lines != null)
            {
                HitTestLines(parent.Lines, selected, rect, false, true);
            }

            if (parent.Rectangles != null)
            {
                HitTestRectangles(parent.Rectangles, selected, rect, false, true, sheet.GetParent());
            }

            if (parent.Ellipses != null)
            {
                HitTestEllipses(parent.Ellipses, selected, rect, false, true, sheet.GetParent());
            }

            if (parent.Texts != null)
            {
                HitTestTexts(parent.Texts, selected, rect, false, true, sheet.GetParent());
            }

            if (parent.Images != null)
            {
                HitTestImages(parent.Images, selected, rect, false, true, sheet.GetParent());
            }

            if (parent.Blocks != null)
            {
                HitTestBlocks(parent.Blocks, selected, rect, false, true, false, sheet.GetParent());
            }

            HitTestClean(selected);
        }

        private static void HitTestClean(XBlock selected)
        {
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

        #endregion

        #region Copy

        public static XBlock ShallowCopy(XBlock original)
        {
            var copy = new XBlock(original.Id, original.X, original.Y, original.Width, original.Height, original.DataId, original.Name);

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
        public static SolidColorBrush TransparentBrush = Brushes.Transparent;

        #endregion

        #region Create

        public static XPoint CreatePoint(double thickness, double x, double y, bool isVisible)
        {
            var ellipse = new Ellipse()
            {
                Fill = NormalBrush,
                Stroke = NormalBrush,
                StrokeThickness = thickness,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                Width = 8.0,
                Height = 8.0,
                Margin = new Thickness(-4.0, -4.0, 0.0, 0.0),
                Visibility = isVisible ? Visibility.Visible : Visibility.Hidden
            };

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
}
