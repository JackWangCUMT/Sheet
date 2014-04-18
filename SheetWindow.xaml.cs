using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public partial class SheetWindow : Window
    {
        #region Constructor

        public SheetWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Events

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.H:
                    Help.Visibility = Help.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
                    break;
            }
        }

        #endregion
    }
}
