using System;
using System.Collections.Generic;
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

    public static class ItemSerializer
    {
        private static char[] lineSeparators = { '\n' };
        private static char[] modelSeparators = { ';' };

        private static void SerializeLine(StringBuilder sb, LineItem line)
        {
            sb.Append('L');
            sb.Append(';');
            sb.Append(line.Id);
            sb.Append(';');
            sb.Append(line.X1);
            sb.Append(';');
            sb.Append(line.Y1);
            sb.Append(';');
            sb.Append(line.X2);
            sb.Append(';');
            sb.Append(line.Y2);
            sb.Append('\n');
        }

        public static string Serialize(List<LineItem> lines)
        {
            var sb = new StringBuilder();

            foreach(var line in lines)
            {
                SerializeLine(sb, line);
            }

            return sb.ToString();
        }

        public static List<LineItem> Deserialize(string s)
        {
            var lines = s.Split(lineSeparators);
            var lineItems = new List<LineItem>();

            foreach(var line in lines)
            {
                var m = line.Split(modelSeparators);
                if (m.Length == 6 && string.Compare(m[0], "L", true) == 0)
                {
                    var lineItem = new LineItem();
                    lineItem.Id = int.Parse(m[1]);
                    lineItem.X1 = double.Parse(m[2]);
                    lineItem.Y1 = double.Parse(m[3]);
                    lineItem.X2 = double.Parse(m[4]);
                    lineItem.Y2 = double.Parse(m[5]);
                    lineItems.Add(lineItem);
                }
            }

            return lineItems;
        }
    }

    public class Block
    {
        public List<Line> Lines = new List<Line>();
        public List<Grid> Texts = new List<Grid>();
    }

    public partial class SheetControl : UserControl
    {
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
        private List<Block> blocks = new List<Block>();
        private List<Line> gridLines = new List<Line>();
        private string serializedLines = null;
        private List<string> models = new List<string>();

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

        private double Snap(double val)
        {
            double r = val % snapSize;
            return r >= snapSize / 2.0 ? val + snapSize - r : val - r;
        }

        public SheetControl()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                CreateGrid();

                blocks.Add(CreateOrGateBlock(30.0, 30.0, 1));
                blocks.Add(CreateOrGateBlock(90.0, 30.0, 1));

                Focus();
            };
        }

        public Block CreateOrGateBlock(double x, double y, double count)
        {
            var block = new Block();

            var text = CreateText("≥" + count.ToString(), x, y, 30.0, 30.0);
            block.Texts.Add(text);
            Sheet.Children.Add(text);

            double thickness = lineThickness / Zoom;

            var l0 = CreateLogicLine(thickness, x, y, x + 30.0, y);
            block.Lines.Add(l0);
            Sheet.Children.Add(l0);

            var l1 = CreateLogicLine(thickness, x, y + 30.0, x + 30.0, y + 30.0);
            block.Lines.Add(l1);
            Sheet.Children.Add(l1);

            var l2 = CreateLogicLine(thickness, x, y, x, y + 30.0);
            block.Lines.Add(l2);
            Sheet.Children.Add(l2);

            var l3 = CreateLogicLine(thickness, x + 30.0, y, x + 30.0 , y + 30.0);
            block.Lines.Add(l3);
            Sheet.Children.Add(l3);

            return block;
        }

        private Grid CreateText(string text, double x, double y, double width, double height)
        {
            var grid = new Grid();
            grid.Background = Brushes.White;
            grid.Width = width;
            grid.Height = height;
            Canvas.SetLeft(grid, x);
            Canvas.SetTop(grid, y);

            var tb = new TextBlock();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            tb.Foreground = Brushes.Red;
            tb.Text = text;

            grid.Children.Add(tb);

            return grid;
        }

        private Line CreateLogicLine(double thickness, double x1, double y1, double x2, double y2)
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

        private void Serialize()
        {
            var lineItems = new List<LineItem>();

            foreach (var line in logicLines)
            {
                lineItems.Add(SerializeLine(line));
            }

            serializedLines = ItemSerializer.Serialize(lineItems);
        }

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

        private void Deserialize()
        {
            if (serializedLines != null)
            {
                var lineItems = ItemSerializer.Deserialize(serializedLines);

                double thickness = lineThickness / Zoom;
                foreach (var lineItem in lineItems)
                {
                    var line = CreateLogicLine(thickness, lineItem.X1, lineItem.Y1, lineItem.X2, lineItem.Y2);
                    logicLines.Add(line);
                    Sheet.Children.Add(line);
                }
            }
        }

        private void Reset()
        {
            foreach (var line in logicLines)
            {
                Sheet.Children.Remove(line);
            }

            logicLines.Clear();
        }

        private void Move(double x, double y)
        {
            foreach (var line in logicLines)
            {
                line.X1 += x;
                line.Y1 += y;
                line.X2 += x;
                line.Y2 += y;
            }

            foreach (var block in blocks)
            {
                foreach(var line in block.Lines)
                {
                    line.X1 += x;
                    line.Y1 += y;
                    line.X2 += x;
                    line.Y2 += y;
                }

                foreach(var text in block.Texts)
                {
                    Canvas.SetLeft(text, Canvas.GetLeft(text) + x);
                    Canvas.SetTop(text, Canvas.GetTop(text) + y);
                }
            }
        }

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
                foreach(var line in block.Lines)
                {
                    line.StrokeThickness = lineThickness / zoom;
                }
            }
        }

        private void ZoomTo(Point p, int oldZoomIndex)
        {
            double oldZoom = zoomFactors[oldZoomIndex];
            double newZoom = zoomFactors[zoomIndex];
            Zoom = newZoom;
            PanX = (p.X * oldZoom + PanX) - p.X * newZoom;
            PanY = (p.Y * oldZoom + PanY) - p.Y * newZoom;
        }

        private void InitLine(Point p)
        {
            double x = Snap(p.X);
            double y = Snap(p.Y);
            tempLine = CreateLogicLine(lineThickness / Zoom, x, y, x, y);
            logicLines.Add(tempLine);
            Sheet.Children.Add(tempLine);
            Sheet.CaptureMouse();
        }

        private void FinishLine()
        {
            if (Math.Round(tempLine.X1, 1) == Math.Round(tempLine.X2, 1) && 
                Math.Round(tempLine.Y1, 1) == Math.Round(tempLine.Y2, 1))
            {
                CancelLine();
            }
            else
            {
                Sheet.ReleaseMouseCapture();
                tempLine = null;
            }
        }

        private void CancelLine()
        {
            Sheet.ReleaseMouseCapture();
            logicLines.Remove(tempLine);
            Sheet.Children.Remove(tempLine);
            tempLine = null;
        }

        private void Sheet_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!Sheet.IsMouseCaptured && tempLine == null)
            {
                InitLine(e.GetPosition(Sheet));
            }
            else if (Sheet.IsMouseCaptured && oneClickMode)
            {
                FinishLine();
            }
        }

        private void Sheet_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Sheet.IsMouseCaptured && !oneClickMode)
            {
                FinishLine();
            }
        }

        private void Sheet_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (Sheet.IsMouseCaptured && tempLine != null)
            {
                var p = e.GetPosition(Sheet);
                double x = Snap(p.X);
                double y = Snap(p.Y);
                if (Math.Round(x, 1) != Math.Round(tempLine.X2, 1) || Math.Round(y, 1) != Math.Round(tempLine.Y2, 1))
                {
                    tempLine.X2 = x;
                    tempLine.Y2 = y;
                }
            }
            else if (Sheet.IsMouseCaptured && tempLine == null)
            {
                var p = e.GetPosition(this);
                PanX = PanX + p.X - panStartPoint.X;
                PanY = PanY + p.Y - panStartPoint.Y;
                panStartPoint = p;
            }
        }

        private void Sheet_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Sheet.IsMouseCaptured && tempLine != null)
            {
                CancelLine();
            }
            else if (!Sheet.IsMouseCaptured && tempLine == null)
            {
                panStartPoint = e.GetPosition(this);
                tempLine = null;
                Sheet.CaptureMouse();
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
            if (e.Delta > 0)
            {
                if (zoomIndex < maxZoomIndex)
                {
                    ZoomTo(e.GetPosition(Sheet), zoomIndex++);
                }
            }
            else
            {
                if (zoomIndex > 0)
                {
                    ZoomTo(e.GetPosition(Sheet), zoomIndex--);
                }
            }
        }

        private void Workspace_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle && e.ClickCount == 2)
            {
                zoomIndex = defaultZoomIndex;
                Zoom = zoomFactors[zoomIndex];
                PanX = 0.0;
                PanY = 0.0;
            }
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.R: 
                    Reset();
                    break;
                case Key.S:
                    Serialize();
                    break;
                case Key.L:
                    Reset();
                    Deserialize();
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
    }
}
