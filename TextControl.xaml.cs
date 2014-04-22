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
    public partial class TextControl : UserControl, ITextEditor
    {
        #region Constructor

        public TextControl()
        {
            InitializeComponent();
        } 

        #endregion

        #region Fields

        private Action<string> okAction = null;
        private Action cancelAction = null; 

        #endregion

        #region ITextEditor

        public void Show(Action<string> ok, Action cancel, string label, string text)
        {
            okAction = ok;
            cancelAction = cancel;
            TextLabel.Text = label;
            TextValue.Text = text;
            Visibility = Visibility.Visible;
            TextValue.Focus();
            TextValue.CaretIndex = text.Length;
        }

        #endregion

        #region Events

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Hidden;

            if (okAction != null)
            {
                okAction(TextValue.Text);
            }

            okAction = null;
            cancelAction = null;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Hidden;

            if (cancelAction != null)
            {
                cancelAction();
            }

            okAction = null;
            cancelAction = null;
        }                 

        #endregion
    }
}
