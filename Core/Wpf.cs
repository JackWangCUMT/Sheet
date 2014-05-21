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
    #region FrameworkElement Properties

    public static class FrameworkElementProperties
    {
        #region IsSelected

        public static bool GetIsSelected(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsSelectedProperty);
        }

        public static void SetIsSelected(DependencyObject obj, bool value)
        {
            obj.SetValue(IsSelectedProperty, value);
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(FrameworkElement), new PropertyMetadata(false));

        #endregion
    }

    #endregion

    #region WpfVisualHelper

    public static class WpfVisualHelper
    {
        public static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null)
            {
                return null;
            }

            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                return FindVisualParent<T>(parentObject);
            }
        }

        public static Rect GetContentBounds(XElement reference)
        {
            var bounds = VisualTreeHelper.GetContentBounds(reference.Element as UIElement);
            return bounds;
        }

        public static Rect GetContentBounds(XElement reference, object relativeTo)
        {
            if (reference.Element != null)
            {
                var bounds = VisualTreeHelper.GetContentBounds(reference.Element as UIElement);
                var offset = (reference.Element as UIElement).TranslatePoint(new Point(0, 0), relativeTo as UIElement);
                if (bounds != null && bounds.IsEmpty == false)
                {
                    bounds.Offset(offset.X, offset.Y);
                }
                return bounds;
            }
            return Rect.Empty;
        }
    }

    #endregion

    #region WpfCanvasSheet

    public class WpfCanvasSheet : ISheet
    {
        #region Fields

        private Canvas canvas = null;

        #endregion

        #region Constructor

        public WpfCanvasSheet(Canvas canvas)
        {
            this.canvas = canvas;
        }

        #endregion

        #region ISheet

        public object GetParent()
        {
            return canvas;
        }

        public void Add(XElement element)
        {
            if (element != null && element.Element != null)
            {
                canvas.Children.Add(element.Element as FrameworkElement);
            }
        }

        public void Remove(XElement element)
        {
            if (element != null && element.Element != null)
            {
                canvas.Children.Remove(element.Element as FrameworkElement);
            }
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
}
