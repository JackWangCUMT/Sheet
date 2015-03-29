﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Sheet.Item;
using Sheet.Controller;
using Sheet.UI;
using Sheet.WPF;
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
    public partial class LibraryControl : UserControl, ILibraryView, ILibraryController
    {
        #region Fields

        private Point dragStartPoint;

        #endregion

        #region Constructor

        public LibraryControl()
        {
            InitializeComponent();
        } 

        #endregion

        #region IDisposable

        public void Dispose()
        {
        }

        #endregion

        #region ILibraryController

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
                Dispatcher.Invoke(() =>
                {
                    Blocks.ItemsSource = null;
                    Blocks.ItemsSource = source;
                    Blocks.SelectedIndex = 0;

                    if (source.Count() == 0)
                    {
                        Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        Visibility = Visibility.Visible;
                    }
                });
            }
        } 

        #endregion

        #region Drag

        private void Blocks_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dragStartPoint = e.GetPosition(null);
        }

        private void Blocks_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point point = e.GetPosition(null);
            Vector diff = dragStartPoint - point;
            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                 Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                var listBox = sender as ListBox;
                var listBoxItem = WpfVisualHelper.FindVisualParent<ListBoxItem>((DependencyObject)e.OriginalSource);
                if (listBoxItem != null)
                {
                    BlockItem block = (BlockItem)listBox.ItemContainerGenerator.ItemFromContainer(listBoxItem);
                    DataObject dragData = new DataObject("Block", block);
                    DragDrop.DoDragDrop(listBoxItem, dragData, DragDropEffects.Move);
                }
            }
        } 

        #endregion
    }
}
