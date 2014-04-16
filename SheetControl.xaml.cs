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

        #endregion

        #region Serialize

        private static void Serialize(StringBuilder sb, LineItem line)
        {
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

        private static void Serialize(StringBuilder sb, TextItem text)
        {
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

        private static void Serialize(StringBuilder sb, BlockItem block)
        {
            sb.Append("BLOCK");
            sb.Append(modelSeparator);
            sb.Append(block.Id);
            sb.Append(modelSeparator);
            sb.Append(block.Name);
            sb.Append(lineSeparator);

            Serialize(sb, block.Lines);
            Serialize(sb, block.Texts);
            Serialize(sb, block.Blocks);

            sb.Append("END");
            sb.Append(lineSeparator);
        }

        private static void Serialize(StringBuilder sb, List<LineItem> lines)
        {
            foreach (var line in lines)
            {
                Serialize(sb, line);
            }
        }

        private static void Serialize(StringBuilder sb, List<TextItem> texts)
        {
            foreach (var text in texts)
            {
                Serialize(sb, text);
            }
        }

        private static void Serialize(StringBuilder sb, List<BlockItem> blocks)
        {
            foreach (var block in blocks)
            {
                Serialize(sb, block);
            }
        }

        public static string Serialize(BlockItem block)
        {
            var sb = new StringBuilder();

            Serialize(sb, block.Lines);
            Serialize(sb, block.Texts);
            Serialize(sb, block.Blocks);

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
                string line = lines[end];
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
    }

    #endregion

    public partial class SheetControl : UserControl
    {
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
            Line,
            AndGate,
            OrGate
        }

        #endregion

        #region Fields

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

            logic = new Block() { Name = "LOGIC" };
            logic.Init();

            selected = new Block { Name = "SELECTED" };

            Loaded += (s, e) =>
            {
                CreateGrid();
                Focus();
            };
        }

        #endregion

        #region Sheet

        private void Add(UIElement element)
        {
            Sheet.Children.Add(element);
        }

        private void Remove(UIElement element)
        {
            Sheet.Children.Remove(element);
        }

        #endregion

        #region Grid

        private void CreateGrid()
        {
            double width = Sheet.ActualWidth;
            double height = Sheet.ActualHeight;

            for (double y = gridSize; y < height; y += gridSize)
            {
                var l = new Line() { Stroke = Brushes.LightGray, StrokeThickness = gridThickness, X1 = 0, Y1 = y, X2 = width, Y2 = y };
                gridLines.Add(l);
                Back.Children.Add(l);
            }

            for (double x = gridSize; x < width; x += gridSize)
            {
                var l = new Line() { Stroke = Brushes.LightGray, StrokeThickness = gridThickness, X1 = x, Y1 = 0, X2 = x, Y2 = height };
                gridLines.Add(l);
                Back.Children.Add(l);
            }

            AdjustThickness(zoomFactors[zoomIndex]);
        }

        #endregion

        #region Blocks

        private Block CreateGenericGateBlock(string name, double x, double y, string text)
        {
            var block = new Block() { Name = name };
            block.Init();

            AddTextToBlock(block, text, x, y, 30.0, 30.0, HorizontalAlignment.Center, VerticalAlignment.Center, 14.0);
            AddLineToBlock(block, x, y, x + 30.0, y);
            AddLineToBlock(block, x, y + 30.0, x + 30.0, y + 30.0);
            AddLineToBlock(block, x, y, x, y + 30.0);
            AddLineToBlock(block, x + 30.0, y, x + 30.0, y + 30.0);

            return block;
        }

        private Block CreateAndGateBlock(double x, double y)
        {
            return CreateGenericGateBlock("AND", x, y, "&");
        }

        private Block CreateOrGateBlock(double x, double y, double count)
        {
            return CreateGenericGateBlock("OR", x, y, "≥" + count.ToString());
        }

        private void AddLineToBlock(Block block, double x1, double y1, double x2, double y2)
        {
            var line = CreateLine(lineThickness / Zoom, x1, y1, x2, y2);
            block.Lines.Add(line);
            Add(line);
        }

        private void AddTextToBlock(Block block, string str, double x, double y, double width, double height, HorizontalAlignment halign, VerticalAlignment valign, double size)
        {
            var text = CreateText(str, x, y, width, height, halign, valign, size);
            block.Texts.Add(text);
            Add(text);
        }

        private void AddOrGate(Point p)
        {
            Push();
            double x = Snap(p.X);
            double y = Snap(p.Y);
            logic.Blocks.Add(CreateOrGateBlock(x, y, 1));
        }

        private void AddAndGate(Point p)
        {
            Push();
            double x = Snap(p.X);
            double y = Snap(p.Y);
            logic.Blocks.Add(CreateAndGateBlock(x, y));
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

        private static BlockItem SerializeBlock(Block block)
        {
            var blockItem = new BlockItem()
            {
                Name = block.Name,
                Lines = new List<LineItem>(),
                Texts = new List<TextItem>(),
                Blocks = new List<BlockItem>()
            };

            foreach (var line in block.Lines)
            {
                blockItem.Lines.Add(SerializeLine(line));
            }

            foreach (var text in block.Texts)
            {
                blockItem.Texts.Add(SerializeText(text));
            }

            return blockItem;
        }

        private static BlockItem CreateSheet(IEnumerable<Line> lines,
                                      IEnumerable<Grid> texts,
                                      IEnumerable<Block> blocks)
        {
            var sheet = new BlockItem()
            {
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
            return CreateSheet(logic.Lines, logic.Texts, logic.Blocks);
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

        private void Load(BlockItem sheet, bool select)
        {
            DeselectAll();
            Load(sheet.Lines, select);
            Load(sheet.Texts, select);
            Load(sheet.Blocks, select);
        }

        private void Load(IEnumerable<LineItem> lineItems, bool select)
        {
            if (select)
            {
                selected.Lines = new List<Line>();
            }

            double thickness = lineThickness / Zoom;

            foreach (var lineItem in lineItems)
            {
                var line = CreateLine(thickness, lineItem.X1, lineItem.Y1, lineItem.X2, lineItem.Y2);
                logic.Lines.Add(line);
                Add(line);

                if (select)
                {
                    line.Stroke = Brushes.Red;
                    selected.Lines.Add(line);
                }
            }
        }

        private void Load(IEnumerable<TextItem> textItems, bool select)
        {
            if (select)
            {
                selected.Texts = new List<Grid>();
            }

            double thickness = lineThickness / Zoom;

            foreach (var textItem in textItems)
            {
                var text = CreateText(textItem.Text,
                                      textItem.X, textItem.Y,
                                      textItem.Width, textItem.Height,
                                      (HorizontalAlignment)textItem.HAlign,
                                      (VerticalAlignment)textItem.VAlign,
                                      textItem.Size);
                logic.Texts.Add(text);
                Add(text);

                if (select)
                {
                    GetTextBlock(text).Foreground = Brushes.Red;
                    selected.Texts.Add(text);
                }
            }
        }

        private void Load(IEnumerable<BlockItem> blockItems, bool select)
        {
            if (select)
            {
                selected.Blocks = new List<Block>();
            }

            double thickness = lineThickness / Zoom;

            foreach (var blockItem in blockItems)
            {
                var block = new Block() { Name = blockItem.Name };
                block.Init();

                foreach (var textItem in blockItem.Texts)
                {
                    var text = CreateText(textItem.Text,
                                          textItem.X, textItem.Y,
                                          textItem.Width, textItem.Height,
                                          (HorizontalAlignment)textItem.HAlign,
                                          (VerticalAlignment)textItem.VAlign,
                                          textItem.Size);
                    block.Texts.Add(text);
                    Add(text);

                    if (select)
                    {
                        GetTextBlock(text).Foreground = Brushes.Red;
                    }
                }

                foreach (var lineItem in blockItem.Lines)
                {
                    var line = CreateLine(thickness, lineItem.X1, lineItem.Y1, lineItem.X2, lineItem.Y2);
                    block.Lines.Add(line);
                    Add(line);

                    if (select)
                    {
                        line.Stroke = Brushes.Red;
                    }
                }

                logic.Blocks.Add(block);

                if (select)
                {
                    selected.Blocks.Add(block);
                }
            }
        }

        #endregion

        #region Reset

        private void RemoveLines(IEnumerable<Line> lines)
        {
            foreach (var line in lines)
            {
                Remove(line);
            }
        }

        private void RemoveTexts(IEnumerable<Grid> texts)
        {
            foreach (var text in texts)
            {
                Remove(text);
            }
        }

        private void RemoveBlocks(IEnumerable<Block> blocks)
        {
            foreach (var block in blocks)
            {
                RemoveLines(block.Lines);
                RemoveTexts(block.Texts);
            }
        }

        private void Remove(Block source, Block selected)
        {
            if (selected.Lines != null)
            {
                RemoveLines(selected.Lines);

                foreach (var line in selected.Lines)
                {
                    source.Lines.Remove(line);
                }

                selected.Lines = null;
            }

            if (selected.Texts != null)
            {
                RemoveTexts(selected.Texts);

                foreach (var text in selected.Texts)
                {
                    source.Texts.Remove(text);
                }

                selected.Texts = null;
            }

            if (selected.Blocks != null)
            {
                foreach (var block in selected.Blocks)
                {
                    RemoveLines(block.Lines);
                    RemoveTexts(block.Texts);

                    source.Blocks.Remove(block);
                }

                selected.Blocks = null;
            }
        }

        private void Delete()
        {
            if (HaveSelected())
            {
                Push();
                Remove(logic, selected);
            }
        }

        private void Reset()
        {
            RemoveLines(logic.Lines);
            RemoveTexts(logic.Texts);
            RemoveBlocks(logic.Blocks);

            logic.Lines.Clear();
            logic.Texts.Clear();
            logic.Blocks.Clear();

            selected.Lines = null;
            selected.Texts = null;
            selected.Blocks = null;
        }

        #endregion

        #region Move

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
            }
        }

        #endregion

        #region Pan & Zoom

        private void AdjustThickness(double zoom)
        {
            double gridThicknessZoomed = gridThickness / zoom;
            double lineThicknessZoomed = lineThickness / zoom;
            double selectionThicknessZoomed = selectionThickness / zoom;

            foreach (var line in gridLines)
            {
                line.StrokeThickness = gridThicknessZoomed;
            }

            foreach (var line in logic.Lines)
            {
                line.StrokeThickness = lineThicknessZoomed;
            }

            foreach (var block in logic.Blocks)
            {
                foreach (var line in block.Lines)
                {
                    line.StrokeThickness = lineThicknessZoomed;
                }
            }

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
            Overlay.CaptureMouse();
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
            Overlay.ReleaseMouseCapture();
        }

        private void ResetPanAndZoom()
        {
            zoomIndex = defaultZoomIndex;
            Zoom = zoomFactors[zoomIndex];
            PanX = 0.0;
            PanY = 0.0;
        }

        #endregion

        #region Selection Rect

        private void InitSelectionRect(Point p)
        {
            selectionStartPoint = p;
            double x = p.X;
            double y = p.Y;
            selectionRect = CreateSelectionRect(selectionThickness / Zoom, x, y, 0.0, 0.0);
            Overlay.Children.Add(selectionRect);
            Overlay.CaptureMouse();
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
            HitTest(new Rect(x, y, width, height));
        }

        private void CancelSelectionRect()
        {
            Overlay.ReleaseMouseCapture();
            Overlay.Children.Remove(selectionRect);
            selectionRect = null;
        }

        #endregion

        #region HitTest

        private void HitTest(Rect rect)
        {
            selected.Init();

            HitTestLines(logic, selected, rect, false);
            HitTestTexts(logic, selected, rect, false, Sheet);
            HitTestBlocks(logic, selected, rect, false, Sheet);

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

        private void HitTest(Point p)
        {
            selected.Init();

            var rect = new Rect(p.X - hitSize, p.Y - hitSize, 2 * hitSize, 2 * hitSize);

            HitTestLines(logic, selected, rect, true);

            if (selected.Lines.Count <= 0)
            {
                HitTestTexts(logic, selected, rect, true, Sheet);
            }

            if (selected.Lines.Count <= 0 && selected.Texts.Count <= 0)
            {
                HitTestBlocks(logic, selected, rect, true, Sheet);
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

        private static void HitTestLines(Block logic, Block selected, Rect rect, bool onlyFirst)
        {
            foreach (var line in logic.Lines)
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

        private static void HitTestTexts(Block logic, Block selected, Rect rect, bool onlyFirst, UIElement relative)
        {
            foreach (var text in logic.Texts)
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

        private static void HitTestBlocks(Block logic, Block selected, Rect rect, bool onlyFirst, UIElement relative)
        {
            foreach (var block in logic.Blocks)
            {
                bool isSelected = false;

                foreach (var line in block.Lines)
                {
                    var bounds = VisualTreeHelper.GetContentBounds(line);
                    if (rect.IntersectsWith(bounds))
                    {
                        selected.Blocks.Add(block);
                        isSelected = true;
                        break;
                    }
                }

                if (!isSelected)
                {
                    foreach (var text in block.Texts)
                    {
                        var bounds = VisualTreeHelper.GetContentBounds(text);
                        var offset = text.TranslatePoint(new Point(0, 0), relative);
                        bounds.Offset(offset.X, offset.Y);
                        if (rect.IntersectsWith(bounds))
                        {
                            selected.Blocks.Add(block);
                            isSelected = true;
                            break;
                        }
                    }
                }

                if (isSelected)
                {
                    foreach (var line in block.Lines)
                    {
                        line.Stroke = Brushes.Red;
                    }

                    foreach (var text in block.Texts)
                    {
                        GetTextBlock(text).Foreground = Brushes.Red;
                    }

                    if (onlyFirst)
                    {
                        break;
                    }
                }
            }
        }

        #endregion

        #region Selection

        private void SelectAll()
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

            foreach (var block in logic.Blocks)
            {
                foreach (var line in block.Lines)
                {
                    line.Stroke = Brushes.Red;
                }

                foreach (var text in block.Texts)
                {
                    GetTextBlock(text).Foreground = Brushes.Red;
                }

                selected.Blocks.Add(block);
            }
        }

        private void DeselectAll()
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
                foreach (var block in selected.Blocks)
                {
                    foreach (var line in block.Lines)
                    {
                        line.Stroke = Brushes.Black;
                    }

                    foreach (var text in block.Texts)
                    {
                        GetTextBlock(text).Foreground = Brushes.Black;
                    }
                }

                selected.Blocks = null;
            }
        }

        #endregion

        #region Temp Line

        private void InitTempLine(Point p)
        {
            Push();
            double x = Snap(p.X);
            double y = Snap(p.Y);
            tempLine = CreateLine(lineThickness / Zoom, x, y, x, y);
            Overlay.Children.Add(tempLine);
            Overlay.CaptureMouse();
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
                Overlay.ReleaseMouseCapture();
                Overlay.Children.Remove(tempLine);
                logic.Lines.Add(tempLine);
                Add(tempLine);
                tempLine = null;
            }
        }

        private void CancelTempLine()
        {
            Pop();
            Overlay.ReleaseMouseCapture();
            Overlay.Children.Remove(tempLine);
            tempLine = null;
        }

        #endregion

        #region Undo/Redo

        private Stack<string> Undos = new Stack<string>();
        private Stack<string> Redos = new Stack<string>();

        private void Push()
        {
            var text = ItemSerializer.Serialize(CreateSheet());
            Undos.Push(text);
        }

        private void Pop()
        {
            Undos.Pop();
        }

        private void Undo()
        {
            if (Undos.Count > 0)
            {
                var redo = ItemSerializer.Serialize(CreateSheet());
                Redos.Push(redo);

                var text = Undos.Pop();
                Reset();
                Load(ItemSerializer.Deserialize(text), false);
            }
        }

        private void Redo()
        {
            if (Redos.Count > 0)
            {
                var undo = ItemSerializer.Serialize(CreateSheet());
                Undos.Push(undo);

                var text = Redos.Pop();
                Reset();
                Load(ItemSerializer.Deserialize(text), false);
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
                CreateSheet(selected.Lines, selected.Texts, selected.Blocks) : CreateSheet());
            Clipboard.SetData(DataFormats.UnicodeText, text);
        }

        private void Paste()
        {
            var text = (string)Clipboard.GetData(DataFormats.UnicodeText);
            Load(ItemSerializer.Deserialize(text), true);
        }

        #endregion

        #region Open & Save

        private void Open(string fileName)
        {
            try
            {
                using (var stream = System.IO.File.OpenText(fileName))
                {
                    var text = stream.ReadToEnd();
                    if (text != null)
                    {
                        Push();
                        Reset();
                        Load(ItemSerializer.Deserialize(text), false);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }

        private void Save(string fileName)
        {
            try
            {
                var text = ItemSerializer.Serialize(CreateSheet());
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

        #region File Dialogs

        private void Open()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "TXT Files (*.txt)|*.txt|All Files (*.*)|*.*"
            };

            if (dlg.ShowDialog() == true)
            {
                Open(dlg.FileName);
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
                Save(dlg.FileName);
            }
        }

        #endregion

        #region Events

        private void Overlay_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DeselectAll();

            if (mode == Mode.AndGate)
            {
                AddAndGate(e.GetPosition(Overlay));
            }
            else if (mode == Mode.OrGate)
            {
                AddOrGate(e.GetPosition(Overlay));
            }
            else if (mode == Mode.Selection)
            {
                HitTest(e.GetPosition(Overlay));
                if (!HaveSelected())
                {
                    InitSelectionRect(e.GetPosition(Overlay));
                }
            }
            else if (mode == Mode.Line && !Overlay.IsMouseCaptured)
            {
                InitTempLine(e.GetPosition(Overlay));
            }
            else if (mode == Mode.Line && Overlay.IsMouseCaptured)
            {
                FinishTempLine();
            }
            else if (mode == Mode.Pan && Overlay.IsMouseCaptured)
            {
                FinishPan();
            }
        }

        private void Overlay_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (mode == Mode.Selection && Overlay.IsMouseCaptured)
            {
                FinishSelectionRect();
            }
        }

        private void Overlay_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (mode == Mode.Selection && Overlay.IsMouseCaptured)
            {
                MoveSelectionRect(e.GetPosition(Overlay));
            }
            else if (mode == Mode.Line && Overlay.IsMouseCaptured)
            {
                MoveTempLine(e.GetPosition(Overlay));
            }
            else if (mode == Mode.Pan && Overlay.IsMouseCaptured)
            {
                Pan(e.GetPosition(this));
            }
        }

        private void Overlay_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            DeselectAll();

            if (mode == Mode.Selection && Overlay.IsMouseCaptured)
            {
                CancelSelectionRect();
            }
            else if (mode == Mode.Line && Overlay.IsMouseCaptured)
            {
                CancelTempLine();
            }
            else if (!Overlay.IsMouseCaptured)
            {
                InitPan(e.GetPosition(this));
            }
        }

        private void Overlay_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (mode == Mode.Pan && Overlay.IsMouseCaptured)
            {
                FinishPan();
            }
        }

        private void Workspace_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ZoomTo(e.Delta, e.GetPosition(Overlay));
        }

        private void Workspace_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle && e.ClickCount == 2)
            {
                ResetPanAndZoom();
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
                        SelectAll();
                    }
                    else
                    {
                        mode = Mode.AndGate;
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

        #endregion
    }
}
