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

        public IEntryEditor EntryEditor { get; set; }

        #endregion

        #region Constructor

        public SolutionControl()
        {
            InitializeComponent();
        }

        #endregion

        #region Entries

        private void EntryChanged(object oldValue, object newValue)
        {
            if (newValue != null && newValue is DocumentEntry)
            {
                if (EntryEditor != null)
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
                if (EntryEditor != null)
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
                if (EntryEditor != null)
                {
                    EmptyPage();
                }
            }
        }

        private void EmptyPage()
        {
            EntryEditor.Set(null);
        }

        private void SetPage(object newValue)
        {
            var newPage = newValue as PageEntry;
            EntryEditor.Set(newPage.Content);
        }

        private void UpdatePage(object oldValue)
        {
            var oldPage = oldValue as PageEntry;
            var text = EntryEditor.Get();
            oldPage.Content = text;
        }

        public void UpdateSelectedPage()
        {
            var item = SolutionTree.SelectedItem;
            if (item != null && item is PageEntry)
            {
                var page = item as PageEntry;
                var text = EntryEditor.Get();
                page.Content = text;
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
            if (EntryEditor != null)
            {
                var item = SolutionTree.SelectedItem;
                if (item != null && item is PageEntry)
                {
                    var page = item as PageEntry;
                    EntryEditor.Export(page.Content);
                }
            }
        } 

        #endregion

        #region Document Menu Events

        private void DocumentAddPage_Click(object sender, RoutedEventArgs e)
        {
            var document = SolutionTree.SelectedItem as DocumentEntry;
            var page = new PageEntry() 
            { 
                Name = "Page", 
                Content = "", 
                Document = document 
            };
            document.Pages.Add(page);
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
            if (EntryEditor != null)
            {
                var item = SolutionTree.SelectedItem;
                if (item != null && item is DocumentEntry)
                {
                    var document = item as DocumentEntry;
                    var texts = document.Pages.Select(x => x.Content);
                    EntryEditor.Export(texts);
                }
            }
        } 

        #endregion

        #region Tree Menu Events

        private void TreeAddDocument_Click(object sender, RoutedEventArgs e)
        {
            var solution = DataContext as SolutionEntry;
            if (solution != null)
            {
                var document = new DocumentEntry() 
                { 
                    Name = string.Concat("Document", solution.Documents.Count), 
                    Pages = new ObservableCollection<PageEntry>(), 
                    Solution = solution 
                };
                solution.Documents.Add(document);
            }
        }

        #endregion
    }
}
