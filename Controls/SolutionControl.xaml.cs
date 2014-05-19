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
    public partial class SolutionControl : UserControl
    {
        #region Properties

        public IPageController Controller { get; set; }

        #endregion

        #region Constructor

        public SolutionControl()
        {
            InitializeComponent();
        }

        #endregion

        #region Entries Editor

        private void EntryChanged(object oldValue, object newValue)
        {
            if (newValue != null && newValue is DocumentEntry)
            {
                if (Controller != null)
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
                if (Controller != null)
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
                if (Controller != null)
                {
                    EmptyPage();
                }
            }
        }

        private void EmptyPage()
        {
            Controller.SetPage(null);
        }

        private void SetPage(object newValue)
        {
            var newPage = newValue as PageEntry;
            Controller.SetPage(newPage.Content);
        }

        private void UpdatePage(object oldValue)
        {
            var oldPage = oldValue as PageEntry;
            var text = Controller.GetPage();
            oldPage.Content = text;
        }

        public void UpdateSelectedPage()
        {
            var item = SolutionTree.SelectedItem;
            if (item != null && item is PageEntry)
            {
                var page = item as PageEntry;
                var text = Controller.GetPage();
                page.Content = text;
            }
        }

        #endregion

        #region Export

        public void ExportPage(object item)
        {
            if (Controller != null)
            {
                if (item != null && item is PageEntry)
                {
                    var page = item as PageEntry;
                    Controller.ExportPage(page.Content);
                }
            }
        }

        public void ExportDocument(object item)
        {
            if (Controller != null)
            {
                if (item != null && item is DocumentEntry)
                {
                    var document = item as DocumentEntry;
                    var texts = document.Pages.Select(x => x.Content);
                    Controller.ExportPages(texts);
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
            EntryController.AddPageBefore(SolutionTree.SelectedItem);
        }

        private void PageInsertAfter_Click(object sender, RoutedEventArgs e)
        {
            EntryController.AddPageAfter(SolutionTree.SelectedItem);
        }

        private void PageDuplicate_Click(object sender, RoutedEventArgs e)
        {
            EntryController.DuplicatePage(SolutionTree.SelectedItem);
        }

        private void PageRemove_Click(object sender, RoutedEventArgs e)
        {
            EntryController.RemovePage(SolutionTree.SelectedItem);
        }

        private void PageExport_Click(object sender, RoutedEventArgs e)
        {
            ExportPage(SolutionTree.SelectedItem);
        }

        #endregion

        #region Document Menu Events

        private void DocumentAddPage_Click(object sender, RoutedEventArgs e)
        {
            EntryController.DocumentAddPage(SolutionTree.SelectedItem);
        }

        private void DocumentInsertBofre_Click(object sender, RoutedEventArgs e)
        {
            EntryController.AddDocumentBefore(SolutionTree.SelectedItem);
        }

        private void DocumentInsertAfter_Click(object sender, RoutedEventArgs e)
        {
            EntryController.AddDocumentAfter(SolutionTree.SelectedItem);
        }

        private void DocumentDuplicate_Click(object sender, RoutedEventArgs e)
        {
            EntryController.DulicateDocument(SolutionTree.SelectedItem);
        }

        private void DocumentRemove_Click(object sender, RoutedEventArgs e)
        {
            EntryController.RemoveDocument(SolutionTree.SelectedItem);
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
                EntryController.AddDocument(DataContext as SolutionEntry);
            }
        }

        #endregion
    }
}
