using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public List<TextItem> Texts { get; set; }
        public List<BlockItem> Blocks { get; set; }
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
            Serialize(sb, block.Texts, "");
            Serialize(sb, block.Blocks, "");

            return sb.ToString();
        }

        #endregion

        #region Deserialize
     
        private static BlockItem Deserialize(string[] lines, int length, ref int end, string name, int id)
        {
            var sheet = new BlockItem()
            {
                Name = name,
                Lines = new List<LineItem>(),
                Texts = new List<TextItem>(),
                Blocks = new List<BlockItem>()
            };

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
        public List<Grid> Texts { get; set; }
        public List<Block> Blocks { get; set; }
        public void Init()
        {
            Lines = new List<Line>();
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
        Pan,
        Move,
        Line,
        Text,
        AndGate,
        OrGate
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
        private double gridThickness = 1.0;
        private double selectionThickness = 1.0;
        private double lineThickness = 2.0;
        private Point panStartPoint;
        private Line tempLine = null;
        private Point selectionStartPoint;
        private Rectangle selectionRect = null;
        private double hitSize = 3.5;
        private List<Line> gridLines = new List<Line>();
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
                CreateGrid(back, 300.0, 0.0, 600.0, 750.0, gridSize, gridThickness);
                AdjustThickness(gridLines, gridThickness / zoomFactors[zoomIndex]);
                Focus();
            };
        }

        #endregion

        #region Grid

        private void AddGridLine(ISheet sheet, double thickness, double x1, double y1, double x2, double y2)
        {
            var line = new Line()
            {
                Stroke = Brushes.LightGray,
                StrokeThickness = thickness,
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2
            };
            gridLines.Add(line);
            sheet.Add(line);
        }

        private void CreateGrid(ISheet sheet, double startX, double startY, double width, double height, double gridSize, double gridThickness)
        {
            for (double y = startY + gridSize; y < height + startY; y += gridSize)
            {
                AddGridLine(sheet, gridThickness, startX, y, width + startX, y);
            }

            for (double x = startX + gridSize; x < startX + width; x += gridSize)
            {
                AddGridLine(sheet, gridThickness, x, startY, x, height + startY);
            }
        }

        #endregion

        #region Blocks

        private void AddAndGate(Point p)
        {
            Push();
            double x = Snap(p.X);
            double y = Snap(p.Y);
            logic.Blocks.Add(CreateAndGateBlock(sheet, x, y, lineThickness / Zoom));
        }

        private void AddOrGate(Point p)
        {
            Push();
            double x = Snap(p.X);
            double y = Snap(p.Y);
            logic.Blocks.Add(CreateOrGateBlock(sheet, x, y, 1, lineThickness / Zoom));
        }

        private static Block CreateAndGateBlock(ISheet sheet, double x, double y, double thickness)
        {
            return CreateGenericGateBlock(sheet, "AND", x, y, "&", thickness);
        }

        private static Block CreateOrGateBlock(ISheet sheet, double x, double y, double count, double thickness)
        {
            return CreateGenericGateBlock(sheet, "OR", x, y, "≥" + count.ToString(), thickness);
        }

        private static Block CreateGenericGateBlock(ISheet sheet, string name, double x, double y, string text, double thickness)
        {
            var block = new Block() { Name = name };
            block.Init();

            AddTextToBlock(sheet, block, text, x, y, 30.0, 30.0, HorizontalAlignment.Center, VerticalAlignment.Center, 14.0);
            AddLineToBlock(sheet, block, thickness, x, y, x + 30.0, y);
            AddLineToBlock(sheet, block, thickness, x, y + 30.0, x + 30.0, y + 30.0);
            AddLineToBlock(sheet, block, thickness, x, y, x, y + 30.0);
            AddLineToBlock(sheet, block, thickness, x + 30.0, y, x + 30.0, y + 30.0);

            return block;
        }

        private static void AddTextToBlock(ISheet sheet, Block block, string str, double x, double y, double width, double height, HorizontalAlignment halign, VerticalAlignment valign, double size)
        {
            var text = CreateText(str, x, y, width, height, halign, valign, size);
            block.Texts.Add(text);
            sheet.Add(text);
        }

        private static void AddLineToBlock(ISheet sheet, Block block, double thickness, double x1, double y1, double x2, double y2)
        {
            var line = CreateLine(thickness, x1, y1, x2, y2);
            block.Lines.Add(line);
            sheet.Add(line);
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
            var blockItem = new BlockItem()
            {
                Name = parent.Name,
                Lines = new List<LineItem>(),
                Texts = new List<TextItem>(),
                Blocks = new List<BlockItem>()
            };

            foreach (var line in parent.Lines)
            {
                blockItem.Lines.Add(SerializeLine(line));
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

        private static BlockItem CreateSheet(int id, string name, IEnumerable<Line> lines, IEnumerable<Grid> texts, IEnumerable<Block> blocks)
        {
            var sheet = new BlockItem()
            {
                Id = id,
                Name = name,
                Lines = new List<LineItem>(),
                Texts = new List<TextItem>(),
                Blocks = new List<BlockItem>()
            };

            if (lines != null)
            {
                foreach (var line in lines)
                {
                    sheet.Lines.Add(SerializeLine(line));
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
            return CreateSheet(0, "LOGIC", logic.Lines, logic.Texts, logic.Blocks);
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
            grid.Background = Brushes.White;
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

        private static Rectangle CreateSelectionRect(double thickness, double x, double y, double width, double height)
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

            foreach (var childBlockItem in blockItem.Blocks)
            {
                LoadBlockItem(sheet, block, childBlockItem, select, thickness);
            }

            parent.Blocks.Add(block);

            return block;
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
                    RemoveTexts(sheet, block.Texts);
                    RemoveBlocks(sheet, block.Blocks);

                    parent.Blocks.Remove(block);
                }

                selected.Blocks = null;
            }
        }

        private void Delete()
        {
            if (HaveSelected())
            {
                Push();
                Remove(sheet, logic, selected);
            }
        }

        private void Reset()
        {
            RemoveLines(sheet, logic.Lines);
            RemoveTexts(sheet, logic.Texts);
            RemoveBlocks(sheet, logic.Blocks);

            logic.Lines.Clear();
            logic.Texts.Clear();
            logic.Blocks.Clear();

            selected.Lines = null;
            selected.Texts = null;
            selected.Blocks = null;
        }

        #endregion

        #region Move

        private void InitMove(Point p)
        {
            Push();
            tempMode = mode;
            mode = Mode.Move;
            p.X = Snap(p.X);
            p.Y = Snap(p.Y);
            panStartPoint = p;
            tempLine = null;
            selectionRect = null;
            overlay.Capture();
        }

        private void Move(Point p)
        {
            p.X = Snap(p.X);
            p.Y = Snap(p.Y);
            Move(p.X - panStartPoint.X, p.Y - panStartPoint.Y, selected);
            panStartPoint = p;
        }

        private void FinishMove()
        {
            mode = tempMode;
            overlay.ReleaseCapture();
        }

        private void Move(double x, double y)
        {
            Push();
            Move(x, y, HaveSelected() ? selected : logic);
        }

        private static void Move(double x, double y, Block block)
        {
            if (block.Lines != null)
            {
                MoveLines(x, y, block.Lines);
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

        private static void AdjustThickness(Block parent, double thickness)
        {
            AdjustThickness(parent.Lines, thickness);

            foreach (var block in parent.Blocks)
            {
                AdjustThickness(block, thickness);
            }
        }

        private void AdjustThickness(double zoom)
        {
            double gridThicknessZoomed = gridThickness / zoom;
            double lineThicknessZoomed = lineThickness / zoom;
            double selectionThicknessZoomed = selectionThickness / zoom;

            AdjustThickness(gridLines, gridThicknessZoomed);
            AdjustThickness(logic, lineThicknessZoomed);

            if (tempLine != null)
            {
                tempLine.StrokeThickness = lineThicknessZoomed;
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

        #region Selection

        private void InitSelectionRect(Point p)
        {
            selectionStartPoint = p;
            double x = p.X;
            double y = p.Y;
            selectionRect = CreateSelectionRect(selectionThickness / Zoom, x, y, 0.0, 0.0);
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

        private static void Select(Block parent)
        {
            foreach (var line in parent.Lines)
            {
                line.Stroke = Brushes.Red;
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
            foreach (var line in parent.Lines)
            {
                line.Stroke = Brushes.Black;
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

        private static void SelectAll(Block selected, Block logic)
        {
            selected.Init();

            foreach (var line in logic.Lines)
            {
                line.Stroke = Brushes.Red;
                selected.Lines.Add(line);
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

        #endregion

        #region HitTest

        private static void HitTest(Rect rect, ISheet sheet, Block parent, Block selected)
        {
            selected.Init();

            if (parent.Lines != null)
            {
                HitTestLines(parent, selected, rect, false);
            }

            if (parent.Texts != null)
            {
                HitTestTexts(parent, selected, rect, false, sheet.GetParent());
            }

            if (parent.Blocks != null)
            {
                HitTestBlocks(parent, selected, rect, false, sheet.GetParent());
            }

            if (selected.Lines.Count == 0)
            {
                selected.Lines = null;
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

        private static void HitTest(Point p, double size, ISheet sheet, Block parent, Block selected)
        {
            selected.Init();

            var rect = new Rect(p.X - size, p.Y - size, 2 * size, 2 * size);

            if (parent.Lines != null)
            {
                HitTestLines(parent, selected, rect, true);
            }

            if (selected.Lines.Count <= 0)
            {
                if (parent.Texts != null)
                {
                    HitTestTexts(parent, selected, rect, true, sheet.GetParent());
                }
            }

            if (selected.Lines.Count <= 0 && selected.Texts.Count <= 0)
            {
                if (parent.Blocks != null)
                {
                    HitTestBlocks(parent, selected, rect, true, sheet.GetParent());
                }
            }

            if (selected.Lines.Count == 0)
            {
                selected.Lines = null;
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

        private static void HitTestLines(Block parent, Block selected, Rect rect, bool onlyFirst)
        {
            foreach (var line in parent.Lines)
            {
                var bounds = VisualTreeHelper.GetContentBounds(line);
                if (rect.IntersectsWith(bounds))
                {
                    line.Stroke = Brushes.Red;
                    selected.Lines.Add(line);

                    if (onlyFirst)
                    {
                        break;
                    }
                }
            }
        }

        private static void HitTestTexts(Block parent, Block selected, Rect rect, bool onlyFirst, UIElement relative)
        {
            foreach (var text in parent.Texts)
            {
                var bounds = VisualTreeHelper.GetContentBounds(text);
                var offset = text.TranslatePoint(new Point(0, 0), relative);
                bounds.Offset(offset.X, offset.Y);
                if (rect.IntersectsWith(bounds))
                {
                    GetTextBlock(text).Foreground = Brushes.Red;
                    selected.Texts.Add(text);

                    if (onlyFirst)
                    {
                        break;
                    }
                }
            }
        }

        private static void HitTestBlocks(Block parent, Block selected, Rect rect, bool onlyFirst, UIElement relative)
        {
            foreach (var block in parent.Blocks)
            {
                bool isSelected = HitTestBlock(block, selected, rect, relative);

                if (isSelected)
                {
                    selected.Blocks.Add(block);
                    Select(block);

                    if (onlyFirst)
                    {
                        break;
                    }
                }
            }
        }

        private static bool HitTestBlock(Block parent, Block selected, Rect rect, UIElement relative)
        {
            bool isSelected = false;

            foreach (var line in parent.Lines)
            {
                var bounds = VisualTreeHelper.GetContentBounds(line);
                if (rect.IntersectsWith(bounds))
                {
                    isSelected = true;
                    break;
                }
            }

            if (!isSelected)
            {
                foreach (var text in parent.Texts)
                {
                    var bounds = VisualTreeHelper.GetContentBounds(text);
                    var offset = text.TranslatePoint(new Point(0, 0), relative);
                    bounds.Offset(offset.X, offset.Y);
                    if (rect.IntersectsWith(bounds))
                    {
                        isSelected = true;
                        break;
                    }
                }
            }

            if (!isSelected)
            {
                foreach(var block in parent.Blocks)
                {
                    if (HitTestBlock(block, selected, rect, relative))
                    {
                        isSelected = true;
                        break;
                    }
                }
            }

            return isSelected;
        }

        #endregion

        #region Temp Line

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

        #region Undo/Redo

        private void Push()
        {
            undos.Push(ItemSerializer.Serialize(CreateSheet()));
        }

        private void Pop()
        {
            undos.Pop();
        }

        private void Undo()
        {
            if (undos.Count > 0)
            {
                redos.Push(ItemSerializer.Serialize(CreateSheet()));
                Reset();
                Load(ItemSerializer.Deserialize(undos.Pop()), false);
            }
        }

        private void Redo()
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

        private bool HaveSelected()
        {
            return (selected.Lines != null || selected.Texts != null || selected.Blocks != null);
        }

        private void Cut()
        {
            Copy();
            Push();

            if (HaveSelected())
            {
                Delete();
            }
            else
            {
                Reset();
            }
        }

        private void Copy()
        {
            var text = ItemSerializer.Serialize(HaveSelected() ?
                CreateSheet(0, "SELECTED", selected.Lines, selected.Texts, selected.Blocks) : CreateSheet());
            Clipboard.SetData(DataFormats.UnicodeText, text);
        }

        private void Paste()
        {
            var text = (string)Clipboard.GetData(DataFormats.UnicodeText);
            Load(ItemSerializer.Deserialize(text), true);
        }

        private static string CreateBlock(int id, string name, Block parent)
        {
            var block = CreateSheet(id, name, parent.Lines, parent.Texts, parent.Blocks);
            var sb = new StringBuilder();

            ItemSerializer.Serialize(sb, block, "");
            return sb.ToString();
        }

        #endregion

        #region File Dialogs

        private void Open()
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

        private void Save()
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

        #endregion

        #region Events

        private bool CanInitMove(Point p)
        {
            var temp = new Block();
            HitTest(p, hitSize, sheet, selected, temp);

            if (temp.Lines != null || temp.Texts != null || temp.Blocks != null)
            {
                return true;
            }

            return false;
        }

        private void Overlay_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (HaveSelected() && CanInitMove(e.GetPosition(overlay.GetParent())))
            {
                InitMove(e.GetPosition(this));
                return;
            }

            DeselectAll(selected);

            if (mode == Mode.AndGate)
            {
                AddAndGate(e.GetPosition(overlay.GetParent()));
            }
            else if (mode == Mode.OrGate)
            {
                AddOrGate(e.GetPosition(overlay.GetParent()));
            }
            else if (mode == Mode.Selection)
            {
                HitTest(e.GetPosition(overlay.GetParent()), hitSize, sheet, logic, selected);
                if (!HaveSelected())
                {
                    InitSelectionRect(e.GetPosition(overlay.GetParent()));
                }
                else
                {
                    InitMove(e.GetPosition(this));
                }
            }
            else if (mode == Mode.Line && !overlay.IsCaptured)
            {
                InitTempLine(e.GetPosition(overlay.GetParent()));
            }
            else if (mode == Mode.Line && overlay.IsCaptured)
            {
                FinishTempLine();
            }
            else if (mode == Mode.Pan && overlay.IsCaptured)
            {
                FinishPan();
            }
            else if (mode == Mode.Text && !overlay.IsCaptured)
            {
                var p = e.GetPosition(overlay.GetParent());
                double x = Snap(p.X);
                double y = Snap(p.Y);
                Push();
                AddTextToBlock(sheet, logic, "Text", x, y, 30.0, 15.0, System.Windows.HorizontalAlignment.Center, System.Windows.VerticalAlignment.Center, 11.0);
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
            DeselectAll(selected);

            if (mode == Mode.Selection && overlay.IsCaptured)
            {
                CancelSelectionRect();
            }
            else if (mode == Mode.Line && overlay.IsCaptured)
            {
                CancelTempLine();
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

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool ctrl = (Keyboard.Modifiers & ModifierKeys.Control) > 0;

            switch (e.Key)
            {
                // Ctrl+Z: Undo
                case Key.Z:
                    if (ctrl)
                    {
                        Undo();
                    }
                    break;
                // Ctrl+Y: Redo
                case Key.Y:
                    if (ctrl)
                    {
                        Redo();
                    }
                    break;
                // Ctrl+X: Cut
                case Key.X:
                    if (ctrl)
                    {
                        Cut();
                    }
                    break;
                // Ctrl+C: Copy
                case Key.C:
                    if (ctrl)
                    {
                        Copy();
                    }
                    break;
                // Ctrl+V: Paste
                case Key.V:
                    if (ctrl)
                    {
                        Paste();
                    }
                    break;
                // Del: Delete
                case Key.Delete:
                    Delete();
                    break;
                // Ctrl+A: Select All
                // A: Mode AndGate
                case Key.A:
                    if (ctrl)
                    {
                        SelectAll(selected, logic);
                    }
                    else
                    {
                        mode = Mode.AndGate;
                    }
                    break;
                // B: Create Block
                case Key.B:
                    {
                        if (HaveSelected())
                        {
                            var text = CreateBlock(0, "BLOCK0", selected);
                            Clipboard.SetData(DataFormats.UnicodeText, text);
                        }
                    }
                    break;
                // R: Reset
                case Key.R:
                    Push();
                    Reset();
                    break;
                // Ctrls+S: Save
                // S: Mode Selection
                case Key.S:
                    if (ctrl)
                    {
                        Save();
                    }
                    else
                    {
                        mode = Mode.Selection;
                    }
                    break;
                // Ctrl+O: Open
                // O: Mode OrGate
                case Key.O:
                    if (ctrl)
                    {
                        Open();
                    }
                    else
                    {
                        mode = Mode.OrGate;
                    }
                    break;
                // L: Mode Line
                case Key.L:
                    mode = Mode.Line;
                    break;
                // T: Mode Text
                case Key.T:
                    mode = Mode.Text;
                    break;
                // N: Mode None
                case Key.N:
                    mode = Mode.None;
                    break;
                // Up: Move Up
                case Key.Up:
                    Move(0.0, -snapSize);
                    break;
                // Down: Move Down
                case Key.Down:
                    Move(0.0, snapSize);
                    break;
                // Left: Move Left
                case Key.Left:
                    Move(-snapSize, 0.0);
                    break;
                // Right: Move Right
                case Key.Right:
                    Move(snapSize, 0.0);
                    break;
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
