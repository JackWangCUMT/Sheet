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
    public partial class SheetControl : UserControl
    {
        private int zoomIndex = 9; // zoomFactors index from 0 to 21, 9 = 100%
        private int defaultZoomIndex = 9;
        private int maxZoomIndex = 21;
        private double[] zoomFactors = { 0.01, 0.0625, 0.0833, 0.125, 0.25, 0.3333, 0.5, 0.6667, 0.75, 1, 1.25, 1.5, 2, 3, 4, 6, 8, 12, 16, 24, 32, 64 };
        private double snapSize = 15;
        private double gridSize = 30;
        private double gridThickness = 1.0;
        private double lineThickness = 3.0;
        private bool oneClickMode = true;
        private Line tempLine = null;
        private Point panStartPoint;
        private List<Line> logicLines = new List<Line>();
        private List<Line> gridLines = new List<Line>();

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

        public SheetControl()
        {
            InitializeComponent();
            Loaded += (s, e) => CreateGrid();
        }

        private void CreateGrid()
        {
            double w = Sheet.ActualWidth;
            double h = Sheet.ActualHeight;

            for (double y = gridSize; y < h; y += gridSize)
            {
                var l = new Line() { Stroke = Brushes.LightGray, StrokeThickness = gridThickness, X1 = 0, Y1 = y, X2 = w, Y2 = y };
                gridLines.Add(l);
                Sheet.Children.Add(l);
            }

            for (double x = gridSize; x < w; x += gridSize)
            {
                var l = new Line() { Stroke = Brushes.LightGray, StrokeThickness = gridThickness, X1 = x, Y1 = 0, X2 = x, Y2 = h };
                gridLines.Add(l);
                Back.Children.Add(l);
            }

            AdjustThickness(zoomFactors[zoomIndex]);
        }

        private double Snap(double val)
        {
            double m = val % snapSize;
            return m >= snapSize / 2.0 ? val + snapSize - m : val - m;
        }

        private void AdjustThickness(double z)
        {
            foreach (var l in gridLines)
            {
                l.StrokeThickness = gridThickness / z;
            }

            foreach (var l in logicLines)
            {
                l.StrokeThickness = lineThickness / z;
            }
        }

        private void ZoomTo(Point p, int oldzi)
        {
            double oldz = zoomFactors[oldzi];
            double z = zoomFactors[zoomIndex];
            Zoom = z;
            PanX = (p.X * oldz + PanX) - p.X * z;
            PanY = (p.Y * oldz + PanY) - p.Y * z;
        }

        private void InitLine(Point p)
        {
            double thickness = lineThickness / Zoom;
            double x = Snap(p.X);
            double y = Snap(p.Y);
            var l = new Line() 
            {
                Stroke = Brushes.Red, StrokeThickness = thickness, 
                StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round,
                X1 = x, Y1 = y, X2 = x, Y2 = y
            };
            tempLine = l;
            logicLines.Add(l);
            Sheet.Children.Add(l);
            Sheet.CaptureMouse();
        }

        private void FinishLine()
        {
            var l = tempLine;
            if (Math.Round(l.X1, 1) == Math.Round(l.X2, 1) && Math.Round(l.Y1, 1) == Math.Round(l.Y2, 1))
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
            var l = tempLine;
            Sheet.ReleaseMouseCapture();
            logicLines.Remove(l);
            Sheet.Children.Remove(l);
            tempLine = null;
        }

        private void Sheet_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!Sheet.IsMouseCaptured && tempLine == null)
            {
                var p = e.GetPosition(Sheet);
                InitLine(p);
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
                var l = tempLine;
                double x = Snap(p.X);
                double y = Snap(p.Y);
                if (Math.Round(x, 1) != Math.Round(l.X2, 1) || Math.Round(y, 1) != Math.Round(l.Y2, 1))
                {
                    l.X2 = x;
                    l.Y2 = y;
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
            var p = e.GetPosition(Sheet);

            if (e.Delta > 0)
            {
                if (zoomIndex < maxZoomIndex)
                {
                    ZoomTo(p, zoomIndex++);
                }
            }
            else
            {
                if (zoomIndex > 0)
                {
                    ZoomTo(p, zoomIndex--);
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
    }
}
