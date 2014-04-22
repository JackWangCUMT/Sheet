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
    public partial class LibraryControl : UserControl, ILibrary
    {
        #region Constructor

        public LibraryControl()
        {
            InitializeComponent();
        } 

        #endregion

        #region ILibrary

        public BlockItem GetSelected()
        {
            if (Blocks != null && Blocks.SelectedIndex >= 0)
            {
                return Blocks.SelectedItem as BlockItem;
            }
            return null;
        }

        public void SetSelected(BlockItem block)
        {
            if (Blocks != null)
            {
                Blocks.SelectedItem = block;
            }
        }

        public IEnumerable<BlockItem> GetSource()
        {
            if (Blocks != null)
            {
                return Blocks.ItemsSource as IEnumerable<BlockItem>;
            }
            return null;
        }

        public void SetSource(IEnumerable<BlockItem> source)
        {
            if (Blocks != null)
            {
                Blocks.ItemsSource = null;
                Blocks.ItemsSource = source;
                Blocks.SelectedIndex = 0;

                if (source.Count() == 0)
                {
                    Visibility = Visibility.Collapsed;
                }
                else
                {
                    Visibility = Visibility.Visible;
                }
            }
        } 

        #endregion
    }
}
