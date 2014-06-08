using Sheet.Block.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Sheet.WPF
{
    public static class WpfVisualHelper
    {
        #region Visual Parent

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

        #endregion

        #region Content Bounds

        public static Rect GetContentBounds(IElement reference)
        {
            var bounds = VisualTreeHelper.GetContentBounds(reference.Native as UIElement);
            return bounds;
        }

        public static Rect GetContentBounds(IElement reference, object relativeTo)
        {
            if (reference.Native != null)
            {
                var bounds = VisualTreeHelper.GetContentBounds(reference.Native as UIElement);
                var offset = (reference.Native as UIElement).TranslatePoint(new Point(0, 0), relativeTo as UIElement);
                if (bounds.IsEmpty == false)
                {
                    bounds.Offset(offset.X, offset.Y);
                }
                return bounds;
            }
            return Rect.Empty;
        }

        #endregion
    }
}
