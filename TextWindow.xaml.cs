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
using System.Windows.Shapes;

namespace Sheet
{
    public partial class TextWindow : Window
    {
        #region Constructor

        public TextWindow()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += TextWindow_Loaded;
        }

        #endregion

        #region Properties

        public string Label { get; set; }

        public string Text { get; set; }

        #endregion

        #region Events

        private void TextWindow_Loaded(object sender, RoutedEventArgs e)
        {
            TextValue.Focus();
            TextValue.CaretIndex = TextValue.Text.Length;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        #endregion
    }
}
