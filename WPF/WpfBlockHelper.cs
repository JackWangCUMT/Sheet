using Sheet.Block;
using Sheet.Block.Core;
using Sheet.Block.Model;
using Sheet.Item.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Sheet.WPF
{
    public class WpfBlockHelper : IBlockHelper
    {
        #region Get

        public static ItemColor GetItemColor(Brush brush)
        {
            var color = (brush as SolidColorBrush).Color;
            return new ItemColor()
            {
                Alpha = color.A,
                Red = color.R,
                Green = color.G,
                Blue = color.B
            };
        }

        public static TextBlock GetTextBlock(IText text)
        {
            return (text.Native as Grid).Children[0] as TextBlock;
        }

        #endregion

        #region HitTest

        public bool HitTest(IElement element, ImmutableRect rect)
        {
            var r = new Rect(rect.X, rect.Y, rect.Width, rect.Height);
            var bounds = WpfVisualHelper.GetContentBounds(element);
            if (r.IntersectsWith(bounds))
            {
                return true;
            }
            return false;
        }

        public bool HitTest(IElement element, ImmutableRect rect, object relativeTo)
        {
            var r = new Rect(rect.X, rect.Y, rect.Width, rect.Height);
            var bounds = WpfVisualHelper.GetContentBounds(element, relativeTo);
            if (r.IntersectsWith(bounds))
            {
                return true;
            }
            return false;
        }

        #endregion

        #region IsSelected

        public void SetIsSelected(IElement element, bool value)
        {
            FrameworkElementProperties.SetIsSelected(element.Native as FrameworkElement, value);
        }

        public bool GetIsSelected(IElement element)
        {
            return FrameworkElementProperties.GetIsSelected(element.Native as FrameworkElement);
        }

        public bool IsSelected(IPoint point)
        {
            throw new NotImplementedException();
        }

        public bool IsSelected(ILine line)
        {
            return (line.Native as Line).Stroke != WpfBlockFactory.SelectedBrush;
        }

        public bool IsSelected(IRectangle rectangle)
        {
            return (rectangle.Native as Rectangle).Stroke != WpfBlockFactory.SelectedBrush;
        }

        public bool IsSelected(IEllipse ellipse)
        {
            return (ellipse.Native as Ellipse).Stroke != WpfBlockFactory.SelectedBrush;
        }

        public bool IsSelected(IText text)
        {
            var tb = WpfBlockHelper.GetTextBlock(text);
            return tb.Foreground != WpfBlockFactory.SelectedBrush;
        }

        public bool IsSelected(IImage image)
        {
            return (image.Native as Image).OpacityMask != WpfBlockFactory.SelectedBrush;
        }

        #endregion

        #region Deselect

        public void Deselect(IPoint point)
        {
            throw new NotImplementedException();
        }

        public void Deselect(ILine line)
        {
            (line.Native as Line).Stroke = WpfBlockFactory.NormalBrush;
        }

        public void Deselect(IRectangle rectangle)
        {
            (rectangle.Native as Rectangle).Stroke = WpfBlockFactory.NormalBrush;
            (rectangle.Native as Rectangle).Fill = (rectangle.Native as Rectangle).Fill == WpfBlockFactory.TransparentBrush ? WpfBlockFactory.TransparentBrush : WpfBlockFactory.NormalBrush;
        }

        public void Deselect(IEllipse ellipse)
        {
            (ellipse.Native as Ellipse).Stroke = WpfBlockFactory.NormalBrush;
            (ellipse.Native as Ellipse).Fill = (ellipse.Native as Ellipse).Fill == WpfBlockFactory.TransparentBrush ? WpfBlockFactory.TransparentBrush : WpfBlockFactory.NormalBrush;
        }

        public void Deselect(IText text)
        {
            WpfBlockHelper.GetTextBlock(text).Foreground = WpfBlockFactory.NormalBrush;
        }

        public void Deselect(IImage image)
        {
            (image.Native as Image).OpacityMask = WpfBlockFactory.NormalBrush;
        }

        #endregion

        #region Select

        public void Select(IPoint point)
        {
            throw new NotImplementedException();
        }

        public void Select(ILine line)
        {
            (line.Native as Line).Stroke = WpfBlockFactory.SelectedBrush;
        }

        public void Select(IRectangle rectangle)
        {
            (rectangle.Native as Rectangle).Stroke = WpfBlockFactory.SelectedBrush;
            (rectangle.Native as Rectangle).Fill = (rectangle.Native as Rectangle).Fill == WpfBlockFactory.TransparentBrush ? WpfBlockFactory.TransparentBrush : WpfBlockFactory.SelectedBrush;
        }

        public void Select(IEllipse ellipse)
        {
            (ellipse.Native as Ellipse).Stroke = WpfBlockFactory.SelectedBrush;
            (ellipse.Native as Ellipse).Fill = (ellipse.Native as Ellipse).Fill == WpfBlockFactory.TransparentBrush ? WpfBlockFactory.TransparentBrush : WpfBlockFactory.SelectedBrush;
        }

        public void Select(IText text)
        {
            WpfBlockHelper.GetTextBlock(text).Foreground = WpfBlockFactory.SelectedBrush;
        }

        public void Select(IImage image)
        {
            (image.Native as Image).OpacityMask = WpfBlockFactory.SelectedBrush;
        }

        #endregion

        #region ZIndex

        public void SetZIndex(IElement element, int index)
        {
            Panel.SetZIndex(element.Native as FrameworkElement, index);
        }

        #endregion

        #region Fill

        public void ToggleFill(IRectangle rectangle)
        {
            (rectangle.Native as Rectangle).Fill = (rectangle.Native as Rectangle).Fill == WpfBlockFactory.TransparentBrush ? WpfBlockFactory.NormalBrush : WpfBlockFactory.TransparentBrush;
        }

        public void ToggleFill(IEllipse ellipse)
        {
            (ellipse.Native as Ellipse).Fill = (ellipse.Native as Ellipse).Fill == WpfBlockFactory.TransparentBrush ? WpfBlockFactory.NormalBrush : WpfBlockFactory.TransparentBrush;
        }

        public void ToggleFill(IPoint point)
        {
            (point.Native as Ellipse).Fill = (point.Native as Ellipse).Fill == WpfBlockFactory.TransparentBrush ? WpfBlockFactory.NormalBrush : WpfBlockFactory.TransparentBrush;
        }

        #endregion

        #region XElement

        public double GetLeft(IElement element)
        {
            return Canvas.GetLeft(element.Native as FrameworkElement);
        }

        public double GetTop(IElement element)
        {
            return Canvas.GetTop(element.Native as FrameworkElement);
        }

        public double GetWidth(IElement element)
        {
            return (element.Native as FrameworkElement).Width;
        }

        public double GetHeight(IElement element)
        {
            return (element.Native as FrameworkElement).Height;
        }

        public void SetLeft(IElement element, double left)
        {
            Canvas.SetLeft(element.Native as FrameworkElement, left);
        }

        public void SetTop(IElement element, double top)
        {
            Canvas.SetTop(element.Native as FrameworkElement, top);
        }

        public void SetWidth(IElement element, double width)
        {
            (element.Native as FrameworkElement).Width = width;
        }

        public void SetHeight(IElement element, double height)
        {
            (element.Native as FrameworkElement).Height = height;
        }

        #endregion

        #region ILine

        public double GetX1(ILine line)
        {
            return (line.Native as Line).X1;
        }

        public double GetY1(ILine line)
        {
            return (line.Native as Line).Y1;
        }

        public double GetX2(ILine line)
        {
            return (line.Native as Line).X2;
        }

        public double GetY2(ILine line)
        {
            return (line.Native as Line).Y2;
        }

        public ItemColor GetStroke(ILine line)
        {
            return GetItemColor((line.Native as Line).Stroke);
        }

        public void SetX1(ILine line, double x1)
        {
            (line.Native as Line).X1 = x1;
        }

        public void SetY1(ILine line, double y1)
        {
            (line.Native as Line).Y1 = y1;
        }

        public void SetX2(ILine line, double x2)
        {
            (line.Native as Line).X2 = x2;
        }

        public void SetY2(ILine line, double y2)
        {
            (line.Native as Line).Y2 = y2;
        }

        public void SetStrokeThickness(ILine line, double thickness)
        {
            (line.Native as Line).StrokeThickness = thickness;
        }

        public double GetStrokeThickness(ILine line)
        {
            return (line.Native as Line).StrokeThickness;
        }

        #endregion

        #region IRectangle

        public ItemColor GetStroke(IRectangle rectangle)
        {
            return GetItemColor((rectangle.Native as Rectangle).Stroke);
        }

        public ItemColor GetFill(IRectangle rectangle)
        {
            return GetItemColor((rectangle.Native as Rectangle).Fill);
        }

        public bool IsTransparent(IRectangle rectangle)
        {
            return (rectangle.Native as Rectangle).Fill == WpfBlockFactory.TransparentBrush ? false : true;
        }

        public void SetStrokeThickness(IRectangle rectangle, double thickness)
        {
            (rectangle.Native as Rectangle).StrokeThickness = thickness;
        }

        public double GetStrokeThickness(IRectangle rectangle)
        {
            return (rectangle.Native as Rectangle).StrokeThickness;
        }

        #endregion

        #region IEllipse

        public ItemColor GetStroke(IEllipse ellipse)
        {
            return GetItemColor((ellipse.Native as Ellipse).Stroke);
        }

        public ItemColor GetFill(IEllipse ellipse)
        {
            return GetItemColor((ellipse.Native as Ellipse).Fill);
        }

        public bool IsTransparent(IEllipse ellipse)
        {
            return (ellipse.Native as Ellipse).Fill == WpfBlockFactory.TransparentBrush ? false : true;
        }

        public void SetStrokeThickness(IEllipse ellipse, double thickness)
        {
            (ellipse.Native as Ellipse).StrokeThickness = thickness;
        }

        public double GetStrokeThickness(IEllipse ellipse)
        {
            return (ellipse.Native as Ellipse).StrokeThickness;
        }

        #endregion

        #region IText

        public ItemColor GetBackground(IText text)
        {
            var tb = WpfBlockHelper.GetTextBlock(text);
            return GetItemColor(tb.Background);

        }

        public ItemColor GetForeground(IText text)
        {
            var tb = WpfBlockHelper.GetTextBlock(text);
            return GetItemColor(tb.Foreground);
        }

        public string GetText(IText text)
        {
            var tb = WpfBlockHelper.GetTextBlock(text);
            return tb.Text;
        }

        public int GetHAlign(IText text)
        {
            var tb = WpfBlockHelper.GetTextBlock(text);
            return (int)tb.HorizontalAlignment;
        }

        public int GetVAlign(IText text)
        {
            var tb = WpfBlockHelper.GetTextBlock(text);
            return (int)tb.VerticalAlignment;
        }

        public double GetSize(IText text)
        {
            var tb = WpfBlockHelper.GetTextBlock(text);
            return tb.FontSize;
        }

        #endregion

        #region IImage

        public byte[] GetData(IImage image)
        {
            return (image.Native as Image).Tag as byte[];
        }

        #endregion
    }
}
