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
    } 

    public class SheetItem : Item
    {
        public List<LineItem> Lines { get; set; }
        public List<TextItem> Texts { get; set; }
        public List<BlockItem> Blocks { get; set; }
    } 

    #endregion

    #region ItemSerializer

    public static class ItemSerializer
    {
        private static string lineSeparator = "\r\n";
        private static string modelSeparator = ";";
        private static char[] lineSeparators = { '\r', '\n' };
        private static char[] modelSeparators = { ';' };

        public static void Serialize(StringBuilder sb, LineItem line)
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

        public static void Serialize(StringBuilder sb, TextItem text)
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

        public static void Serialize(StringBuilder sb, BlockItem block)
        {
            sb.Append("BLOCK");
            sb.Append(modelSeparator);
            sb.Append(block.Id);
            sb.Append(modelSeparator);
            sb.Append(block.Name);
            sb.Append(lineSeparator);

            Serialize(sb, block.Lines);
            Serialize(sb, block.Texts);

            sb.Append("END");
            sb.Append(lineSeparator);
        }

        public static void Serialize(StringBuilder sb, List<LineItem> lines)
        {
            foreach (var line in lines)
            {
                Serialize(sb, line);
            }
        }

        public static void Serialize(StringBuilder sb, List<TextItem> texts)
        {
            foreach (var text in texts)
            {
                Serialize(sb, text);
            }
        }

        public static void Serialize(StringBuilder sb, List<BlockItem> blocks)
        {
            foreach (var block in blocks)
            {
                Serialize(sb, block);
            }
        }

        public static string Serialize(SheetItem sheet)
        {
            var sb = new StringBuilder();

            Serialize(sb, sheet.Lines);
            Serialize(sb, sheet.Texts);
            Serialize(sb, sheet.Blocks);

            return sb.ToString();
        }

        public static SheetItem Deserialize(string s)
        {
            var lines = s.Split(lineSeparators, StringSplitOptions.RemoveEmptyEntries);

            var sheet = new SheetItem()
            {
                Lines = new List<LineItem>(),
                Texts = new List<TextItem>(),
                Blocks = new List<BlockItem>()
            };

            BlockItem block = null;

            foreach (var line in lines)
            {
                var m = line.Split(modelSeparators);
                if (m.Length == 6 && string.Compare(m[0], "LINE", true) == 0)
                {
                    var lineItem = new LineItem();
                    lineItem.Id = int.Parse(m[1]);
                    lineItem.X1 = double.Parse(m[2]);
                    lineItem.Y1 = double.Parse(m[3]);
                    lineItem.X2 = double.Parse(m[4]);
                    lineItem.Y2 = double.Parse(m[5]);

                    if (block == null)
                    {
                        sheet.Lines.Add(lineItem);
                    }
                    else
                    {
                        block.Lines.Add(lineItem);
                    }
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

                    if (block == null)
                    {
                        sheet.Texts.Add(textItem);
                    }
                    else
                    {
                        block.Texts.Add(textItem);
                    }
                }
                else if (m.Length == 3 && string.Compare(m[0], "BLOCK", true) == 0 && block == null)
                {
                    block = new BlockItem() { Lines = new List<LineItem>(), Texts = new List<TextItem>() };
                    block.Id = int.Parse(m[1]);
                    block.Name = m[2];
                }
                else if (m.Length == 1 && string.Compare(m[0], "END", true) == 0 && block != null)
                {
                    sheet.Blocks.Add(block);
                    block = null;
                }
            }

            return sheet;
        }
    }

    #endregion

    #region Block

    public class Block
    {
        public string Name = "";
        public List<Line> Lines = new List<Line>();
        public List<Grid> Texts = new List<Grid>();
    }

    #endregion

    public partial class SheetControl : UserControl
    {
        #region Fields

        private int zoomIndex = 9; // zoomFactors index from 0 to 21, 9 = 100%
        private int defaultZoomIndex = 9;
        private int maxZoomIndex = 21;
        private double[] zoomFactors = { 0.01, 0.0625, 0.0833, 0.125, 0.25, 0.3333, 0.5, 0.6667, 0.75, 1, 1.25, 1.5, 2, 3, 4, 6, 8, 12, 16, 24, 32, 64 };
        private double snapSize = 15;
        private double gridSize = 30;
        private double gridThickness = 1.0;
        private double lineThickness = 2.0;
        private bool oneClickMode = true;
        private Line tempLine = null;
        private Point panStartPoint;
        private List<Line> logicLines = new List<Line>();
        private List<Grid> texts = new List<Grid>();
        private List<Block> blocks = new List<Block>();
        private List<Line> gridLines = new List<Line>();

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

            Loaded += (s, e) =>
            {
                CreateGrid();

                Push();
                blocks.Add(CreateOrGateBlock(300.0, 90.0, 1));
                blocks.Add(CreateOrGateBlock(300.0, 180.0, 1));
                blocks.Add(CreateAndGateBlock(360.0, 180.0));

                Focus();
            };
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
                Sheet.Children.Add(l);
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
            Sheet.Children.Add(line);
        }

        private void AddTextToBlock(Block block, string str, double x, double y, double width, double height, HorizontalAlignment halign, VerticalAlignment valign, double size)
        {
            var text = CreateText(str, x, y, width, height, halign, valign, size);
            block.Texts.Add(text);
            Sheet.Children.Add(text);
        }

        private void AddOrGate(Point p)
        {
            Push();
            double x = Snap(p.X);
            double y = Snap(p.Y);
            blocks.Add(CreateOrGateBlock(x, y, 1));
        }

        private void AddAndGate(Point p)
        {
            Push();
            double x = Snap(p.X);
            double y = Snap(p.Y);
            blocks.Add(CreateAndGateBlock(x, y));
        }

        #endregion

        #region Serialize

        private LineItem SerializeLine(Line line)
        {
            var lineItem = new LineItem();

            lineItem.Id = 0;
            lineItem.X1 = line.X1;
            lineItem.Y1 = line.Y1;
            lineItem.X2 = line.X2;
            lineItem.Y2 = line.Y2;

            return lineItem;
        }

        private TextItem SerializeText(Grid text)
        {
            var textItem = new TextItem();

            textItem.Id = 0;
            textItem.X = Canvas.GetLeft(text);
            textItem.Y = Canvas.GetTop(text);
            textItem.Width = text.Width;
            textItem.Height = text.Height;

            var tb = text.Children[0] as TextBlock;
            textItem.Text = tb.Text;
            textItem.HAlign = (int) tb.HorizontalAlignment;
            textItem.VAlign = (int) tb.VerticalAlignment;
            textItem.Size = tb.FontSize;

            return textItem;
        }

        private BlockItem SerializeBlock(Block block)
        {
            var blockItem = new BlockItem() 
            {
                Name = block.Name,
                Lines = new List<LineItem>(),
                Texts = new List<TextItem>(),
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

        private SheetItem CreateSheet()
        {
            var sheet = new SheetItem()
            {
                Lines = new List<LineItem>(),
                Texts = new List<TextItem>(),
                Blocks = new List<BlockItem>()
            };

            foreach (var line in logicLines)
            {
                sheet.Lines.Add(SerializeLine(line));
            }

            foreach (var text in texts)
            {
                sheet.Texts.Add(SerializeText(text));
            }

            foreach (var block in blocks)
            {
                sheet.Blocks.Add(SerializeBlock(block));
            }

            return sheet;
        }

        #endregion

        #region Deserialize

        private Grid CreateText(string text, 
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
            tb.Foreground = Brushes.Red;
            tb.FontSize = size;
            tb.FontFamily = new FontFamily("Calibri");
            tb.Text = text;

            grid.Children.Add(tb);

            return grid;
        }

        private Line CreateLine(double thickness, double x1, double y1, double x2, double y2)
        {
            var line = new Line()
            {
                Stroke = Brushes.Red,
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

        #endregion

        #region Load

        private void Load(SheetItem sheet)
        {
            Load(sheet.Lines);
            Load(sheet.Texts);
            Load(sheet.Blocks);
        }

        private void Load(IEnumerable<LineItem> lineItems)
        {
            double thickness = lineThickness / Zoom;

            foreach (var lineItem in lineItems)
            {
                var line = CreateLine(thickness, lineItem.X1, lineItem.Y1, lineItem.X2, lineItem.Y2);
                logicLines.Add(line);
                Sheet.Children.Add(line);
            }
        }

        private void Load(IEnumerable<TextItem> textItems)
        {
            double thickness = lineThickness / Zoom;

            foreach (var textItem in textItems)
            {
                var text = CreateText(textItem.Text,
                                      textItem.X, textItem.Y,
                                      textItem.Width, textItem.Height,
                                      (HorizontalAlignment)textItem.HAlign,
                                      (VerticalAlignment)textItem.VAlign,
                                      textItem.Size);
                texts.Add(text);
                Sheet.Children.Add(text);
            }
        }

        private void Load(IEnumerable<BlockItem> blockItems)
        {
            double thickness = lineThickness / Zoom;

            foreach (var blockItem in blockItems)
            {
                var block = new Block() { Name = blockItem.Name };

                foreach (var textItem in blockItem.Texts)
                {
                    var text = CreateText(textItem.Text,
                                          textItem.X, textItem.Y,
                                          textItem.Width, textItem.Height,
                                          (HorizontalAlignment)textItem.HAlign,
                                          (VerticalAlignment)textItem.VAlign,
                                          textItem.Size);
                    block.Texts.Add(text);
                    Sheet.Children.Add(text);
                }

                foreach (var lineItem in blockItem.Lines)
                {
                    var line = CreateLine(thickness, lineItem.X1, lineItem.Y1, lineItem.X2, lineItem.Y2);
                    block.Lines.Add(line);
                    Sheet.Children.Add(line);
                }

                blocks.Add(block);
            }
        }

        #endregion

        #region Reset

        private void Reset()
        {
            ResetLines();
            ResetTexts();
            ResetBlocks();
        }

        private void ResetLines()
        {
            foreach (var line in logicLines)
            {
                Sheet.Children.Remove(line);
            }

            logicLines.Clear();
        }

        private void ResetTexts()
        {
            foreach (var text in texts)
            {
                Sheet.Children.Remove(text);
            }

            texts.Clear();
        }

        private void ResetBlocks()
        {
            foreach (var block in blocks)
            {
                foreach (var line in block.Lines)
                {
                    Sheet.Children.Remove(line);
                }

                foreach (var text in block.Texts)
                {
                    Sheet.Children.Remove(text);
                }
            }

            blocks.Clear();
        }

        #endregion

        #region Move

        private void Move(double x, double y)
        {
            Push();
            MoveLines(x, y, logicLines);

            foreach (var block in blocks)
            {
                MoveLines(x, y, block.Lines);
                MoveTexts(x, y, block.Texts);
            }
        }

        private void MoveTexts(double x, double y, IEnumerable<Grid> texts)
        {
            foreach (var text in texts)
            {
                Canvas.SetLeft(text, Canvas.GetLeft(text) + x);
                Canvas.SetTop(text, Canvas.GetTop(text) + y);
            }
        }

        private void MoveLines(double x, double y, IEnumerable<Line> lines)
        {
            foreach (var line in lines)
            {
                line.X1 += x;
                line.Y1 += y;
                line.X2 += x;
                line.Y2 += y;
            }
        }

        #endregion

        #region Pan & Zoom

        private void AdjustThickness(double zoom)
        {
            foreach (var line in gridLines)
            {
                line.StrokeThickness = gridThickness / zoom;
            }

            foreach (var line in logicLines)
            {
                line.StrokeThickness = lineThickness / zoom;
            }

            foreach (var block in blocks)
            {
                foreach (var line in block.Lines)
                {
                    line.StrokeThickness = lineThickness / zoom;
                }
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
            panStartPoint = p;
            tempLine = null;
            Sheet.CaptureMouse();
        }

        private void Pan(Point p)
        {
            PanX = PanX + p.X - panStartPoint.X;
            PanY = PanY + p.Y - panStartPoint.Y;
            panStartPoint = p;
        }

        private void ResetPanAndZoom()
        {
            zoomIndex = defaultZoomIndex;
            Zoom = zoomFactors[zoomIndex];
            PanX = 0.0;
            PanY = 0.0;
        } 

        #endregion

        #region Temp Line

        private void InitTempLine(Point p)
        {
            Push();
            double x = Snap(p.X);
            double y = Snap(p.Y);
            tempLine = CreateLine(lineThickness / Zoom, x, y, x, y);
            logicLines.Add(tempLine);
            Sheet.Children.Add(tempLine);
            Sheet.CaptureMouse();
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
                Sheet.ReleaseMouseCapture();
                tempLine = null;
            }
        }

        private void CancelTempLine()
        {
            Pop();
            Sheet.ReleaseMouseCapture();
            logicLines.Remove(tempLine);
            Sheet.Children.Remove(tempLine);
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
                Load(ItemSerializer.Deserialize(text));
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
                Load(ItemSerializer.Deserialize(text));
            }
        }

        #endregion

        #region Clipboard

        private void Cut()
        {
            Copy();
            Push();
            Reset();
        }

        private void Copy()
        {
            var text = ItemSerializer.Serialize(CreateSheet());
            Clipboard.SetData(DataFormats.UnicodeText, text);
        }

        private void Paste()
        {
            var text = (string)Clipboard.GetData(DataFormats.UnicodeText);
            Load(ItemSerializer.Deserialize(text));
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
                        Load(ItemSerializer.Deserialize(text));
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

        private void Sheet_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.A))
            {
                AddAndGate(e.GetPosition(Sheet));
            }
            else if (Keyboard.IsKeyDown(Key.O))
            {
                AddOrGate(e.GetPosition(Sheet));
            }
            else
            {
                if (!Sheet.IsMouseCaptured && tempLine == null)
                {
                    InitTempLine(e.GetPosition(Sheet));
                }
                else if (Sheet.IsMouseCaptured && oneClickMode)
                {
                    FinishTempLine();
                }
            }
        }

        private void Sheet_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Sheet.IsMouseCaptured && !oneClickMode)
            {
                FinishTempLine();
            }
        }

        private void Sheet_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (Sheet.IsMouseCaptured && tempLine != null)
            {
                MoveTempLine(e.GetPosition(Sheet));
            }
            else if (Sheet.IsMouseCaptured && tempLine == null)
            {
                Pan(e.GetPosition(this));
            }
        }

        private void Sheet_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Sheet.IsMouseCaptured && tempLine != null)
            {
                CancelTempLine();
            }
            else if (!Sheet.IsMouseCaptured && tempLine == null)
            {
                InitPan(e.GetPosition(this));
            }
        }

        private void Sheet_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Sheet.IsMouseCaptured && tempLine == null)
            {
                Sheet.ReleaseMouseCapture();
            }
        }

        private void Workspace_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ZoomTo(e.Delta, e.GetPosition(Sheet));
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

            switch(e.Key)
            {
                case Key.Z:
                    {
                        if (ctrl)
                        {
                            Undo();
                        }
                    }
                    break;
                case Key.Y:
                    {
                        if (ctrl)
                        {
                            Redo();
                        }
                    }
                    break;
                case Key.X:
                    if (ctrl)
                    {
                        Cut();
                    }
                    break;
                case Key.C:
                    if (ctrl)
                    {
                        Copy();
                    }
                    break;
                case Key.V:
                    if (ctrl)
                    {
                        Paste();
                    }
                    break;
                case Key.R:
                    Push();
                    Reset();
                    break;
                case Key.S:
                    if (ctrl)
                    {
                        Save();
                    }
                    break;
                case Key.O:
                    if (ctrl)
                    {
                        Open();
                    }
                    break;
                case Key.Up:
                    Move(0.0, -snapSize);
                    break;
                case Key.Down:
                    Move(0.0, snapSize);
                    break;
                case Key.Left:
                    Move(-snapSize, 0.0);
                    break;
                case Key.Right:
                    Move(snapSize, 0.0);
                    break;
            }
        }

        #endregion
    }
}
