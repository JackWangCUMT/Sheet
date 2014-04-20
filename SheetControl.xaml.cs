using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;

namespace Sheet
{
    #region Model

    public abstract class Item
    {
        public int Id { get; set; }
    }

    public class LineItem : Item
    {
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
    }

    public class RectangleItem : Item
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool IsFilled { get; set; }
    } 

    public class TextItem : Item
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public int HAlign { get; set; }
        public int VAlign { get; set; }
        public double Size { get; set; }
        public string Text { get; set; }
    }

    public class BlockItem : Item
    {
        public string Name { get; set; }
        public List<LineItem> Lines { get; set; }
        public List<RectangleItem> Rectangles { get; set; }
        public List<TextItem> Texts { get; set; }
        public List<BlockItem> Blocks { get; set; }
        public void Init(int id, string name)
        {
            Id = id;
            Name = name;
            Lines = new List<LineItem>();
            Rectangles = new List<RectangleItem>();
            Texts = new List<TextItem>();
            Blocks = new List<BlockItem>();
        }
    } 

    #endregion

    #region ItemSerializer

    public static class ItemSerializer
    {
        #region Fields

        private static string lineSeparator = "\r\n";
        private static string modelSeparator = ";";
        private static char[] lineSeparators = { '\r', '\n' };
        private static char[] modelSeparators = { ';' };
        private static char[] whiteSpace = { ' ', '\t' };
        private static string indentWhiteSpace = "    ";

        #endregion

        #region Serialize

        public static void Serialize(StringBuilder sb, LineItem line, string indent)
        {
            sb.Append(indent);
            sb.Append("LINE");
            sb.Append(modelSeparator);
            sb.Append(line.Id);
            sb.Append(modelSeparator);
            sb.Append(line.X1);
            sb.Append(modelSeparator);
            sb.Append(line.Y1);
            sb.Append(modelSeparator);
            sb.Append(line.X2);
            sb.Append(modelSeparator);
            sb.Append(line.Y2);
            sb.Append(lineSeparator);
        }

        public static void Serialize(StringBuilder sb, RectangleItem rectangle, string indent)
        {
            sb.Append(indent);
            sb.Append("RECTANGLE");
            sb.Append(modelSeparator);
            sb.Append(rectangle.Id);
            sb.Append(modelSeparator);
            sb.Append(rectangle.X);
            sb.Append(modelSeparator);
            sb.Append(rectangle.Y);
            sb.Append(modelSeparator);
            sb.Append(rectangle.Width);
            sb.Append(modelSeparator);
            sb.Append(rectangle.Height);
            sb.Append(modelSeparator);
            sb.Append(rectangle.IsFilled);
            sb.Append(lineSeparator);
        }

        public static void Serialize(StringBuilder sb, TextItem text, string indent)
        {
            sb.Append(indent);
            sb.Append("TEXT");
            sb.Append(modelSeparator);
            sb.Append(text.Id);
            sb.Append(modelSeparator);
            sb.Append(text.X);
            sb.Append(modelSeparator);
            sb.Append(text.Y);
            sb.Append(modelSeparator);
            sb.Append(text.Width);
            sb.Append(modelSeparator);
            sb.Append(text.Height);
            sb.Append(modelSeparator);
            sb.Append(text.HAlign);
            sb.Append(modelSeparator);
            sb.Append(text.VAlign);
            sb.Append(modelSeparator);
            sb.Append(text.Size);
            sb.Append(modelSeparator);
            sb.Append(text.Text);
            sb.Append(lineSeparator);
        }

        public static void Serialize(StringBuilder sb, BlockItem block, string indent)
        {
            sb.Append(indent);
            sb.Append("BLOCK");
            sb.Append(modelSeparator);
            sb.Append(block.Id);
            sb.Append(modelSeparator);
            sb.Append(block.Name);
            sb.Append(lineSeparator);

            Serialize(sb, block.Lines, indent + indentWhiteSpace);
            Serialize(sb, block.Rectangles, indent + indentWhiteSpace);
            Serialize(sb, block.Texts, indent + indentWhiteSpace);
            Serialize(sb, block.Blocks, indent + indentWhiteSpace);

            sb.Append(indent);
            sb.Append("END");
            sb.Append(lineSeparator);
        }

        public static void Serialize(StringBuilder sb, List<LineItem> lines, string indent)
        {
            foreach (var line in lines)
            {
                Serialize(sb, line, indent);
            }
        }

        public static void Serialize(StringBuilder sb, List<RectangleItem> rectangles, string indent)
        {
            foreach (var rectangle in rectangles)
            {
                Serialize(sb, rectangle, indent);
            }
        }

        public static void Serialize(StringBuilder sb, List<TextItem> texts, string indent)
        {
            foreach (var text in texts)
            {
                Serialize(sb, text, indent);
            }
        }

        public static void Serialize(StringBuilder sb, List<BlockItem> blocks, string indent)
        {
            foreach (var block in blocks)
            {
                Serialize(sb, block, indent);
            }
        }

        public static string Serialize(BlockItem block)
        {
            var sb = new StringBuilder();

            Serialize(sb, block.Lines, "");
            Serialize(sb, block.Rectangles, "");
            Serialize(sb, block.Texts, "");
            Serialize(sb, block.Blocks, "");

            return sb.ToString();
        }

        #endregion

        #region Deserialize
     
        private static BlockItem Deserialize(string[] lines, int length, ref int end, string name, int id)
        {
            var sheet = new BlockItem();
            sheet.Init(id, name);

            for (; end < length; end++)
            {
                string line = lines[end].TrimStart(whiteSpace);
                var m = line.Split(modelSeparators);
                if (m.Length == 6 && string.Compare(m[0], "LINE", true) == 0)
                {
                    var lineItem = new LineItem();
                    lineItem.Id = int.Parse(m[1]);
                    lineItem.X1 = double.Parse(m[2]);
                    lineItem.Y1 = double.Parse(m[3]);
                    lineItem.X2 = double.Parse(m[4]);
                    lineItem.Y2 = double.Parse(m[5]);
                    sheet.Lines.Add(lineItem);
                }
                if (m.Length == 7 && string.Compare(m[0], "RECTANGLE", true) == 0)
                {
                    var rectangleItem = new RectangleItem();
                    rectangleItem.Id = int.Parse(m[1]);
                    rectangleItem.X = double.Parse(m[2]);
                    rectangleItem.Y = double.Parse(m[3]);
                    rectangleItem.Width = double.Parse(m[4]);
                    rectangleItem.Height = double.Parse(m[5]);
                    rectangleItem.IsFilled = bool.Parse(m[6]);
                    sheet.Rectangles.Add(rectangleItem);
                }
                else if (m.Length == 10 && string.Compare(m[0], "TEXT", true) == 0)
                {
                    var textItem = new TextItem();
                    textItem.Id = int.Parse(m[1]);
                    textItem.X = double.Parse(m[2]);
                    textItem.Y = double.Parse(m[3]);
                    textItem.Width = double.Parse(m[4]);
                    textItem.Height = double.Parse(m[5]);
                    textItem.HAlign = int.Parse(m[6]);
                    textItem.VAlign = int.Parse(m[7]);
                    textItem.Size = double.Parse(m[8]);
                    textItem.Text = m[9];
                    sheet.Texts.Add(textItem);
                }
                else if (m.Length == 3 && string.Compare(m[0], "BLOCK", true) == 0)
                {
                    end++;
                    var block = Deserialize(lines, length, ref end, m[2], int.Parse(m[1]));
                    sheet.Blocks.Add(block);
                    continue;
                }
                else if (m.Length == 1 && string.Compare(m[0], "END", true) == 0)
                {
                    return sheet;
                }
            }

            return sheet;
        }

        public static BlockItem Deserialize(string model)
        {
            string[] lines = model.Split(lineSeparators, StringSplitOptions.RemoveEmptyEntries);
            int length = lines.Length;
            int end = 0;
            return Deserialize(lines, length, ref end, "LOGIC", 0);
        }

        #endregion

        #region I/O

        public static string OpenText(string fileName)
        {
            try
            {
                using (var stream = System.IO.File.OpenText(fileName))
                {
                    var text = stream.ReadToEnd();
                    return text;
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
            return null;
        }

        public static void SaveText(string fileName, string text)
        {
            try
            {
                if (text != null)
                {
                    using (var stream = System.IO.File.CreateText(fileName))
                    {
                        stream.Write(text);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }

        #endregion
    }

    #endregion

    #region ItemEditor

    public static class ItemEditor
    {
        #region Normalize Position

        public static void NormalizePosition(BlockItem block, double originX, double originY, double width, double height)
        {
            double minX = width;
            double minY = height;
            double maxX = originX;
            double maxY = originY;
            MinMax(block, ref minX, ref minY, ref maxX, ref maxY);
            double x = -(maxX - (maxX - minX));
            double y = -(maxY - (maxY - minY));
            Move(block, x, y);
        }

        #endregion

        #region MinMax

        public static void MinMax(BlockItem block, ref double minX, ref double minY, ref double maxX, ref double maxY)
        {
            MinMax(block.Lines, ref minX, ref minY, ref maxX, ref maxY);
            MinMax(block.Rectangles, ref minX, ref minY, ref maxX, ref maxY);
            MinMax(block.Texts, ref minX, ref minY, ref maxX, ref maxY);
            MinMax(block.Blocks, ref minX, ref minY, ref maxX, ref maxY);
        }

        public static void MinMax(IEnumerable<BlockItem> blocks, ref double minX, ref double minY, ref double maxX, ref double maxY)
        {
            foreach (var block in blocks)
            {
                MinMax(block, ref minX, ref minY, ref maxX, ref maxY);
            }
        }

        public static void MinMax(IEnumerable<LineItem> lines, ref double minX, ref double minY, ref double maxX, ref double maxY)
        {
            foreach (var line in lines)
            {
                minX = Math.Min(minX, line.X1);
                minX = Math.Min(minX, line.X2);
                minY = Math.Min(minY, line.Y1);
                minY = Math.Min(minY, line.Y2);
                maxX = Math.Max(maxX, line.X1);
                maxX = Math.Max(maxX, line.X2);
                maxY = Math.Max(maxY, line.Y1);
                maxY = Math.Max(maxY, line.Y2);
            }
        }

        public static void MinMax(IEnumerable<RectangleItem> rectangles, ref double minX, ref double minY, ref double maxX, ref double maxY)
        {
            foreach (var rectangle in rectangles)
            {
                minX = Math.Min(minX, rectangle.X);
                minY = Math.Min(minY, rectangle.Y);
                maxX = Math.Max(maxX, rectangle.X);
                maxY = Math.Max(maxY, rectangle.Y);
            }
        }

        public static void MinMax(IEnumerable<TextItem> texts, ref double minX, ref double minY, ref double maxX, ref double maxY)
        {
            foreach (var text in texts)
            {
                minX = Math.Min(minX, text.X);
                minY = Math.Min(minY, text.Y);
                maxX = Math.Max(maxX, text.X);
                maxY = Math.Max(maxY, text.Y);
            }
        }

        #endregion

        #region Move

        public static void Move(IEnumerable<BlockItem> blocks, double x, double y)
        {
            foreach (var block in blocks)
            {
                Move(block, x, y);
            }
        }

        public static void Move(BlockItem block, double x, double y)
        {
            Move(block.Lines, x, y);
            Move(block.Rectangles, x, y);
            Move(block.Texts, x, y);
            Move(block.Blocks, x, y);
        }

        public static void Move(IEnumerable<LineItem> lines, double x, double y)
        {
            foreach (var line in lines)
            {
                Move(line, x, y);
            }
        }

        public static void Move(IEnumerable<RectangleItem> rectangles, double x, double y)
        {
            foreach (var rectangle in rectangles)
            {
                Move(rectangle, x, y);
            }
        }

        public static void Move(IEnumerable<TextItem> texts, double x, double y)
        {
            foreach (var text in texts)
            {
                Move(text, x, y);
            }
        }

        public static void Move(LineItem line, double x, double y)
        {
            line.X1 += x;
            line.Y1 += y;
            line.X2 += x;
            line.Y2 += y;
        }

        public static void Move(RectangleItem rect, double x, double y)
        {
            rect.X += x;
            rect.Y += y;
        }

        public static void Move(TextItem text, double x, double y)
        {
            text.X += x;
            text.Y += y;
        }

        #endregion
    }

    #endregion

    #region ISheet

    public interface ISheet
    {
        FrameworkElement GetParent();
        void Add(UIElement element);
        void Remove(UIElement element);
        void Capture();
        void ReleaseCapture();
        bool IsCaptured { get; }
    }

    #endregion

    #region Block

    public class Block
    {
        public string Name { get; set; }
        public List<Line> Lines { get; set; }
        public List<Rectangle> Rectangles { get; set; }
        public List<Grid> Texts { get; set; }
        public List<Block> Blocks { get; set; }
        public void Init()
        {
            Lines = new List<Line>();
            Rectangles = new List<Rectangle>();
            Texts = new List<Grid>();
            Blocks = new List<Block>();
        }
    }

    #endregion

    #region Mode

    public enum Mode
    {
        None,
        Selection,
        Insert,
        Pan,
        Move,
        Line,
        Rectangle,
        Text,
    }

    #endregion

    #region CanvasSheet

    public class CanvasSheet : ISheet
    {
        #region Fields

        private Canvas canvas = null;

        #endregion

        #region Constructor

        public CanvasSheet(Canvas canvas)
        {
            this.canvas = canvas;
        }

        #endregion

        #region ISheet

        public FrameworkElement GetParent()
        {
            return canvas;
        }

        public void Add(UIElement element)
        {
            canvas.Children.Add(element);
        }

        public void Remove(UIElement element)
        {
            canvas.Children.Remove(element);
        }

        public void Capture()
        {
            canvas.CaptureMouse();
        }

        public void ReleaseCapture()
        {
            canvas.ReleaseMouseCapture();
        }

        public bool IsCaptured
        {
            get { return canvas.IsMouseCaptured; }
        }

        #endregion
    }

    #endregion

    public partial class SheetControl : UserControl
    {
        #region Fields

        private ISheet back = null;
        private ISheet sheet = null;
        private ISheet overlay = null;
        private Stack<string> undos = new Stack<string>();
        private Stack<string> redos = new Stack<string>();
        private Mode mode = Mode.Selection;
        private Mode tempMode = Mode.None;
        private int zoomIndex = 9;
        private int defaultZoomIndex = 9;
        private int maxZoomIndex = 21;
        private double[] zoomFactors = { 0.01, 0.0625, 0.0833, 0.125, 0.25, 0.3333, 0.5, 0.6667, 0.75, 1, 1.25, 1.5, 2, 3, 4, 6, 8, 12, 16, 24, 32, 64 };
        private double snapSize = 15;
        private double gridSize = 30;
        private double frameThickness = 1.0;
        private double gridThickness = 1.0;
        private double selectionThickness = 1.0;
        private double lineThickness = 2.0;
        private Point panStartPoint;
        private Line tempLine = null;
        private Rectangle tempRectangle = null;
        private Point selectionStartPoint;
        private Rectangle selectionRect = null;
        private double hitSize = 3.5;
        private List<Line> gridLines = new List<Line>();
        private List<Line> frameLines = new List<Line>();
        private Block logic = null;
        private Block selected = null;

        #endregion

        #region Properties

        public int ZoomIndex
        {
            get { return zoomIndex; }
            set
            {
                if (value >= 0 && value <= maxZoomIndex)
                {
                    zoomIndex = value;
                    Zoom = zoomFactors[zoomIndex];
                }
            }
        }

        public double Zoom
        {
            get { return zoom.ScaleX; }
            set
            {
                if (IsLoaded)
                    AdjustThickness(value);

                zoom.ScaleX = value;
                zoom.ScaleY = value;
            }
        }

        public double PanX
        {
            get { return pan.X; }
            set
            {
                pan.X = value;
            }
        }

        public double PanY
        {
            get { return pan.Y; }
            set
            {
                pan.Y = value;
            }
        }

        #endregion

        #region Snap

        private double Snap(double val)
        {
            double r = val % snapSize;
            return r >= snapSize / 2.0 ? val + snapSize - r : val - r;
        }

        #endregion

        #region Constructor

        public SheetControl()
        {
            InitializeComponent();

            back = new CanvasSheet(Back);
            sheet = new CanvasSheet(Sheet);
            overlay = new CanvasSheet(Overlay);

            logic = new Block() { Name = "LOGIC" };
            logic.Init();

            selected = new Block() { Name = "SELECTED" };

            Loaded += (s, e) =>
            {
                CreateGrid(back, gridLines, 300.0, 0.0, 600.0, 750.0, gridSize, gridThickness);
                CreateFrame(back, frameLines, gridSize, gridThickness);
                AdjustThickness(gridLines, gridThickness / zoomFactors[zoomIndex]);
                AdjustThickness(frameLines, frameThickness / zoomFactors[zoomIndex]);
                LoadStandardLibrary();
                Focus();
            };
        }

        #endregion

        #region Mode

        public void ModeNone()
        {
            mode = Mode.None;
        }

        public void ModeSelection()
        {
            mode = Mode.Selection;
        }

        public void ModeInsert()
        {
            mode = Mode.Insert;
        }

        public void ModePan()
        {
            mode = Mode.Pan;
        }

        public void ModeMove()
        {
            mode = Mode.Move;
        }

        public void ModeLine()
        {
            mode = Mode.Line;
        }

        public void ModeRectangle()
        {
            mode = Mode.Rectangle;
        }

        public void ModeText()
        {
            mode = Mode.Text;
        }

        #endregion

        #region Back

        private static void AddLine(ISheet sheet, List<Line> lines, double thickness, double x1, double y1, double x2, double y2, Brush stroke)
        {
            var line = new Line()
            {
                Stroke = stroke,
                StrokeThickness = thickness,
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2
            };
            lines.Add(line);
            sheet.Add(line);
        }

        private static void CreateFrame(ISheet sheet, List<Line> lines, double size, double thickness)
        {
            AddLine(sheet, lines, thickness, 210.0, 0.0, 210.0, 750.0, Brushes.DarkGray);
            AddLine(sheet, lines, thickness, 300.0, 0.0, 300.0, 750.0, Brushes.DarkGray);

            for (double y = 30.0; y < 750.0; y += size)
            {
                AddLine(sheet, lines, thickness, 0.0, y, 300.0, y, Brushes.DarkGray);
            }

            AddLine(sheet, lines, thickness, 900.0, 0.0, 900.0, 750.0, Brushes.DarkGray);
            AddLine(sheet, lines, thickness, 1110.0, 0.0, 1110.0, 750.0, Brushes.DarkGray);

            for (double y = 30.0; y < 750.0; y += size)
            {
                AddLine(sheet, lines, thickness, 900.0, y, 1200.0, y, Brushes.DarkGray);
            }
        }

        private static void CreateGrid(ISheet sheet, List<Line> lines, double startX, double startY, double width, double height, double size, double thickness)
        {
            for (double y = startY + size; y < height + startY; y += size)
            {
                AddLine(sheet, lines, thickness, startX, y, width + startX, y, Brushes.LightGray);
            }

            for (double x = startX + size; x < startX + width; x += size)
            {
                AddLine(sheet, lines, thickness, x, startY, x, height + startY, Brushes.LightGray);
            }
        }

        #endregion

        #region Serialize

        private static LineItem SerializeLine(Line line)
        {
            var lineItem = new LineItem();

            lineItem.Id = 0;
            lineItem.X1 = line.X1;
            lineItem.Y1 = line.Y1;
            lineItem.X2 = line.X2;
            lineItem.Y2 = line.Y2;

            return lineItem;
        }

        private static RectangleItem SerializeRectangle(Rectangle rectangle)
        {
            var rectangleItem = new RectangleItem();

            rectangleItem.Id = 0;
            rectangleItem.X = Canvas.GetLeft(rectangle);
            rectangleItem.Y = Canvas.GetTop(rectangle);
            rectangleItem.Width = rectangle.Width;
            rectangleItem.Height = rectangle.Height;
            rectangleItem.IsFilled = rectangle.Fill == Brushes.Transparent ? false : true;

            return rectangleItem;
        }

        private static TextItem SerializeText(Grid text)
        {
            var textItem = new TextItem();

            textItem.Id = 0;
            textItem.X = Canvas.GetLeft(text);
            textItem.Y = Canvas.GetTop(text);
            textItem.Width = text.Width;
            textItem.Height = text.Height;

            var tb = GetTextBlock(text);
            textItem.Text = tb.Text;
            textItem.HAlign = (int)tb.HorizontalAlignment;
            textItem.VAlign = (int)tb.VerticalAlignment;
            textItem.Size = tb.FontSize;

            return textItem;
        }

        private static BlockItem SerializeBlock(Block parent)
        {
            var blockItem = new BlockItem();
            blockItem.Init(0, parent.Name);

            foreach (var line in parent.Lines)
            {
                blockItem.Lines.Add(SerializeLine(line));
            }

            foreach (var rectangle in parent.Rectangles)
            {
                blockItem.Rectangles.Add(SerializeRectangle(rectangle));
            }

            foreach (var text in parent.Texts)
            {
                blockItem.Texts.Add(SerializeText(text));
            }

            foreach(var block in parent.Blocks)
            {
                blockItem.Blocks.Add(SerializeBlock(block));
            }

            return blockItem;
        }

        private static BlockItem CreateSheet(int id, 
            string name, 
            IEnumerable<Line> lines, 
            IEnumerable<Rectangle> rectangles, 
            IEnumerable<Grid> texts, 
            IEnumerable<Block> blocks)
        {
            var sheet = new BlockItem();
            sheet.Init(id, name);

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

            if (texts != null)
            {
                foreach (var text in texts)
                {
                    sheet.Texts.Add(SerializeText(text));
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

        private BlockItem CreateSheet()
        {
            return CreateSheet(0, "LOGIC", logic.Lines, logic.Rectangles, logic.Texts, logic.Blocks);
        }

        #endregion

        #region Create

        private static TextBlock GetTextBlock(Grid text)
        {
            return text.Children[0] as TextBlock;
        }

        private static Grid CreateText(string text,
                                double x, double y,
                                double width, double height,
                                HorizontalAlignment halign, VerticalAlignment valign,
                                double size)
        {
            var grid = new Grid();
            grid.Background = Brushes.Transparent;
            grid.Width = width;
            grid.Height = height;
            Canvas.SetLeft(grid, x);
            Canvas.SetTop(grid, y);

            var tb = new TextBlock();
            tb.HorizontalAlignment = halign;
            tb.VerticalAlignment = valign;
            tb.Background = Brushes.White;
            tb.Foreground = Brushes.Black;
            tb.FontSize = size;
            tb.FontFamily = new FontFamily("Calibri");
            tb.Text = text;

            grid.Children.Add(tb);

            return grid;
        }

        private static Line CreateLine(double thickness, double x1, double y1, double x2, double y2)
        {
            var line = new Line()
            {
                Stroke = Brushes.Black,
                StrokeThickness = thickness,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2
            };

            return line;
        }

        private static Rectangle CreateRectangle(double thickness, double x, double y, double width, double height, bool isFilled)
        {
            var rect = new Rectangle()
            {
                Fill = isFilled ? Brushes.Black : Brushes.Transparent,
                Stroke = Brushes.Black,
                StrokeThickness = thickness,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                Width = width,
                Height = height
            };

            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);

            return rect;
        }

        private static Rectangle CreateSelectionRectangle(double thickness, double x, double y, double width, double height)
        {
            var rect = new Rectangle()
            {
                Fill = new SolidColorBrush(Color.FromArgb(0x3A, 0x00, 0x00, 0xFF)),
                Stroke = new SolidColorBrush(Color.FromArgb(0x7F, 0x00, 0x00, 0xFF)),
                StrokeThickness = thickness,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                Width = width,
                Height = height
            };

            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);

            return rect;
        }

        #endregion

        #region Load

        private void Load(BlockItem block, bool select)
        {
            DeselectAll(selected);
            double thickness = lineThickness / Zoom;
            Load(sheet, block.Texts, logic, selected, select, thickness);
            Load(sheet, block.Lines, logic, selected, select, thickness);
            Load(sheet, block.Rectangles, logic, selected, select, thickness);
            Load(sheet, block.Blocks, logic, selected, select, thickness);
        }

        private static void Load(ISheet sheet, IEnumerable<TextItem> textItems, Block parent, Block selected, bool select, double thickness)
        {
            if (select)
            {
                selected.Texts = new List<Grid>();
            }

            foreach (var textItem in textItems)
            {
                var text = LoadTextItem(sheet, parent, textItem);

                if (select)
                {
                    GetTextBlock(text).Foreground = Brushes.Red;
                    selected.Texts.Add(text);
                }
            }
        }

        private static void Load(ISheet sheet, IEnumerable<LineItem> lineItems, Block parent, Block selected, bool select, double thickness)
        {
            if (select)
            {
                selected.Lines = new List<Line>();
            }

            foreach (var lineItem in lineItems)
            {
                var line = LoadLineItem(sheet, parent, lineItem, thickness);

                if (select)
                {
                    line.Stroke = Brushes.Red;
                    selected.Lines.Add(line);
                }
            }
        }

        private static void Load(ISheet sheet, IEnumerable<RectangleItem> rectangleItems, Block parent, Block selected, bool select, double thickness)
        {
            if (select)
            {
                selected.Rectangles = new List<Rectangle>();
            }

            foreach (var rectangleItem in rectangleItems)
            {
                var rectangle = LoadRectangleItem(sheet, parent, rectangleItem, thickness);

                if (select)
                {
                    rectangle.Stroke = Brushes.Red;
                    rectangle.Fill = rectangleItem.IsFilled ? Brushes.Red : Brushes.Transparent;
                    selected.Rectangles.Add(rectangle);
                }
            }
        }

        private static void Load(ISheet sheet, IEnumerable<BlockItem> blockItems, Block parent, Block selected, bool select, double thickness)
        {
            if (select)
            {
                selected.Blocks = new List<Block>();
            }

            foreach (var blockItem in blockItems)
            {
                var block = LoadBlockItem(sheet, parent, blockItem, select, thickness);

                if (select)
                {
                    selected.Blocks.Add(block);
                }
            }
        }

        private static Grid LoadTextItem(ISheet sheet, Block parent, TextItem textItem)
        {
            var text = CreateText(textItem.Text,
                                  textItem.X, textItem.Y,
                                  textItem.Width, textItem.Height,
                                  (HorizontalAlignment)textItem.HAlign,
                                  (VerticalAlignment)textItem.VAlign,
                                  textItem.Size);
            parent.Texts.Add(text);
            sheet.Add(text);
            return text;
        }

        private static Line LoadLineItem(ISheet sheet, Block parent, LineItem lineItem, double thickness)
        {
            var line = CreateLine(thickness, lineItem.X1, lineItem.Y1, lineItem.X2, lineItem.Y2);
            parent.Lines.Add(line);
            sheet.Add(line);
            return line;
        }

        private static Rectangle LoadRectangleItem(ISheet sheet, Block parent, RectangleItem rectangleItem, double thickness)
        {
            var rectangle = CreateRectangle(thickness, rectangleItem.X, rectangleItem.Y, rectangleItem.Width, rectangleItem.Height, rectangleItem.IsFilled);
            parent.Rectangles.Add(rectangle);
            sheet.Add(rectangle);
            return rectangle;
        }

        private static Block LoadBlockItem(ISheet sheet, Block parent, BlockItem blockItem, bool select, double thickness)
        {
            var block = new Block() { Name = blockItem.Name };
            block.Init();

            foreach (var textItem in blockItem.Texts)
            {
                var text = LoadTextItem(sheet, block, textItem);

                if (select)
                {
                    GetTextBlock(text).Foreground = Brushes.Red;
                }
            }

            foreach (var lineItem in blockItem.Lines)
            {
                var line = LoadLineItem(sheet, block, lineItem, thickness);

                if (select)
                {
                    line.Stroke = Brushes.Red;
                }
            }

            foreach (var rectangleItem in blockItem.Rectangles)
            {
                var rectangle = LoadRectangleItem(sheet, block, rectangleItem, thickness);

                if (select)
                {
                    rectangle.Stroke = Brushes.Red;
                    rectangle.Fill = rectangleItem.IsFilled ? Brushes.Red : Brushes.Transparent;
                }
            }

            foreach (var childBlockItem in blockItem.Blocks)
            {
                LoadBlockItem(sheet, block, childBlockItem, select, thickness);
            }

            parent.Blocks.Add(block);

            return block;
        }

        #endregion

        #region Undo/Redo

        public void Push()
        {
            undos.Push(ItemSerializer.Serialize(CreateSheet()));
        }

        public void Pop()
        {
            undos.Pop();
        }

        public void Undo()
        {
            if (undos.Count > 0)
            {
                redos.Push(ItemSerializer.Serialize(CreateSheet()));
                Reset();
                Load(ItemSerializer.Deserialize(undos.Pop()), false);
            }
        }

        public void Redo()
        {
            if (redos.Count > 0)
            {
                undos.Push(ItemSerializer.Serialize(CreateSheet()));
                Reset();
                Load(ItemSerializer.Deserialize(redos.Pop()), false);
            }
        }

        #endregion

        #region Clipboard

        public void Cut()
        {
            Copy();
            Push();

            if (HaveSelected(selected))
            {
                Delete();
            }
            else
            {
                Reset();
            }
        }

        public void Copy()
        {
            var text = ItemSerializer.Serialize(HaveSelected(selected) ?
                CreateSheet(0, "SELECTED", selected.Lines, selected.Rectangles, selected.Texts, selected.Blocks) : CreateSheet());
            Clipboard.SetData(DataFormats.UnicodeText, text);
        }

        public void Paste()
        {
            var text = (string)Clipboard.GetData(DataFormats.UnicodeText);
            Load(ItemSerializer.Deserialize(text), true);
        }

        #endregion

        #region File Dialogs

        public void Open()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "TXT Files (*.txt)|*.txt|All Files (*.*)|*.*"
            };

            if (dlg.ShowDialog() == true)
            {
                var text = ItemSerializer.OpenText(dlg.FileName);
                if (text != null)
                {
                    Push();
                    Reset();
                    Load(ItemSerializer.Deserialize(text), false);
                }
            }
        }

        public void Save()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "TXT Files (*.txt)|*.txt|All Files (*.*)|*.*",
                FileName = "sheet"
            };

            if (dlg.ShowDialog() == true)
            {
                var text = ItemSerializer.Serialize(CreateSheet());
                ItemSerializer.SaveText(dlg.FileName, text);
            }
        }

        public void Export()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "XPS Documents (*.xps)|*.xps|All Files (*.*)|*.*",
                FileName = "sheet"
            };

            if (dlg.ShowDialog() == true)
            {
                using (var package = Package.Open(dlg.FileName, System.IO.FileMode.Create))
                {
                    var doc = new XpsDocument(package);
                    var writer = XpsDocument.CreateXpsDocumentWriter(doc);
                    writer.Write(Root);
                    doc.Close();
                }
            }
        }

        public void Library()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "TXT Files (*.txt)|*.txt|All Files (*.*)|*.*"
            };

            if (dlg.ShowDialog() == true)
            {
                LoadLibrary(dlg.FileName);
            }
        }

        #endregion

        #region Library

        private void Insert(Point p)
        {
            var library = this == null ? null : GetOwner<SheetWindow>(this).Library;
            if (library != null && library.SelectedIndex >= 0)
            {
                var blockItem = library.SelectedItem as BlockItem;
                Insert(blockItem, p, true);
            }
        }

        private void Insert(BlockItem blockItem, Point p, bool select)
        {
            DeselectAll(selected);
            double thickness = lineThickness / Zoom;

            if (select)
            {
                selected.Blocks = new List<Block>();
            }

            Push();

            var block = LoadBlockItem(sheet, logic, blockItem, select, thickness);

            if (select)
            {
                selected.Blocks.Add(block);
            }

            Move(Snap(p.X), Snap(p.Y), block);
        }

        private void LoadStandardLibrary()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var resourceName = "Sheet.Libraries.Standard.txt";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var reader = new System.IO.StreamReader(stream))
                {
                    string text = reader.ReadToEnd();
                    if (text != null)
                    {
                        InitLibrary(text);
                    }
                }
            }
        }

        private void LoadLibrary(string fileName)
        {
            var text = ItemSerializer.OpenText(fileName);
            if (text != null)
            {
                InitLibrary(text);
            }
        }

        private void InitLibrary(string text)
        {
            var owner = this == null ? null : GetOwner<SheetWindow>(this);
            if (owner != null && text != null)
            {
                var library = owner.Library;
                var blocks = ItemSerializer.Deserialize(text).Blocks;
                library.ItemsSource = blocks;
                library.SelectedIndex = 0;
                if (blocks.Count == 0)
                {
                    library.Visibility = Visibility.Collapsed;
                }
                else
                {
                    library.Visibility = Visibility.Visible;
                }
            }
        }

        private void AddToLibrary(BlockItem block)
        {
            var library = this == null ? null : GetOwner<SheetWindow>(this).Library;
            if (library != null && block != null)
            {
                var items = library.ItemsSource as List<BlockItem>;
                ItemEditor.NormalizePosition(block, 0.0, 0.0, 1200.0, 750.0);
                items.Add(block);
                library.ItemsSource = null;
                library.ItemsSource = items;
            }
        }

        #endregion

        #region Remove

        private static void RemoveLines(ISheet sheet, IEnumerable<Line> lines)
        {
            foreach (var line in lines)
            {
                sheet.Remove(line);
            }
        }

        private static void RemoveRectangles(ISheet sheet, IEnumerable<Rectangle> rectangles)
        {
            foreach (var rectangle in rectangles)
            {
                sheet.Remove(rectangle);
            }
        }

        private static void RemoveTexts(ISheet sheet, IEnumerable<Grid> texts)
        {
            foreach (var text in texts)
            {
                sheet.Remove(text);
            }
        }

        private static void RemoveBlocks(ISheet sheet, IEnumerable<Block> blocks)
        {
            foreach (var block in blocks)
            {
                RemoveLines(sheet, block.Lines);
                RemoveRectangles(sheet, block.Rectangles);
                RemoveTexts(sheet, block.Texts);
                RemoveBlocks(sheet, block.Blocks);
            }
        }

        private static void Remove(ISheet sheet, Block parent, Block selected)
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

            if (selected.Texts != null)
            {
                RemoveTexts(sheet, selected.Texts);

                foreach (var text in selected.Texts)
                {
                    parent.Texts.Remove(text);
                }

                selected.Texts = null;
            }

            if (selected.Blocks != null)
            {
                foreach (var block in selected.Blocks)
                {
                    RemoveLines(sheet, block.Lines);
                    RemoveRectangles(sheet, block.Rectangles);
                    RemoveTexts(sheet, block.Texts);
                    RemoveBlocks(sheet, block.Blocks);

                    parent.Blocks.Remove(block);
                }

                selected.Blocks = null;
            }
        }

        public void Delete()
        {
            if (HaveSelected(selected))
            {
                Push();
                Remove(sheet, logic, selected);
            }
        }

        public void Reset()
        {
            RemoveLines(sheet, logic.Lines);
            RemoveRectangles(sheet, logic.Rectangles);
            RemoveTexts(sheet, logic.Texts);
            RemoveBlocks(sheet, logic.Blocks);

            logic.Lines.Clear();
            logic.Rectangles.Clear();
            logic.Texts.Clear();
            logic.Blocks.Clear();

            selected.Lines = null;
            selected.Rectangles = null;
            selected.Texts = null;
            selected.Blocks = null;
        }

        #endregion

        #region Move Mode

        private bool CanInitMove(Point p)
        {
            var temp = new Block();
            HitTest(p, hitSize, sheet, selected, temp, false);

            if (HaveSelected(temp))
            {
                return true;
            }

            return false;
        }

        private void InitMove(Point p)
        {
            Push();
            tempMode = mode;
            mode = Mode.Move;
            p.X = Snap(p.X);
            p.Y = Snap(p.Y);
            panStartPoint = p;
            tempLine = null;
            tempRectangle = null;
            selectionRect = null;
            overlay.Capture();
        }

        private void Move(Point p)
        {
            p.X = Snap(p.X);
            p.Y = Snap(p.Y);

            double x = p.X - panStartPoint.X;
            double y = p.Y - panStartPoint.Y;
            double z = zoomFactors[zoomIndex];

            if (x != 0.0 || y != 0.0)
            {
                x = x / z;
                y = y / z;
                Move(x, y, selected);
                panStartPoint = p;
            }  
        }

        private void FinishMove()
        {
            mode = tempMode;
            overlay.ReleaseCapture();
        }

        #endregion

        #region Move

        private void Move(double x, double y)
        {
            Push();
            Move(x, y, HaveSelected(selected) ? selected : logic);
        }

        public void MoveUp()
        {
            Move(0.0, -snapSize);
        }

        public void MoveDown()
        {
            Move(0.0, snapSize);
        }

        public void MoveLeft()
        {
            Move(-snapSize, 0.0);
        }

        public void MoveRight()
        {
            Move(snapSize, 0.0);
        }

        private static void Move(double x, double y, Block block)
        {
            if (block.Lines != null)
            {
                MoveLines(x, y, block.Lines);
            }

            if (block.Rectangles != null)
            {
                MoveRectangles(x, y, block.Rectangles);
            }

            if (block.Texts != null)
            {
                MoveTexts(x, y, block.Texts);
            }

            if (block.Blocks != null)
            {
                MoveBlocks(x, y, block.Blocks);
            }
        }

        private static void MoveLines(double x, double y, IEnumerable<Line> lines)
        {
            foreach (var line in lines)
            {
                line.X1 += x;
                line.Y1 += y;
                line.X2 += x;
                line.Y2 += y;
            }
        }

        private static void MoveRectangles(double x, double y, IEnumerable<Rectangle> rectangles)
        {
            foreach (var rectangle in rectangles)
            {
                Canvas.SetLeft(rectangle, Canvas.GetLeft(rectangle) + x);
                Canvas.SetTop(rectangle, Canvas.GetTop(rectangle) + y);
            }
        }

        private static void MoveTexts(double x, double y, IEnumerable<Grid> texts)
        {
            foreach (var text in texts)
            {
                Canvas.SetLeft(text, Canvas.GetLeft(text) + x);
                Canvas.SetTop(text, Canvas.GetTop(text) + y);
            }
        }

        private static void MoveBlocks(double x, double y, IEnumerable<Block> blocks)
        {
            foreach (var block in blocks)
            {
                MoveLines(x, y, block.Lines);
                MoveRectangles(x, y, block.Rectangles);
                MoveTexts(x, y, block.Texts);
                MoveBlocks(x, y, block.Blocks);
            }
        }

        #endregion

        #region Pan & Zoom

        private static void AdjustThickness(IEnumerable<Line> lines, double thickness)
        {
            foreach (var line in lines)
            {
                line.StrokeThickness = thickness;
            }
        }

        private static void AdjustThickness(IEnumerable<Rectangle> rectangles, double thickness)
        {
            foreach (var rectangle in rectangles)
            {
                rectangle.StrokeThickness = thickness;
            }
        }

        private static void AdjustThickness(Block parent, double thickness)
        {
            AdjustThickness(parent.Lines, thickness);
            AdjustThickness(parent.Rectangles, thickness);

            foreach (var block in parent.Blocks)
            {
                AdjustThickness(block, thickness);
            }
        }

        private void AdjustThickness(double zoom)
        {
            double gridThicknessZoomed = gridThickness / zoom;
            double frameThicknessZoomed = frameThickness / zoom;
            double lineThicknessZoomed = lineThickness / zoom;
            double selectionThicknessZoomed = selectionThickness / zoom;

            AdjustThickness(gridLines, gridThicknessZoomed);
            AdjustThickness(frameLines, frameThicknessZoomed);
            AdjustThickness(logic, lineThicknessZoomed);

            if (tempLine != null)
            {
                tempLine.StrokeThickness = lineThicknessZoomed;
            }

            if (tempRectangle != null)
            {
                tempRectangle.StrokeThickness = lineThicknessZoomed;
            }

            if (selectionRect != null)
            {
                selectionRect.StrokeThickness = selectionThicknessZoomed;
            }
        }

        private void ZoomTo(double x, double y, int oldZoomIndex)
        {
            double oldZoom = zoomFactors[oldZoomIndex];
            double newZoom = zoomFactors[zoomIndex];
            Zoom = newZoom;
            PanX = (x * oldZoom + PanX) - x * newZoom;
            PanY = (y * oldZoom + PanY) - y * newZoom;
        }

        private void ZoomTo(int delta, Point p)
        {
            if (delta > 0)
            {
                if (zoomIndex < maxZoomIndex)
                {
                    ZoomTo(p.X, p.Y, zoomIndex++);
                }
            }
            else
            {
                if (zoomIndex > 0)
                {
                    ZoomTo(p.X, p.Y, zoomIndex--);
                }
            }
        }

        private void InitPan(Point p)
        {
            tempMode = mode;
            mode = Mode.Pan;
            panStartPoint = p;
            tempLine = null;
            tempRectangle = null;
            selectionRect = null;
            overlay.Capture();
        }

        private void Pan(Point p)
        {
            PanX = PanX + p.X - panStartPoint.X;
            PanY = PanY + p.Y - panStartPoint.Y;
            panStartPoint = p;
        }

        private void FinishPan()
        {
            mode = tempMode;
            overlay.ReleaseCapture();
        }

        private void ResetPanAndZoom()
        {
            zoomIndex = defaultZoomIndex;
            Zoom = zoomFactors[zoomIndex];
            PanX = 0.0;
            PanY = 0.0;
        }

        #endregion

        #region Selection Rules

        private static bool HaveSelected(Block selected)
        {
            return (selected.Lines != null || selected.Rectangles != null || selected.Texts != null || selected.Blocks != null);
        }

        private static bool HaveOneTextSelected(Block selected)
        {
            return (selected.Lines == null && selected.Rectangles == null && selected.Blocks == null &&  selected.Texts != null && selected.Texts.Count == 1);
        }

        #endregion

        #region Selection Mode

        private void InitSelectionRect(Point p)
        {
            selectionStartPoint = p;
            double x = p.X;
            double y = p.Y;
            selectionRect = CreateSelectionRectangle(selectionThickness / Zoom, x, y, 0.0, 0.0);
            overlay.Add(selectionRect);
            overlay.Capture();
        }

        private void MoveSelectionRect(Point p)
        {
            double sx = selectionStartPoint.X;
            double sy = selectionStartPoint.Y;
            double x = p.X;
            double y = p.Y;
            double width = Math.Abs(sx - x);
            double height = Math.Abs(sy - y);
            Canvas.SetLeft(selectionRect, Math.Min(sx, x));
            Canvas.SetTop(selectionRect, Math.Min(sy, y));
            selectionRect.Width = width;
            selectionRect.Height = height;
        }

        private void FinishSelectionRect()
        {
            double x = Canvas.GetLeft(selectionRect);
            double y = Canvas.GetTop(selectionRect);
            double width = selectionRect.Width;
            double height = selectionRect.Height;

            CancelSelectionRect();
            HitTest(new Rect(x, y, width, height), sheet, logic, selected);
        }

        private void CancelSelectionRect()
        {
            overlay.ReleaseCapture();
            overlay.Remove(selectionRect);
            selectionRect = null;
        }

        #endregion

        #region Selection

        private static void Select(Block parent)
        {
            foreach (var line in parent.Lines)
            {
                line.Stroke = Brushes.Red;
            }

            foreach (var rectangle in parent.Rectangles)
            {
                rectangle.Stroke = Brushes.Red;
                rectangle.Fill = rectangle.Fill == Brushes.Transparent ? Brushes.Transparent : Brushes.Red;
            }

            foreach (var text in parent.Texts)
            {
                GetTextBlock(text).Foreground = Brushes.Red;
            }

            foreach (var block in parent.Blocks)
            {
                Select(block);
            }
        }

        private static void Deselect(Block parent)
        {
            if (parent.Lines != null)
            {
                foreach (var line in parent.Lines)
                {
                    line.Stroke = Brushes.Black;
                }
            }

            if (parent.Rectangles != null)
            {
                foreach (var rectangle in parent.Rectangles)
                {
                    rectangle.Stroke = Brushes.Black;
                    rectangle.Fill = rectangle.Fill == Brushes.Transparent ? Brushes.Transparent : Brushes.Black;
                }
            }

            if (parent.Texts != null)
            {
                foreach (var text in parent.Texts)
                {
                    GetTextBlock(text).Foreground = Brushes.Black;
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

        private static void SelectAll(Block selected, Block logic)
        {
            selected.Init();

            foreach (var line in logic.Lines)
            {
                line.Stroke = Brushes.Red;
                selected.Lines.Add(line);
            }

            foreach (var rectangle in logic.Rectangles)
            {
                rectangle.Stroke = Brushes.Red;
                rectangle.Fill = rectangle.Fill == Brushes.Transparent ? Brushes.Transparent : Brushes.Red;
                selected.Rectangles.Add(rectangle);
            }

            foreach (var text in logic.Texts)
            {
                GetTextBlock(text).Foreground = Brushes.Red;
                selected.Texts.Add(text);
            }

            foreach (var parent in logic.Blocks)
            {
                foreach (var line in parent.Lines)
                {
                    line.Stroke = Brushes.Red;
                }

                foreach (var rectangle in parent.Rectangles)
                {
                    rectangle.Stroke = Brushes.Red;
                    rectangle.Fill = rectangle.Fill == Brushes.Transparent ? Brushes.Transparent : Brushes.Red;
                }

                foreach (var text in parent.Texts)
                {
                    GetTextBlock(text).Foreground = Brushes.Red;
                }

                foreach(var block in parent.Blocks)
                {
                    Select(block);
                }

                selected.Blocks.Add(parent);
            }
        }

        private static void DeselectAll(Block selected)
        {
            if (selected.Lines != null)
            {
                foreach (var line in selected.Lines)
                {
                    line.Stroke = Brushes.Black;
                }

                selected.Lines = null;
            }

            if (selected.Rectangles != null)
            {
                foreach (var rectangle in selected.Rectangles)
                {
                    rectangle.Stroke = Brushes.Black;
                    rectangle.Fill = rectangle.Fill == Brushes.Transparent ? Brushes.Transparent : Brushes.Black;
                }

                selected.Rectangles = null;
            }

            if (selected.Texts != null)
            {
                foreach (var text in selected.Texts)
                {
                    GetTextBlock(text).Foreground = Brushes.Black;
                }

                selected.Texts = null;
            }

            if (selected.Blocks != null)
            {
                foreach (var parent in selected.Blocks)
                {
                    foreach (var line in parent.Lines)
                    {
                        line.Stroke = Brushes.Black;
                    }

                    foreach (var rectangle in parent.Rectangles)
                    {
                        rectangle.Stroke = Brushes.Black;
                        rectangle.Fill = rectangle.Fill == Brushes.Transparent ? Brushes.Transparent : Brushes.Black;
                    }

                    foreach (var text in parent.Texts)
                    {
                        GetTextBlock(text).Foreground = Brushes.Black;
                    }

                    foreach (var block in parent.Blocks)
                    {
                        Deselect(block);
                    }
                }

                selected.Blocks = null;
            }
        }

        public void SelecteAll()
        {
            SelectAll(selected, logic);
        }

        #endregion

        #region HitTest

        private static void HitTest(Rect rect, ISheet sheet, Block parent, Block selected)
        {
            selected.Init();

            if (parent.Lines != null)
            {
                HitTestLines(parent, selected, rect, false, true);
            }

            if (parent.Rectangles != null)
            {
                HitTestRectangles(parent, selected, rect, false, true, sheet.GetParent());
            }

            if (parent.Texts != null)
            {
                HitTestTexts(parent, selected, rect, false, true, sheet.GetParent());
            }

            if (parent.Blocks != null)
            {
                HitTestBlocks(parent, selected, rect, false, true, false, sheet.GetParent());
            }

            if (selected.Lines.Count == 0)
            {
                selected.Lines = null;
            }

            if (selected.Rectangles.Count == 0)
            {
                selected.Rectangles = null;
            }

            if (selected.Texts.Count == 0)
            {
                selected.Texts = null;
            }

            if (selected.Blocks.Count == 0)
            {
                selected.Blocks = null;
            }
        }

        private static void HitTest(Point p, double size, ISheet sheet, Block parent, Block selected, bool selectInsideBlock)
        {
            selected.Init();

            var rect = new Rect(p.X - size, p.Y - size, 2 * size, 2 * size);

            if (parent.Lines != null)
            {
                HitTestLines(parent, selected, rect, true, true);
            }

            if (selected.Lines.Count <= 0)
            {
                if (parent.Rectangles != null)
                {
                    HitTestRectangles(parent, selected, rect, true, true, sheet.GetParent());
                }
            }

            if (selected.Lines.Count <= 0 && selected.Rectangles.Count <= 0)
            {
                if (parent.Texts != null)
                {
                    HitTestTexts(parent, selected, rect, true, true, sheet.GetParent());
                }
            }

            if (selected.Lines.Count <= 0 && selected.Rectangles.Count <= 0 && selected.Texts.Count <= 0)
            {
                if (parent.Blocks != null)
                {
                    HitTestBlocks(parent, selected, rect, true, true, selectInsideBlock, sheet.GetParent());
                }
            }

            if (selected.Lines.Count == 0)
            {
                selected.Lines = null;
            }

            if (selected.Rectangles.Count == 0)
            {
                selected.Rectangles = null;
            }

            if (selected.Texts.Count == 0)
            {
                selected.Texts = null;
            }

            if (selected.Blocks.Count == 0)
            {
                selected.Blocks = null;
            }
        }

        private static bool HitTestLines(Block parent, Block selected, Rect rect, bool onlyFirst, bool select)
        {
            foreach (var line in parent.Lines)
            {
                var bounds = VisualTreeHelper.GetContentBounds(line);
                if (rect.IntersectsWith(bounds))
                {
                    if (select)
                    {
                        line.Stroke = Brushes.Red;
                        selected.Lines.Add(line);
                    }

                    if (onlyFirst)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool HitTestRectangles(Block parent, Block selected, Rect rect, bool onlyFirst, bool select, UIElement relative)
        {
            foreach (var rectangle in parent.Rectangles)
            {
                var bounds = VisualTreeHelper.GetContentBounds(rectangle);
                var offset = rectangle.TranslatePoint(new Point(0, 0), relative);
                bounds.Offset(offset.X, offset.Y);
                if (rect.IntersectsWith(bounds))
                {
                    if (select)
                    {
                        rectangle.Stroke = Brushes.Red;
                        rectangle.Fill = rectangle.Fill == Brushes.Transparent ? Brushes.Transparent : Brushes.Red;
                        selected.Rectangles.Add(rectangle);
                    }

                    if (onlyFirst)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool HitTestTexts(Block parent, Block selected, Rect rect, bool onlyFirst, bool select, UIElement relative)
        {
            foreach (var text in parent.Texts)
            {
                var bounds = VisualTreeHelper.GetContentBounds(text);
                var offset = text.TranslatePoint(new Point(0, 0), relative);
                bounds.Offset(offset.X, offset.Y);
                if (rect.IntersectsWith(bounds))
                {
                    if (select)
                    {
                        GetTextBlock(text).Foreground = Brushes.Red;
                        selected.Texts.Add(text);
                    }

                    if (onlyFirst)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool HitTestBlocks(Block parent, Block selected, Rect rect, bool onlyFirst, bool select, bool selectInsideBlock, UIElement relative)
        {
            foreach (var block in parent.Blocks)
            {
                bool isSelected = HitTestBlock(block, selected, rect, true, selectInsideBlock, relative);

                if (isSelected)
                {
                    if (select && !selectInsideBlock)
                    {
                        selected.Blocks.Add(block);
                        Select(block);
                    }

                    if (onlyFirst)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool HitTestBlock(Block parent, Block selected, Rect rect, bool onlyFirst, bool selectInsideBlock, UIElement relative)
        {
            bool result = false;

            result = HitTestLines(parent, selected, rect, onlyFirst, selectInsideBlock);
            if (result && onlyFirst)
            {
                return true;
            }

            result = HitTestRectangles(parent, selected, rect, onlyFirst, selectInsideBlock, relative);
            if (result && onlyFirst)
            {
                return true;
            }

            result = HitTestTexts(parent, selected, rect, onlyFirst, selectInsideBlock, relative);
            if (result && onlyFirst)
            {
                return true;
            }

            foreach(var block in parent.Blocks)
            {
                result = HitTestBlock(block, selected, rect, onlyFirst, selectInsideBlock, relative);
                if (result && onlyFirst)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Line Mode

        private void InitTempLine(Point p)
        {
            Push();
            double x = Snap(p.X);
            double y = Snap(p.Y);
            tempLine = CreateLine(lineThickness / Zoom, x, y, x, y);
            overlay.Add(tempLine);
            overlay.Capture();
        }

        private void MoveTempLine(Point p)
        {
            double x = Snap(p.X);
            double y = Snap(p.Y);
            if (Math.Round(x, 1) != Math.Round(tempLine.X2, 1) || Math.Round(y, 1) != Math.Round(tempLine.Y2, 1))
            {
                tempLine.X2 = x;
                tempLine.Y2 = y;
            }
        }

        private void FinishTempLine()
        {
            if (Math.Round(tempLine.X1, 1) == Math.Round(tempLine.X2, 1) &&
                Math.Round(tempLine.Y1, 1) == Math.Round(tempLine.Y2, 1))
            {
                CancelTempLine();
            }
            else
            {
                overlay.ReleaseCapture();
                overlay.Remove(tempLine);
                logic.Lines.Add(tempLine);
                sheet.Add(tempLine);
                tempLine = null;
            }
        }

        private void CancelTempLine()
        {
            Pop();
            overlay.ReleaseCapture();
            overlay.Remove(tempLine);
            tempLine = null;
        }

        #endregion

        #region Rectangle Mode

        private void InitTempRect(Point p)
        {
            Push();
            double x = Snap(p.X);
            double y = Snap(p.Y);
            selectionStartPoint = new Point(x, y);
            tempRectangle = CreateRectangle(selectionThickness / Zoom, x, y, 0.0, 0.0, true);
            overlay.Add(tempRectangle);
            overlay.Capture();
        }

        private void MoveTempRect(Point p)
        {
            double sx = selectionStartPoint.X;
            double sy = selectionStartPoint.Y;
            double x = Snap(p.X);
            double y = Snap(p.Y);
            double width = Math.Abs(sx - x);
            double height = Math.Abs(sy - y);
            Canvas.SetLeft(tempRectangle, Math.Min(sx, x));
            Canvas.SetTop(tempRectangle, Math.Min(sy, y));
            tempRectangle.Width = width;
            tempRectangle.Height = height;
        }

        private void FinishTempRect()
        {
            double x = Canvas.GetLeft(tempRectangle);
            double y = Canvas.GetTop(tempRectangle);
            double width = tempRectangle.Width;
            double height = tempRectangle.Height;

            if (width == 0.0 || height == 0.0)
            {
                CancelTempRect();
            }
            else
            {
                overlay.ReleaseCapture();
                overlay.Remove(tempRectangle);
                logic.Rectangles.Add(tempRectangle);
                sheet.Add(tempRectangle);
                tempRectangle = null;
            }
        }

        private void CancelTempRect()
        {
            Pop();
            overlay.ReleaseCapture();
            overlay.Remove(tempRectangle);
            tempRectangle = null;
        }

        #endregion

        #region Edit Text

        private SheetWindow owner = null;
        private Action<string> okAction = null;
        private Action cancelAction = null;

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (owner != null)
            {
                owner.OK.Click -= OK_Click;
                owner.Cancel.Click -= Cancel_Click;
                owner.TextGrid.Visibility = Visibility.Collapsed;
                okAction(owner.TextValue.Text);
                Focus();
                mode = tempMode;
                okAction = null;
                cancelAction = null;
                owner = null;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (owner != null)
            {
                owner.OK.Click -= OK_Click;
                owner.Cancel.Click -= Cancel_Click;
                owner.TextGrid.Visibility = Visibility.Collapsed;
                owner = null;
                cancelAction();
                Focus();
                mode = tempMode;
                okAction = null;
                cancelAction = null;
            }
        }

        private T GetOwner<T>(DependencyObject reference) where T : class
        {
            if (reference == null)
                return null;

            var parent = VisualTreeHelper.GetParent(reference);
            while (!(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }

        private void EditText(Action<string> ok, Action cancel, string label, string text)
        {
            owner = this == null ? null : GetOwner<SheetWindow>(this);
            if (owner != null)
            {
                okAction = ok;
                cancelAction = cancel;
                tempMode = mode;
                mode = Mode.None;
                owner.TextLabel.Text = label;
                owner.TextValue.Text = text;
                owner.OK.Click += OK_Click;
                owner.Cancel.Click += Cancel_Click;
                owner.TextGrid.Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region Text

        private bool TryToEditText(Point p)
        {
            var temp = new Block();
            HitTest(p, hitSize, sheet, logic, temp, true);

            if (HaveOneTextSelected(temp))
            {
                var tb = GetTextBlock(temp.Texts[0]);

                Action<string> ok = (str) =>
                {
                    Push();
                    tb.Text = str;
                };

                Action cancel = () => { };

                EditText(ok, cancel, "Text:", tb.Text);

                Deselect(temp);
                return true;
            }

            Deselect(temp);
            return false;
        }

        private void CreateText(Point p)
        {
            double x = Snap(p.X);
            double y = Snap(p.Y);
            Push();
            var text = CreateText("Text", x, y, 30.0, 15.0, HorizontalAlignment.Center, VerticalAlignment.Center, 11.0);
            logic.Texts.Add(text);
            sheet.Add(text);
        }

        #endregion

        #region Block

        private static string SerializeAsBlock(int id, string name, Block parent)
        {
            var block = CreateSheet(id, name, parent.Lines, parent.Rectangles, parent.Texts, parent.Blocks);
            var sb = new StringBuilder();
            ItemSerializer.Serialize(sb, block, "");
            return sb.ToString();
        }

        private BlockItem CreateBlock(string name)
        {
            var text = SerializeAsBlock(0, name, selected);
            Delete();
            var block = ItemSerializer.Deserialize(text);
            Load(block, true);
            return block.Blocks.FirstOrDefault();
        }

        public void CreateBlock()
        {
            if (HaveSelected(selected))
            {
                Action<string> ok = (str) =>
                {
                    Push();
                    var block = CreateBlock(str);
                    AddToLibrary(block);
                };

                Action cancel = () => { };

                EditText(ok, cancel, "Name:", "BLOCK0");
            }
        }

        public void BreakBlock()
        {
            if (HaveSelected(selected))
            {
                var text = ItemSerializer.Serialize(CreateSheet(0, "SELECTED", selected.Lines, selected.Rectangles, selected.Texts, selected.Blocks));
                var parent = ItemSerializer.Deserialize(text);

                Push();
                Delete();

                double thickness = lineThickness / Zoom;

                Load(sheet, parent.Texts, logic, selected, true, thickness);
                Load(sheet, parent.Lines, logic, selected, true, thickness);
                Load(sheet, parent.Rectangles, logic, selected, true, thickness);

                foreach (var block in parent.Blocks)
                {
                    Load(sheet, block.Texts, logic, selected, true, thickness);
                    Load(sheet, block.Lines, logic, selected, true, thickness);
                    Load(sheet, block.Rectangles, logic, selected, true, thickness);
                    Load(sheet, block.Blocks, logic, selected, true, thickness);
                }
            }
        }

        #endregion

        #region Events

        private void Overlay_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Focus();

            if (mode == Mode.None)
            {
                return;
            }

            if (HaveSelected(selected) && CanInitMove(e.GetPosition(overlay.GetParent())))
            {
                InitMove(e.GetPosition(this));
                return;
            }

            DeselectAll(selected);

            if (mode == Mode.Selection)
            {
                HitTest(e.GetPosition(overlay.GetParent()), hitSize, sheet, logic, selected, false);
                if (!HaveSelected(selected))
                {
                    InitSelectionRect(e.GetPosition(overlay.GetParent()));
                }
                else
                {
                    InitMove(e.GetPosition(this));
                }
            }
            else if (mode == Mode.Insert && !overlay.IsCaptured)
            {
                Insert(e.GetPosition(overlay.GetParent()));
            }
            else if (mode == Mode.Line && !overlay.IsCaptured)
            {
                InitTempLine(e.GetPosition(overlay.GetParent()));
            }
            else if (mode == Mode.Line && overlay.IsCaptured)
            {
                FinishTempLine();
            }
            else if (mode == Mode.Rectangle && !overlay.IsCaptured)
            {
                InitTempRect(e.GetPosition(overlay.GetParent()));
            }
            else if (mode == Mode.Rectangle && overlay.IsCaptured)
            {
                FinishTempRect();
            }
            else if (mode == Mode.Pan && overlay.IsCaptured)
            {
                FinishPan();
            }
            else if (mode == Mode.Text && !overlay.IsCaptured)
            {
                CreateText(e.GetPosition(overlay.GetParent()));
            }
        }

        private void Overlay_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (mode == Mode.Selection && overlay.IsCaptured)
            {
                FinishSelectionRect();
            }
            else if (mode == Mode.Move && overlay.IsCaptured)
            {
                FinishMove();
            }
        }

        private void Overlay_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (mode == Mode.Selection && overlay.IsCaptured)
            {
                MoveSelectionRect(e.GetPosition(overlay.GetParent()));
            }
            else if (mode == Mode.Line && overlay.IsCaptured)
            {
                MoveTempLine(e.GetPosition(overlay.GetParent()));
            }
            else if (mode == Mode.Rectangle && overlay.IsCaptured)
            {
                MoveTempRect(e.GetPosition(overlay.GetParent()));
            }
            else if (mode == Mode.Pan && overlay.IsCaptured)
            {
                Pan(e.GetPosition(this));
            }
            else if (mode == Mode.Move && overlay.IsCaptured)
            {
                Move(e.GetPosition(this));
            }
        }

        private void Overlay_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Focus();

            if (mode == Mode.None)
            {
                return;
            }

            if (TryToEditText(e.GetPosition(overlay.GetParent())))
            {
                e.Handled = true;
                return;
            }

            DeselectAll(selected);

            if (mode == Mode.Selection && overlay.IsCaptured)
            {
                CancelSelectionRect();
            }
            else if (mode == Mode.Line && overlay.IsCaptured)
            {
                CancelTempLine();
            }
            else if (mode == Mode.Rectangle && overlay.IsCaptured)
            {
                CancelTempRect();
            }
            else if (!overlay.IsCaptured)
            {
                InitPan(e.GetPosition(this));
            }
        }

        private void Overlay_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (mode == Mode.Pan && overlay.IsCaptured)
            {
                FinishPan();
            }
        }

        private void UserControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ZoomTo(e.Delta, e.GetPosition(overlay.GetParent()));
        }

        private void UserControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle && e.ClickCount == 2)
            {
                ResetPanAndZoom();
            }
        }

        #endregion
    }
}
