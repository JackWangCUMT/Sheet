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
        private int zi = 9; // zs index from 0 to 21, 9 = 100%
        private int defaultzi = 9;
        private double[] zs = 
        { 
            0.01, 0.0625, 0.0833, 0.125, 0.25, 0.3333, 0.5, 0.6667, 0.75,
            1, 
            1.25, 1.5, 2, 3, 4, 6, 8, 12, 16, 24, 32, 64
        };
        private double snap = 15;
        private double grid = 30;
        private double gthickness = 1.0;
        private double lthickness = 3.0;
        private bool one = true;
        private Line temp = null;
        private Point start;
        private List<Line> lines = new List<Line>();
        private List<Line> grids = new List<Line>();

        private double Zoom
        {
            get { return zoom.ScaleX; }
            set
            {
                AdjustThickness(value);
                zoom.ScaleX = value;
                zoom.ScaleY = value;
            }
        }

        private double PanX
        {
            get { return pan.X; }
            set
            {
                pan.X = value;
            }
        }

        private double PanY
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
            Loaded += (s, e) => Grid();
        }

        private void Grid()
        {
            double w = Sheet.ActualWidth;
            double h = Sheet.ActualHeight;

            for (double y = grid; y < h; y += grid)
            {
                var l = new Line() { Stroke = Brushes.LightGray, StrokeThickness = gthickness, X1 = 0, Y1 = y, X2 = w, Y2 = y };
                grids.Add(l);
                Sheet.Children.Add(l);
            }

            for (double x = grid; x < w; x += grid)
            {
                var l = new Line() { Stroke = Brushes.LightGray, StrokeThickness = gthickness, X1 = x, Y1 = 0, X2 = x, Y2 = h };
                grids.Add(l);
                Back.Children.Add(l);
            }
        }

        private double Snap(double val)
        {
            double m = val % snap;
            return m >= snap / 2.0 ? val + snap - m : val - m;
        }

        private void AdjustThickness(double z)
        {
            foreach (var l in grids)
            {
                l.StrokeThickness = gthickness / z;
            }

            foreach (var l in lines)
            {
                l.StrokeThickness = lthickness / z;
            }
        }

        private void ZoomTo(Point p, int oldzi)
        {
            double oldz = zs[oldzi];
            double z = zs[zi];
            Zoom = z;
            PanX = (p.X * oldz + PanX) - p.X * z;
            PanY = (p.Y * oldz + PanY) - p.Y * z;
        }

        private void InitLine(Point p)
        {
            double thickness = lthickness / Zoom;
            double x = Snap(p.X);
            double y = Snap(p.Y);
            var l = new Line() 
            {
                Stroke = Brushes.Red, StrokeThickness = thickness, 
                StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round,
                X1 = x, Y1 = y, X2 = x, Y2 = y
            };
            temp = l;
            lines.Add(l);
            Sheet.Children.Add(l);
            Sheet.CaptureMouse();
        }

        private void FinishLine()
        {
            var l = temp;
            if (Math.Round(l.X1, 1) == Math.Round(l.X2, 1) && Math.Round(l.Y1, 1) == Math.Round(l.Y2, 1))
            {
                CancelLine();
            }
            else
            {
                Sheet.ReleaseMouseCapture();
                temp = null;
            }
        }

        private void CancelLine()
        {
            var l = temp;
            Sheet.ReleaseMouseCapture();
            lines.Remove(l);
            Sheet.Children.Remove(l);
            temp = null;
        }

        private void Sheet_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!Sheet.IsMouseCaptured && temp == null)
            {
                var p = e.GetPosition(Sheet);
                InitLine(p);
            }
            else if (Sheet.IsMouseCaptured && one)
            {
                FinishLine();
            }
        }

        private void Sheet_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Sheet.IsMouseCaptured && !one)
            {
                FinishLine();
            }
        }

        private void Sheet_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (Sheet.IsMouseCaptured && temp != null)
            {
                var p = e.GetPosition(Sheet);
                var l = temp;
                double x = Snap(p.X);
                double y = Snap(p.Y);
                if (Math.Round(x, 1) != Math.Round(l.X2, 1) || Math.Round(y, 1) != Math.Round(l.Y2, 1))
                {
                    l.X2 = x;
                    l.Y2 = y;
                }
            }
            else if (Sheet.IsMouseCaptured && temp == null)
            {
                var p = e.GetPosition(this);
                PanX = PanX + p.X - start.X;
                PanY = PanY + p.Y - start.Y;
                start = p;
            }
        }

        private void Sheet_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Sheet.IsMouseCaptured && temp != null)
            {
                CancelLine();
            }
            else if (!Sheet.IsMouseCaptured && temp == null)
            {
                start = e.GetPosition(this);
                temp = null;
                Sheet.CaptureMouse();
            }
        }

        private void Sheet_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Sheet.IsMouseCaptured && temp == null)
            {
                Sheet.ReleaseMouseCapture();
            }
        }

        private void Workspace_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var p = e.GetPosition(Sheet);

            if (e.Delta > 0)
            {
                if (zi < 21)
                {
                    int oldzi = zi;
                    zi++;
                    ZoomTo(p, oldzi);
                }
            }
            else
            {
                if (zi > 0)
                {
                    int oldzi = zi;
                    zi--;
                    ZoomTo(p, oldzi);
                }
            }
        }

        private void Workspace_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle && e.ClickCount == 2)
            {
                zi = defaultzi;
                Zoom = zs[zi];
                PanX = 0.0;
                PanY = 0.0;
            }
        }
    }
}
