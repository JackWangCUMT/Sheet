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
    #region Wpf Helper

    public static class WpfHelper
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

        public static Rect GetContentBounds(UIElement reference)
        {
            var bounds = VisualTreeHelper.GetContentBounds(reference);
            return bounds;
        }

        public static Rect GetContentBounds(UIElement reference, UIElement relativeTo)
        {
            var bounds = VisualTreeHelper.GetContentBounds(reference);
            var offset = reference.TranslatePoint(new Point(0, 0), relativeTo);
            if (bounds != null && bounds.IsEmpty == false)
            {
                bounds.Offset(offset.X, offset.Y);
            }
            return bounds;
        }
    }

    #endregion
}
