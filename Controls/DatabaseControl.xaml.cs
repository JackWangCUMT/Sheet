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
    public partial class DatabaseControl : UserControl, IDatabaseController
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

        #region IDatabaseController

        public string[] Get(int index)
        {
            return data.Where(x => int.Parse(x[0]) == index).FirstOrDefault();
        }

        public bool Update(int index, string[] item)
        {
            for (int i = 0; i < data.Count; i++)
            {
                if (int.Parse(data[i][0]) == index)
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
                Filter = FileDialogSettings.DatabaseFilter
            };

            var result = dlg.ShowDialog();
            if (result.HasValue && result.Value == true)
            {
                try
                {
                    Open(dlg.FileName);
                    return System.IO.Path.GetFileName(dlg.FileName);
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                    Debug.Print(ex.StackTrace);
                }
            }
            return null;
        }

        public void Open(string fileName)
        {
            var reader = new CsvDataReader();
            var fields = reader.Read(fileName);
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
                gv.Columns.Add(new GridViewColumn { Header = column, Width = double.NaN, DisplayMemberBinding = new Binding("[" + i + "]") });
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
                var listViewItem = WpfHelper.FindVisualParent<ListViewItem>((DependencyObject)e.OriginalSource);
                if (listViewItem != null)
                {
                    string[] data = (string[])listView.ItemContainerGenerator.ItemFromContainer(listViewItem);
                    var dataItem = new DataItem() { Columns = columns, Data = data };
                    DataObject dragData = new DataObject("Data", dataItem);
                    DragDrop.DoDragDrop(listViewItem, dragData, DragDropEffects.Move);
                }
            }
        }

        #endregion
    }
}
