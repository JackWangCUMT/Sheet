using Sheet.Block.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Sheet.WPF
{
    public class WpfCanvasSheet : ISheet
    {
        #region Fields

        private Canvas _canvas;

        #endregion

        #region ISheet

        public double Width
        {
            get { return _canvas.Width; }
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
}
