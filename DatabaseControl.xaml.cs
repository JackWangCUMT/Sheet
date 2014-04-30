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
    #region CsvReader

    using Microsoft.VisualBasic.FileIO;

    public static class CsvReader
    {
        public static IEnumerable<string[]> Read(string path)
        {
            // reference Microsoft.VisualBasic, namespace Microsoft.VisualBasic.FileIO
            using (TextFieldParser parser = new TextFieldParser(path))
            {
                parser.CommentTokens = new string[] { "#" };
                parser.SetDelimiters(new string[] { ";" });
                parser.HasFieldsEnclosedInQuotes = true;
                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    yield return fields;
                }
            }
        }
    } 

    #endregion

    public partial class DatabaseControl : UserControl, IDatabase
    {
        #region Fields

        private Point dragStartPoint;

        #endregion

        #region Properties

        private string[] columns = null;
        private List<string[]> data = null;

        public string[] Columns
        {
            get { return columns; }
            set
            {
                columns = value;
            }
        }

        public List<string[]> Data
        {
            get { return data;  }
            set
            {
                data = value;
            }
        }

        #endregion

        #region Constructor

        public DatabaseControl()
        {
            InitializeComponent();
        }

        #endregion

        #region IDatabase

        public string[] Get(int index)
        {
            return Data.Where(x => int.Parse(x[0]) == index).FirstOrDefault();
        }

        public bool Update(int index, string[] item)
        {
            for (int i = 0; i < Data.Count; i++)
            {
                if (int.Parse(Data[i][0]) == index)
                {
                    data[i] = item;
                    return true;
                }
            }
            return false;
        }

        public int Add(string[] item)
        {
            int index = data.Max((x) => int.Parse(x[0])) + 1;
            item[0] = index.ToString();
            data.Add(item);
            return index;
        }

        #endregion

        #region Open

        public string Open()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*"
            };

            var result = dlg.ShowDialog();
            if (result.HasValue && result.Value == true)
            {
                Open(dlg.FileName);
                return System.IO.Path.GetFileName(dlg.FileName);
            }
            return null;
        }

        public void Open(string fileName)
        {
            var fields = CsvReader.Read(fileName);
            SetColumns(fields.FirstOrDefault());
            SetData(fields.Skip(1).ToList());
        }

        public void SetColumns(string[] columns)
        {
            Columns = columns;
            Database.View = CreateColumnsView(columns);
        }

        private GridView CreateColumnsView(string[] columns)
        {
            var gv = new GridView();
            int i = 0;
            foreach (var column in columns)
            {
                if (i > 0)
                {
                    gv.Columns.Add(new GridViewColumn { Header = column, Width = double.NaN, DisplayMemberBinding = new Binding("[" + i + "]") });
                }
                i++;
            }
            return gv;
        }

        public void SetData(List<string[]> data)
        {
            Data = data;
            Database.ItemsSource = null;
            Database.ItemsSource = data;
        }

        #endregion

        #region Drag

        private static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null)
                return null;

            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindVisualParent<T>(parentObject);
        }

        private void Csv_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dragStartPoint = e.GetPosition(null);
        }

        private void Csv_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point point = e.GetPosition(null);
            Vector diff = dragStartPoint - point;
            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                 Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                var listView = sender as ListView;
                var listViewItem = FindVisualParent<ListViewItem>((DependencyObject)e.OriginalSource);
                if (listViewItem != null)
                {
                    string[] data = (string[])listView.ItemContainerGenerator.ItemFromContainer(listViewItem);
                    var tag = new TagItem() { Columns = columns, Data = data };
                    DataObject dragData = new DataObject("Tag", tag);
                    DragDrop.DoDragDrop(listViewItem, dragData, DragDropEffects.Move);
                }
            }
        }

        #endregion
    }
}
