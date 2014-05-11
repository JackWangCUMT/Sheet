using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public partial class SolutionControl : UserControl
    {
        #region Constructor

        public SolutionControl()
        {
            InitializeComponent();
        }

        #endregion

        #region TreeView Events

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // TODO: 
        }

        private void TreeViewItem_MouseRightButtonDown(object sender, MouseEventArgs e)
        {
            var item = sender as TreeViewItem;
            if (item != null)
            {
                item.IsSelected = true;
                item.BringIntoView();
                e.Handled = true;
            }
        }

        #endregion

        #region Page Menu Events

        private void PageAdd_Click(object sender, RoutedEventArgs e)
        {
            // TODO:
        }

        private void PageDuplicate_Click(object sender, RoutedEventArgs e)
        {
            // TODO:
        }

        private void PageRemove_Click(object sender, RoutedEventArgs e)
        {
            // TODO:
        }

        private void PageExport_Click(object sender, RoutedEventArgs e)
        {
            // TODO:
        } 

        #endregion

        #region Document Menu Events

        private void DocumentAddPage_Click(object sender, RoutedEventArgs e)
        {
            // TODO:
        }

        private void DocumentAdd_Click(object sender, RoutedEventArgs e)
        {
            // TODO:
        }

        private void DocumentDuplicate_Click(object sender, RoutedEventArgs e)
        {
            // TODO:
        }

        private void DocumentRemove_Click(object sender, RoutedEventArgs e)
        {
            // TODO:
        }

        private void DocumentExport_Click(object sender, RoutedEventArgs e)
        {
            // TODO:
        } 

        #endregion

        #region Tree Menu Events

        private void TreeAddDocument_Click(object sender, RoutedEventArgs e)
        {
            // TODO:
        }

        #endregion
    }
}
