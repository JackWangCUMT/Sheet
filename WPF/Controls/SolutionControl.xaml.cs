using Sheet.Entry;
using Sheet.Controller;
using Sheet.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
    public partial class SolutionControl : UserControl, ISolutionView
    {
        #region IoC

        private ISheetController _sheetController;
        private IEntryController _entryController;

        public SolutionControl(ISheetController sheetController, IEntryController entryController)
        {
            InitializeComponent();

            Init(sheetController, entryController);
        }

        public void Init(ISheetController sheetController, IEntryController entryController)
        {
            this._sheetController = sheetController;
            this._entryController = entryController;
        }

        #endregion

        #region Constructor

        public SolutionControl()
        {
            InitializeComponent();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
        }

        #endregion

        #region Entries Editor

        private void EntryChanged(object oldValue, object newValue)
        {
            if (newValue != null && newValue is DocumentEntry)
            {
                if (_sheetController != null)
                {
                    if (oldValue != null && oldValue is PageEntry)
                    {
                        UpdatePage(oldValue);
                    }

                    EmptyPage();
                }
            }
            else if (newValue != null && newValue is PageEntry)
            {
                if (_sheetController != null)
                {
                    if (oldValue != null && oldValue is PageEntry)
                    {
                        UpdatePage(oldValue);
                    }

                    SetPage(newValue);
                }
            }
            else
            {
                if (_sheetController != null)
                {
                    EmptyPage();
                }
            }
        }

        private void EmptyPage()
        {
            _sheetController.SetPage(null);
        }

        private void SetPage(object newValue)
        {
            var newPage = newValue as PageEntry;
            _sheetController.SetPage(newPage.Content);
        }

        private void UpdatePage(object oldValue)
        {
            var oldPage = oldValue as PageEntry;
            var text = _sheetController.GetPage();
            oldPage.Content = text;
        }

        public void UpdateSelectedPage()
        {
            var item = SolutionTree.SelectedItem;
            if (item != null && item is PageEntry)
            {
                var page = item as PageEntry;
                var text = _sheetController.GetPage();
                page.Content = text;
            }
        }

        #endregion

        #region Export

        public void ExportPage(object item)
        {
            if (_sheetController != null)
            {
                if (item != null && item is PageEntry)
                {
                    var page = item as PageEntry;
                    _sheetController.ExportPage(page.Content);
                }
            }
        }

        public void ExportDocument(object item)
        {
            if (_sheetController != null)
            {
                if (item != null && item is DocumentEntry)
                {
                    var document = item as DocumentEntry;
                    var texts = document.Pages.Select(x => x.Content);
                    _sheetController.ExportPages(texts);
                }
            }
        }

        #endregion

        #region TreeView Events

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            EntryChanged(e.OldValue, e.NewValue);
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

        private void PageInsertBefore_Click(object sender, RoutedEventArgs e)
        {
            _entryController.AddPageBefore(SolutionTree.SelectedItem);
        }

        private void PageInsertAfter_Click(object sender, RoutedEventArgs e)
        {
            _entryController.AddPageAfter(SolutionTree.SelectedItem);
        }

        private void PageDuplicate_Click(object sender, RoutedEventArgs e)
        {
            _entryController.DuplicatePage(SolutionTree.SelectedItem);
        }

        private void PageRemove_Click(object sender, RoutedEventArgs e)
        {
            _entryController.RemovePage(SolutionTree.SelectedItem);
        }

        private void PageExport_Click(object sender, RoutedEventArgs e)
        {
            ExportPage(SolutionTree.SelectedItem);
        }

        #endregion

        #region Document Menu Events

        private void DocumentAddPage_Click(object sender, RoutedEventArgs e)
        {
            _entryController.DocumentAddPage(SolutionTree.SelectedItem);
        }

        private void DocumentInsertBofre_Click(object sender, RoutedEventArgs e)
        {
            _entryController.AddDocumentBefore(SolutionTree.SelectedItem);
        }

        private void DocumentInsertAfter_Click(object sender, RoutedEventArgs e)
        {
            _entryController.AddDocumentAfter(SolutionTree.SelectedItem);
        }

        private void DocumentDuplicate_Click(object sender, RoutedEventArgs e)
        {
            _entryController.DulicateDocument(SolutionTree.SelectedItem);
        }

        private void DocumentRemove_Click(object sender, RoutedEventArgs e)
        {
            _entryController.RemoveDocument(SolutionTree.SelectedItem);
        }

        private void DocumentExport_Click(object sender, RoutedEventArgs e)
        {
            ExportDocument(SolutionTree.SelectedItem);
        }

        #endregion

        #region Tree Menu Events

        private void TreeAddDocument_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext != null && DataContext is SolutionEntry)
            {
                _entryController.AddDocument(DataContext as SolutionEntry);
            }
        }

        #endregion
    }
}
