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
    #region FrameworkElementProperties

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

        #endregion
    }

    #endregion

    #region WpfCanvasSheet

    public class WpfCanvasSheet : ISheet
    {
        #region Fields

        private Canvas _canvas;

        #endregion

        #region Constructor

        public WpfCanvasSheet(Canvas canvas)
        {
            this._canvas = canvas;
        }

        #endregion

        #region ISheet

        public double Width 
        {
            get { return _canvas.Width;  }
            set
            {
                _canvas.Width = value;
            }
        }

        public double Height
        {
            get { return _canvas.Height; }
            set
            {
                _canvas.Height = value;
            }
        }

        public bool IsCaptured
        {
            get { return _canvas.IsMouseCaptured; }
        }

        public object GetParent()
        {
            return _canvas;
        }

        public void Add(XElement element)
        {
            if (element != null && element.Element != null)
            {
                _canvas.Children.Add(element.Element as FrameworkElement);
            }
        }

        public void Remove(XElement element)
        {
            if (element != null && element.Element != null)
            {
                _canvas.Children.Remove(element.Element as FrameworkElement);
            }
        }

        public void Add(object element)
        {
            if (element != null)
            {
                _canvas.Children.Add(element as FrameworkElement);
            }
        }

        public void Remove(object element)
        {
            if (element != null)
            {
                _canvas.Children.Remove(element as FrameworkElement);
            }
        }

        public void Capture()
        {
            _canvas.CaptureMouse();
        }

        public void ReleaseCapture()
        {
            _canvas.ReleaseMouseCapture();
        }

        #endregion
    }

    #endregion

    #region WpfBlockHelper

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

        public static TextBlock GetTextBlock(XText text)
        {
            return (text.Element as Grid).Children[0] as TextBlock;
        }

        #endregion

        #region HitTest

        public bool HitTest(XElement element, XImmutableRect rect)
        {
            var r = new Rect(rect.X, rect.Y, rect.Width, rect.Height);
            var bounds = WpfVisualHelper.GetContentBounds(element);
            if (r.IntersectsWith(bounds))
            {
                return true;
            }
            return false;
        }

        public bool HitTest(XElement element, XImmutableRect rect, object relativeTo)
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

        public void SetIsSelected(XElement element, bool value)
        {
            FrameworkElementProperties.SetIsSelected(element.Element as FrameworkElement, value);
        }

        public bool GetIsSelected(XElement element)
        {
            return FrameworkElementProperties.GetIsSelected(element.Element as FrameworkElement);
        }

        public bool IsSelected(XPoint point)
        {
            throw new NotImplementedException();
        }

        public bool IsSelected(XLine line)
        {
            return (line.Element as Line).Stroke != WpfBlockFactory.SelectedBrush;
        }

        public bool IsSelected(XRectangle rectangle)
        {
            return (rectangle.Element as Rectangle).Stroke != WpfBlockFactory.SelectedBrush;
        }

        public bool IsSelected(XEllipse ellipse)
        {
            return (ellipse.Element as Ellipse).Stroke != WpfBlockFactory.SelectedBrush;
        }

        public bool IsSelected(XText text)
        {
            var tb = WpfBlockHelper.GetTextBlock(text);
            return tb.Foreground != WpfBlockFactory.SelectedBrush;
        }

        public bool IsSelected(XImage image)
        {
            return (image.Element as Image).OpacityMask != WpfBlockFactory.SelectedBrush;
        }

        #endregion

        #region Deselect

        public void Deselect(XPoint point)
        {
            throw new NotImplementedException();
        }

        public void Deselect(XLine line)
        {
            (line.Element as Line).Stroke = WpfBlockFactory.NormalBrush;
        }

        public void Deselect(XRectangle rectangle)
        {
            (rectangle.Element as Rectangle).Stroke = WpfBlockFactory.NormalBrush;
            (rectangle.Element as Rectangle).Fill = (rectangle.Element as Rectangle).Fill == WpfBlockFactory.TransparentBrush ? WpfBlockFactory.TransparentBrush : WpfBlockFactory.NormalBrush;
        }

        public void Deselect(XEllipse ellipse)
        {
            (ellipse.Element as Ellipse).Stroke = WpfBlockFactory.NormalBrush;
            (ellipse.Element as Ellipse).Fill = (ellipse.Element as Ellipse).Fill == WpfBlockFactory.TransparentBrush ? WpfBlockFactory.TransparentBrush : WpfBlockFactory.NormalBrush;
        }

        public void Deselect(XText text)
        {
            WpfBlockHelper.GetTextBlock(text).Foreground = WpfBlockFactory.NormalBrush;
        }

        public void Deselect(XImage image)
        {
            (image.Element as Image).OpacityMask = WpfBlockFactory.NormalBrush;
        }

        #endregion

        #region Select

        public void Select(XPoint point)
        {
            throw new NotImplementedException();
        }

        public void Select(XLine line)
        {
            (line.Element as Line).Stroke = WpfBlockFactory.SelectedBrush;
        }

        public void Select(XRectangle rectangle)
        {
            (rectangle.Element as Rectangle).Stroke = WpfBlockFactory.SelectedBrush;
            (rectangle.Element as Rectangle).Fill = (rectangle.Element as Rectangle).Fill == WpfBlockFactory.TransparentBrush ? WpfBlockFactory.TransparentBrush : WpfBlockFactory.SelectedBrush;
        }

        public void Select(XEllipse ellipse)
        {
            (ellipse.Element as Ellipse).Stroke = WpfBlockFactory.SelectedBrush;
            (ellipse.Element as Ellipse).Fill = (ellipse.Element as Ellipse).Fill == WpfBlockFactory.TransparentBrush ? WpfBlockFactory.TransparentBrush : WpfBlockFactory.SelectedBrush;
        }

        public void Select(XText text)
        {
            WpfBlockHelper.GetTextBlock(text).Foreground = WpfBlockFactory.SelectedBrush;
        }

        public void Select(XImage image)
        {
            (image.Element as Image).OpacityMask = WpfBlockFactory.SelectedBrush;
        }

        #endregion

        #region ZIndex

        public void SetZIndex(XElement element, int index)
        {
            Panel.SetZIndex(element.Element as FrameworkElement, index);
        }

        #endregion

        #region Fill

        public void ToggleFill(XRectangle rectangle)
        {
            (rectangle.Element as Rectangle).Fill = (rectangle.Element as Rectangle).Fill == WpfBlockFactory.TransparentBrush ? WpfBlockFactory.NormalBrush : WpfBlockFactory.TransparentBrush;
        }

        public void ToggleFill(XEllipse ellipse)
        {
            (ellipse.Element as Ellipse).Fill = (ellipse.Element as Ellipse).Fill == WpfBlockFactory.TransparentBrush ? WpfBlockFactory.NormalBrush : WpfBlockFactory.TransparentBrush;
        }

        public void ToggleFill(XPoint point)
        {
            (point.Element as Ellipse).Fill = (point.Element as Ellipse).Fill == WpfBlockFactory.TransparentBrush ? WpfBlockFactory.NormalBrush : WpfBlockFactory.TransparentBrush;
        }

        #endregion

        #region XElement

        public double GetLeft(XElement element)
        {
            return Canvas.GetLeft(element.Element as FrameworkElement);
        }

        public double GetTop(XElement element)
        {
            return Canvas.GetTop(element.Element as FrameworkElement);
        }

        public double GetWidth(XElement element)
        {
            return (element.Element as FrameworkElement).Width;
        }

        public double GetHeight(XElement element)
        {
            return (element.Element as FrameworkElement).Height;
        }

        public void SetLeft(XElement element, double left)
        {
            Canvas.SetLeft(element.Element as FrameworkElement, left);
        }

        public void SetTop(XElement element, double top)
        {
            Canvas.SetTop(element.Element as FrameworkElement, top);
        }

        public void SetWidth(XElement element, double width)
        {
            (element.Element as FrameworkElement).Width = width;
        }

        public void SetHeight(XElement element, double height)
        {
            (element.Element as FrameworkElement).Height = height;
        }

        #endregion

        #region XLine

        public double GetX1(XLine line)
        {
            return (line.Element as Line).X1;
        }

        public double GetY1(XLine line)
        {
            return (line.Element as Line).Y1;
        }

        public double GetX2(XLine line)
        {
            return (line.Element as Line).X2;
        }

        public double GetY2(XLine line)
        {
            return (line.Element as Line).Y2;
        }

        public ItemColor GetStroke(XLine line)
        {
            return GetItemColor((line.Element as Line).Stroke);
        }

        public void SetX1(XLine line, double x1)
        {
            (line.Element as Line).X1 = x1;
        }

        public void SetY1(XLine line, double y1)
        {
            (line.Element as Line).Y1 = y1;
        }

        public void SetX2(XLine line, double x2)
        {
            (line.Element as Line).X2 = x2;
        }

        public void SetY2(XLine line, double y2)
        {
            (line.Element as Line).Y2 = y2;
        }

        public void SetStrokeThickness(XLine line, double thickness)
        {
            (line.Element as Line).StrokeThickness = thickness;
        }

        public double GetStrokeThickness(XLine line)
        {
            return (line.Element as Line).StrokeThickness;
        }

        #endregion

        #region XRectangle

        public ItemColor GetStroke(XRectangle rectangle)
        {
            return GetItemColor((rectangle.Element as Rectangle).Stroke);
        }

        public ItemColor GetFill(XRectangle rectangle)
        {
            return GetItemColor((rectangle.Element as Rectangle).Fill);
        }

        public bool IsTransparent(XRectangle rectangle)
        {
            return (rectangle.Element as Rectangle).Fill == WpfBlockFactory.TransparentBrush ? false : true;
        }

        public void SetStrokeThickness(XRectangle rectangle, double thickness)
        {
            (rectangle.Element as Rectangle).StrokeThickness = thickness;
        }

        public double GetStrokeThickness(XRectangle rectangle)
        {
            return (rectangle.Element as Rectangle).StrokeThickness;
        }

        #endregion

        #region XEllipse

        public ItemColor GetStroke(XEllipse ellipse)
        {
            return GetItemColor((ellipse.Element as Ellipse).Stroke);
        }

        public ItemColor GetFill(XEllipse ellipse)
        {
            return GetItemColor((ellipse.Element as Ellipse).Fill);
        }

        public bool IsTransparent(XEllipse ellipse)
        {
            return (ellipse.Element as Ellipse).Fill == WpfBlockFactory.TransparentBrush ? false : true;
        }

        public void SetStrokeThickness(XEllipse ellipse, double thickness)
        {
            (ellipse.Element as Ellipse).StrokeThickness = thickness;
        }

        public double GetStrokeThickness(XEllipse ellipse)
        {
            return (ellipse.Element as Ellipse).StrokeThickness;
        }

        #endregion

        #region XText

        public ItemColor GetBackground(XText text)
        {
            var tb = WpfBlockHelper.GetTextBlock(text);
            return GetItemColor(tb.Background);

        }

        public ItemColor GetForeground(XText text)
        {
            var tb = WpfBlockHelper.GetTextBlock(text);
            return GetItemColor(tb.Foreground);
        }

        public string GetText(XText text)
        {
            var tb = WpfBlockHelper.GetTextBlock(text);
            return tb.Text;
        }

        public int GetHAlign(XText text)
        {
            var tb = WpfBlockHelper.GetTextBlock(text);
            return (int)tb.HorizontalAlignment;
        }

        public int GetVAlign(XText text)
        {
            var tb = WpfBlockHelper.GetTextBlock(text);
            return (int)tb.VerticalAlignment;
        }

        public double GetSize(XText text)
        {
            var tb = WpfBlockHelper.GetTextBlock(text);
            return tb.FontSize;
        }

        #endregion

        #region XImage

        public byte[] GetData(XImage image)
        {
            return (image.Element as Image).Tag as byte[];
        }

        #endregion
    }

    #endregion

    #region WpfBlockFactory

    public class WpfBlockFactory : IBlockFactory
    {
        #region Brushes

        public static SolidColorBrush NormalBrush = Brushes.Black;
        public static SolidColorBrush SelectedBrush = Brushes.Red;
        public static SolidColorBrush HoverBrush = Brushes.Yellow;
        public static SolidColorBrush TransparentBrush = Brushes.Transparent;

        #endregion

        #region Styles

        private void SetStyle(Ellipse ellipse, bool isVisible)
        {
            var style = new Style(typeof(Ellipse));
            style.Setters.Add(new Setter(Ellipse.FillProperty, isVisible ? NormalBrush : TransparentBrush));
            style.Setters.Add(new Setter(Ellipse.StrokeProperty, isVisible ? NormalBrush : TransparentBrush));

            var isSelectedTrigger = new Trigger() { Property = FrameworkElementProperties.IsSelectedProperty, Value = true };
            isSelectedTrigger.Setters.Add(new Setter(Ellipse.FillProperty, SelectedBrush));
            isSelectedTrigger.Setters.Add(new Setter(Ellipse.StrokeProperty, SelectedBrush));
            style.Triggers.Add(isSelectedTrigger);

            var isMouseOverTrigger = new Trigger() { Property = Ellipse.IsMouseOverProperty, Value = true };
            isMouseOverTrigger.Setters.Add(new Setter(Ellipse.FillProperty, HoverBrush));
            isMouseOverTrigger.Setters.Add(new Setter(Ellipse.StrokeProperty, HoverBrush));
            style.Triggers.Add(isMouseOverTrigger);

            ellipse.Style = style;

            FrameworkElementProperties.SetIsSelected(ellipse, false);
        }

        #endregion

        #region Create

        public XPoint CreatePoint(double thickness, double x, double y, bool isVisible)
        {
            var ellipse = new Ellipse()
            {
                StrokeThickness = thickness,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                Width = 8.0,
                Height = 8.0,
                Margin = new Thickness(-4.0, -4.0, 0.0, 0.0),
            };

            SetStyle(ellipse, isVisible);
            Panel.SetZIndex(ellipse, 1);

            Canvas.SetLeft(ellipse, x);
            Canvas.SetTop(ellipse, y);

            var xpoint = new XPoint(ellipse, x, y, isVisible);

            return xpoint;
        }

        public XLine CreateLine(double thickness, double x1, double y1, double x2, double y2, ItemColor stroke)
        {
            var strokeBrush = new SolidColorBrush(Color.FromArgb(stroke.Alpha, stroke.Red, stroke.Green, stroke.Blue));

            strokeBrush.Freeze();

            var line = new Line()
            {
                Stroke = strokeBrush,
                StrokeThickness = thickness,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2
            };

            var xline = new XLine(line);

            return xline;
        }

        public XLine CreateLine(double thickness, XPoint start, XPoint end, ItemColor stroke)
        {
            var xline = CreateLine(thickness, start.X, start.Y, end.X, end.Y, stroke);
            xline.Start = start;
            xline.End = end;
            return xline;
        }

        public XRectangle CreateRectangle(double thickness, double x, double y, double width, double height, bool isFilled, ItemColor stroke, ItemColor fill)
        {
            var strokeBrush = new SolidColorBrush(Color.FromArgb(stroke.Alpha, stroke.Red, stroke.Green, stroke.Blue));
            var fillBrush = new SolidColorBrush(Color.FromArgb(fill.Alpha, fill.Red, fill.Green, fill.Blue));

            strokeBrush.Freeze();
            fillBrush.Freeze();

            var rectangle = new Rectangle()
            {
                Fill = isFilled ? fillBrush : TransparentBrush,
                Stroke = strokeBrush,
                StrokeThickness = thickness,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                Width = width,
                Height = height
            };

            Canvas.SetLeft(rectangle, x);
            Canvas.SetTop(rectangle, y);

            var xrectangle = new XRectangle(rectangle);

            return xrectangle;
        }

        public XEllipse CreateEllipse(double thickness, double x, double y, double width, double height, bool isFilled, ItemColor stroke, ItemColor fill)
        {
            var strokeBrush = new SolidColorBrush(Color.FromArgb(stroke.Alpha, stroke.Red, stroke.Green, stroke.Blue));
            var fillBrush = new SolidColorBrush(Color.FromArgb(fill.Alpha, fill.Red, fill.Green, fill.Blue));

            strokeBrush.Freeze();
            fillBrush.Freeze();

            var ellipse = new Ellipse()
            {
                Fill = isFilled ? fillBrush : TransparentBrush,
                Stroke = strokeBrush,
                StrokeThickness = thickness,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                Width = width,
                Height = height
            };

            Canvas.SetLeft(ellipse, x);
            Canvas.SetTop(ellipse, y);

            var xellipse = new XEllipse(ellipse);

            return xellipse;
        }

        public XText CreateText(string text, double x, double y, double width, double height, int halign, int valign, double fontSize, ItemColor backgroud, ItemColor foreground)
        {
            var backgroundBrush = new SolidColorBrush(Color.FromArgb(backgroud.Alpha, backgroud.Red, backgroud.Green, backgroud.Blue));
            var foregroundBrush = new SolidColorBrush(Color.FromArgb(foreground.Alpha, foreground.Red, foreground.Green, foreground.Blue));

            backgroundBrush.Freeze();
            foregroundBrush.Freeze();

            var grid = new Grid();
            grid.Background = backgroundBrush;
            grid.Width = width;
            grid.Height = height;
            Canvas.SetLeft(grid, x);
            Canvas.SetTop(grid, y);

            var tb = new TextBlock();
            tb.HorizontalAlignment = (HorizontalAlignment)halign;
            tb.VerticalAlignment = (VerticalAlignment)valign;
            tb.Background = backgroundBrush;
            tb.Foreground = foregroundBrush;
            tb.FontSize = fontSize;
            tb.FontFamily = new FontFamily("Calibri");
            tb.Text = text;

            grid.Children.Add(tb);

            var xtext = new XText(grid);

            return xtext;
        }

        public XImage CreateImage(double x, double y, double width, double height, byte[] data)
        {
            Image image = new Image();

            // enable high quality image scaling
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);

            // store original image data is Tag property
            image.Tag = data;

            // opacity mask is used for determining selection state
            image.OpacityMask = NormalBrush;

            //using(var ms = new System.IO.MemoryStream(data))
            //{
            //    image = Image.FromStream(ms);
            //}
            using (var ms = new System.IO.MemoryStream(data))
            {
                IBitmap profileImage = BitmapLoader.Current.Load(ms, null, null).Result;
                image.Source = profileImage.ToNative();
            }

            image.Width = width;
            image.Height = height;

            Canvas.SetLeft(image, x);
            Canvas.SetTop(image, y);

            var ximage = new XImage(image);

            return ximage;
        }

        #endregion
    } 
    
    #endregion
}
