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

        #region Sheet

        private SheetControl GetSheet()
        {
            return Sheet;
        }

        #endregion

        #region Events

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool ctrl = (Keyboard.Modifiers & ModifierKeys.Control) > 0;

            switch (e.Key)
            {
                // Ctrl+Z: Undo
                case Key.Z:
                    if (ctrl)
                    {
                        GetSheet().Undo();
                    }
                    break;
                // Ctrl+Y: Redo
                case Key.Y:
                    if (ctrl)
                    {
                        GetSheet().Redo();
                    }
                    break;
                // Ctrl+X: Cut
                case Key.X:
                    if (ctrl)
                    {
                        GetSheet().Cut();
                    }
                    break;
                // Ctrl+C: Copy
                case Key.C:
                    if (ctrl)
                    {
                        GetSheet().Copy();
                    }
                    break;
                // Ctrl+V: Paste
                case Key.V:
                    if (ctrl)
                    {
                        GetSheet().Paste();
                    }
                    break;
                // Del: Delete
                // Ctrl+Del: Reset
                case Key.Delete:
                    if (ctrl)
                    {
                        GetSheet().PushUndo("Reset");
                        GetSheet().Reset();
                    }
                    else
                    {
                        GetSheet().Delete();
                    }
                    break;
                // Ctrl+A: Select All
                case Key.A:
                    if (ctrl)
                    {
                        GetSheet().SelecteAll();
                    }
                    break;
                // B: Create Block
                // Ctrl+B: Break Block
                case Key.B:
                    {
                        if (ctrl)
                        {
                            GetSheet().BreakBlock();
                        }
                        else
                        {
                            GetSheet().CreateBlock();
                        }
                    }
                    break;
                // Ctrl+E: Export
                // E: Mode Ellipse
                case Key.E:
                    if (ctrl)
                    {
                        GetSheet().Export();
                    }
                    else
                    {
                        GetSheet().ModeEllipse();
                    }
                    break;
                // Ctrl+S: Save
                // S: Mode Selection
                case Key.S:
                    if (ctrl)
                    {
                        GetSheet().Save();
                    }
                    else
                    {
                        GetSheet().ModeSelection();
                    }
                    break;
                // Ctrl+O: Open
                case Key.O:
                    if (ctrl)
                    {
                        GetSheet().Open();
                    }
                    break;
                // Ctrl+L: Library
                // L: Mode Line
                case Key.L:
                    if (ctrl)
                    {
                        GetSheet().Library();
                    }
                    else
                    {
                        GetSheet().ModeLine();
                    }
                    break;
                // R: Mode Rectangle
                case Key.R:
                    GetSheet().ModeRectangle();
                    break;
                // T: Mode Text
                case Key.T:
                    GetSheet().ModeText();
                    break;
                // I: Mode Signal
                case Key.I:
                    GetSheet().ModeInsert();
                    break;
                // N: Mode None
                case Key.N:
                    GetSheet().ModeNone();
                    break;
                // F: Toggle Fill
                case Key.F:
                    GetSheet().ToggleFill();
                    break;
                // Up: Move Up
                case Key.Up:
                    {
                        if (GetSheet().IsFocused)
                        {
                            GetSheet().MoveUp();
                            e.Handled = true;
                        }
                    }
                    break;
                // Down: Move Down
                case Key.Down:
                    {
                        if (GetSheet().IsFocused)
                        {
                            GetSheet().MoveDown();
                            e.Handled = true;
                        }
                    }
                    break;
                // Left: Move Left
                case Key.Left:
                    {
                        if (GetSheet().IsFocused)
                        {
                            GetSheet().MoveLeft();
                            e.Handled = true;
                        }
                    }
                    break;
                // Right: Move Right
                case Key.Right:
                    {
                        if (GetSheet().IsFocused)
                        {
                            GetSheet().MoveRight();
                            e.Handled = true;
                        }
                    }
                    break;
                // Help Panel
                case Key.H:
                    ToggleHelpPanel();
                    break;
                // Blocks Panel
                case Key.J:
                    ToggleBlocksPanel();
                    break;
            }
        }

        #endregion

        #region Panels

        private void ToggleHelpPanel()
        {
            Help.Visibility = Help.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ToggleBlocksPanel()
        {
            Library.Visibility = Library.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion
    }
}
