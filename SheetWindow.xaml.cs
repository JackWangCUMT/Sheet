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
    public partial class SheetWindow : Window
    {
        bool one = true;
        Line temp = null;
        double snap = 15;
        double grid = 30;
        double gthickness = 1.0;
        double lthickness = 2.0;
        List<Line> lines = new List<Line>();
        List<Line> grids = new List<Line>();

        public SheetWindow()
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
                Sheet.Children.Add(l);
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

        private void InitLine(Point p)
        {
            double z = zoom.ScaleX;
            double thickness = lthickness / z;
            var l = new Line() { Stroke = Brushes.Red, StrokeThickness = thickness };
            temp = l;
            l.X1 = Snap(p.X);
            l.Y1 = Snap(p.Y);
            l.X2 = Snap(p.X);
            l.Y2 = Snap(p.Y);
            lines.Add(l);
            Sheet.Children.Add(l);
            Sheet.CaptureMouse();
        }

        private void FinishLine()
        {
            Sheet.ReleaseMouseCapture();
            temp = null;
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
                l.X2 = Snap(p.X);
                l.Y2 = Snap(p.Y);
            }
        }

        private void Sheet_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Sheet.IsMouseCaptured && temp != null)
            {
                CancelLine();
            }
        }

        private void Workspace_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double z = zoom.ScaleX;

            if (e.Delta > 0)
            {
                z += 0.1;
                AdjustThickness(z);
                zoom.ScaleX = z;
                zoom.ScaleY = z;
            }
            else
            {
                if (Math.Round(z, 1) > 0.1)
                {
                    z -= 0.1;
                    AdjustThickness(z);
                    zoom.ScaleX = z;
                    zoom.ScaleY = z;
                }
            }
        }

        private void Workspace_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle && e.ClickCount == 2)
            {
                AdjustThickness(1.0);
                zoom.ScaleX = 1.0;
                zoom.ScaleY = 1.0;
            }
        }
    }
}
