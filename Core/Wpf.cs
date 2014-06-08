using Sheet.Block.Core;
using Sheet.Block.Model;
using Sheet.Item.Model;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
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

    #region WpfClipboard

    public class WpfClipboard : IClipboard
    {
        public void Set(string text)
        {
            Clipboard.SetData(DataFormats.UnicodeText, text);
        }

        public string Get()
        {
            return (string)Clipboard.GetData(DataFormats.UnicodeText);
        }
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

    #endregion

    #region WpfCanvasSheet

    public class WpfCanvasSheet : ISheet
    {
        #region Fields

        private Canvas _canvas;

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

        public void SetParent(object parent)
        {
            _canvas = parent as Canvas;
        }

        public void Add(IElement element)
        {
            if (element != null && element.Native != null)
            {
                _canvas.Children.Add(element.Native as FrameworkElement);
            }
        }

        public void Remove(IElement element)
        {
            if (element != null && element.Native != null)
            {
                _canvas.Children.Remove(element.Native as FrameworkElement);
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

        public static TextBlock GetTextBlock(IText text)
        {
            return (text.Native as Grid).Children[0] as TextBlock;
        }

        #endregion

        #region HitTest

        public bool HitTest(IElement element, XImmutableRect rect)
        {
            var r = new Rect(rect.X, rect.Y, rect.Width, rect.Height);
            var bounds = WpfVisualHelper.GetContentBounds(element);
            if (r.IntersectsWith(bounds))
            {
                return true;
            }
            return false;
        }

        public bool HitTest(IElement element, XImmutableRect rect, object relativeTo)
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

        #region Thumb

        private string ThumbTemplate = "<Thumb Cursor=\"SizeAll\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><Thumb.Template><ControlTemplate><Rectangle Fill=\"Transparent\" Stroke=\"Red\" StrokeThickness=\"2\" Width=\"8\" Height=\"8\" Margin=\"-4,-4,0,0\"/></ControlTemplate></Thumb.Template></Thumb>";

        private void SetLineDragDeltaHandler(ILine line, IThumb thumb, Action<ILine, IThumb, double, double> drag)
        {
            (thumb.Native as Thumb).DragDelta += (sender, e) => drag(line, thumb, e.HorizontalChange, e.VerticalChange);
        }

        private void SetElementDragDeltaHandler(IElement element, IThumb thumb, Action<IElement, IThumb, double, double> drag)
        {
            (thumb.Native as Thumb).DragDelta += (sender, e) => drag(element, thumb, e.HorizontalChange, e.VerticalChange);
        }

        #endregion

        #region Color

        private IArgbColor ToArgbColor(ItemColor color)
        {
            if (color != null)
            {
                return new XArgbColor(color.Alpha, color.Red, color.Green, color.Blue);
            }
            return null;
        }

        #endregion

        #region Create

        public IThumb CreateThumb(double x, double y)
        {
            using (var stringReader = new System.IO.StringReader(ThumbTemplate))
            {
                using (var xmlReader = System.Xml.XmlReader.Create(stringReader))
                {
                    var thumb = XamlReader.Load(xmlReader) as Thumb;
                    Canvas.SetLeft(thumb, x);
                    Canvas.SetTop(thumb, y);
                    return new XThumb(thumb);
                }
            }
        }

        public IThumb CreateThumb(double x, double y, ILine line, Action<ILine, IThumb, double, double> drag)
        {
            var thumb = CreateThumb(x, y);
            SetLineDragDeltaHandler(line, thumb, drag);
            return thumb;
        }

        public IThumb CreateThumb(double x, double y, IElement element, Action<IElement, IThumb, double, double> drag)
        {
            var thumb = CreateThumb(x, y);
            SetElementDragDeltaHandler(element, thumb, drag);
            return thumb;
        }

        public IPoint CreatePoint(double thickness, double x, double y, bool isVisible)
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

        public ILine CreateLine(double thickness, double x1, double y1, double x2, double y2, ItemColor stroke)
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

        public ILine CreateLine(double thickness, IPoint start, IPoint end, ItemColor stroke)
        {
            var xline = CreateLine(thickness, start.X, start.Y, end.X, end.Y, stroke);
            xline.Start = start;
            xline.End = end;
            return xline;
        }

        public IRectangle CreateRectangle(double thickness, double x, double y, double width, double height, bool isFilled, ItemColor stroke, ItemColor fill)
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

        public IEllipse CreateEllipse(double thickness, double x, double y, double width, double height, bool isFilled, ItemColor stroke, ItemColor fill)
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

        public IText CreateText(string text, double x, double y, double width, double height, int halign, int valign, double fontSize, ItemColor backgroud, ItemColor foreground)
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

        public IImage CreateImage(double x, double y, double width, double height, byte[] data)
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

        public IBlock CreateBlock(int id, double x, double y, double width, double height, int dataId, string name, ItemColor backgroud)
        {
            var xblock = new XBlock(id, x, y, width, height, dataId, name) 
            { 
                Backgroud = ToArgbColor(backgroud)
            };
            return xblock;
        }

        #endregion
    } 
    
    #endregion
}
