using Sheet.Block;
using Sheet.Block.Core;
using Sheet.Block.Model;
using Sheet.Item.Model;
using Sheet.Simulation.Core;
using Sheet.Simulation.Tests;
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
    public class WpfBlockSimulationHelper : IBlockSimulationHelper
    {
        #region Fields

        private readonly SolidColorBrush NullBrush;
        private readonly SolidColorBrush FalseBrush;
        private readonly SolidColorBrush TrueBrush; 

        #endregion

        #region Contructor

        public WpfBlockSimulationHelper()
        {
            NullBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x66, 0x66, 0x66));
            NullBrush.Freeze();

            FalseBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0xBF, 0xFF));
            FalseBrush.Freeze();

            TrueBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x14, 0x93));
            TrueBrush.Freeze();
        }

        #endregion

        #region Brush Selector

        private SolidColorBrush SelectStateBrush(IBoolState state)
        {
            if (state.State == true)
                return TrueBrush;
            else if (state.State == false)
                return FalseBrush;
            else
                return NullBrush;
        }

        #endregion

        #region Set State

        public void SeState(ILine line, IBoolState state)
        {
            (line.Native as Line).Stroke = SelectStateBrush(state);
        }

        public void SeState(IRectangle rectangle, IBoolState state)
        {
            (rectangle.Native as Rectangle).Stroke = SelectStateBrush(state);
            (rectangle.Native as Rectangle).Fill = (rectangle.Native as Rectangle).Fill == WpfBlockFactory.TransparentBrush ? WpfBlockFactory.TransparentBrush : SelectStateBrush(state);
        }

        public void SeState(IEllipse ellipse, IBoolState state)
        {
            (ellipse.Native as Ellipse).Stroke = SelectStateBrush(state);
            (ellipse.Native as Ellipse).Fill = (ellipse.Native as Ellipse).Fill == WpfBlockFactory.TransparentBrush ? WpfBlockFactory.TransparentBrush : SelectStateBrush(state);
        }

        public void SeState(IText text, IBoolState state)
        {
            WpfBlockHelper.GetTextBlock(text).Foreground = SelectStateBrush(state);
        }

        public void SeState(IImage image, IBoolState state)
        {
            (image.Native as Image).OpacityMask = SelectStateBrush(state);
        }

        public void SeState(IBlock parent, IBoolState state)
        {
            if (parent.Lines != null)
            {
                foreach (var line in parent.Lines)
                {
                    SeState(line, state);
                }
            }

            if (parent.Rectangles != null)
            {
                foreach (var rectangle in parent.Rectangles)
                {
                    SeState(rectangle, state);
                }
            }

            if (parent.Ellipses != null)
            {
                foreach (var ellipse in parent.Ellipses)
                {
                    SeState(ellipse, state);
                }
            }

            if (parent.Texts != null)
            {
                foreach (var text in parent.Texts)
                {
                    SeState(text, state);
                }
            }

            if (parent.Images != null)
            {
                foreach (var image in parent.Images)
                {
                    SeState(image, state);
                }
            }

            if (parent.Blocks != null)
            {
                foreach (var block in parent.Blocks)
                {
                    SeState(block, state);
                }
            }
        } 

        #endregion
    }
}
