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
            Init();
        }

        #endregion

        #region Init

        private void Init()
        {
            Sheet.Library = Library;
            Sheet.Database = Csv;
            Sheet.TextEditor = Text;
            UpdateModeMenu();
            CreateTestDatabase();
        }

        #endregion

        #region Sheet

        private SheetControl GetSheet()
        {
            return Sheet;
        }

        #endregion

        #region Database

        public void Database()
        {
            Csv.Open();
        }

        private void CreateTestDatabase()
        {
            string[] columns = { "Index", "Designation", "Description", "Signal", "Condition" };

            var data = new List<string[]>();
            for (int i = 0; i < 10; i++)
            {
                string[] item = { i.ToString(), "Designation", "Description", "Signal", "Condition" };
                data.Add(item);
            }

            Csv.SetColumns(columns);
            Csv.SetData(data);
        }

        #endregion

        #region Toggle Panels

        private void ToggleHelpPanel()
        {
            Help.Visibility = Help.Visibility == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;
        }

        private void ToggleBlocksPanel()
        {
            if (Library.Blocks.Items.Count > 0)
            {
                Library.Visibility = Library.Visibility == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;
            }
            else
            {
                Library.Visibility = Visibility.Hidden;
            }
        }

        #endregion

        #region Mode Menu

        private void UpdateModeMenu()
        {
            var mode = GetSheet().GetMode();
            ModeNone.IsChecked = mode == SheetControl.Mode.None ? true : false;
            ModeSelection.IsChecked = mode == SheetControl.Mode.Selection ? true : false;
            ModeInsert.IsChecked = mode == SheetControl.Mode.Insert ? true : false;
            ModeLine.IsChecked = mode == SheetControl.Mode.Line ? true : false;
            ModeRectangle.IsChecked = mode == SheetControl.Mode.Rectangle ? true : false;
            ModeEllipse.IsChecked = mode == SheetControl.Mode.Ellipse ? true : false;
            ModeText.IsChecked = mode == SheetControl.Mode.Text ? true : false;
        }

        #endregion

        #region Key Events

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Text.Visibility == Visibility.Visible)
            {
                return;
            }

            bool ctrl = (Keyboard.Modifiers & ModifierKeys.Control) > 0;

            switch (e.Key)
            {
                // Ctrl+O: Open
                case Key.O:
                    if (ctrl)
                    {
                        GetSheet().Open();
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
                        UpdateModeMenu();
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
                        UpdateModeMenu();
                    }
                    break;
                // Ctrl+L: Library
                // L: Mode Line
                case Key.L:
                    if (ctrl)
                    {
                        GetSheet().Load();
                    }
                    else
                    {
                        GetSheet().ModeLine();
                        UpdateModeMenu();
                    }
                    break;
                // Ctrl+D: Database
                case Key.D:
                    if (ctrl)
                    {
                        Database();
                    }
                    break;
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
                            e.Handled = true;
                            GetSheet().CreateBlock();
                        }
                    }
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
                // F: Toggle Fill
                case Key.F:
                    GetSheet().ToggleFill();
                    break;
                // N: Mode None
                case Key.N:
                    GetSheet().ModeNone();
                    UpdateModeMenu();
                    break;
                // I: Mode Insert
                case Key.I:
                    GetSheet().ModeInsert();
                    UpdateModeMenu();
                    break;
                // R: Mode Rectangle
                case Key.R:
                    GetSheet().ModeRectangle();
                    UpdateModeMenu();
                    break;
                // T: Mode Text
                case Key.T:
                    GetSheet().ModeText();
                    UpdateModeMenu();
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

        #region File Menu Events

        private void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            GetSheet().Open();
        }

        private void FileSave_Click(object sender, RoutedEventArgs e)
        {
            GetSheet().Save();
        }

        private void FileExport_Click(object sender, RoutedEventArgs e)
        {
            GetSheet().Export();
        }

        private void FileLibrary_Click(object sender, RoutedEventArgs e)
        {
            GetSheet().Load();
        }

        private void FileDatabase_Click(object sender, RoutedEventArgs e)
        {
            Database();
        }

        private void FileExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #region Edit Menu Events

        private void EditUndo_Click(object sender, RoutedEventArgs e)
        {
            GetSheet().Undo();
        }

        private void EditRedo_Click(object sender, RoutedEventArgs e)
        {
            GetSheet().Redo();
        }

        private void EditCut_Click(object sender, RoutedEventArgs e)
        {
            GetSheet().Cut();
        }

        private void EditCopy_Click(object sender, RoutedEventArgs e)
        {
            GetSheet().Copy();
        }

        private void EditPaste_Click(object sender, RoutedEventArgs e)
        {
            GetSheet().Paste();
        }

        private void EditDelete_Click(object sender, RoutedEventArgs e)
        {
            GetSheet().Delete();
        }

        private void EditReset_Click(object sender, RoutedEventArgs e)
        {
            GetSheet().PushUndo("Reset");
            GetSheet().Reset();
        }

        private void EditSelectAll_Click(object sender, RoutedEventArgs e)
        {
            GetSheet().SelecteAll();
        }

        private void EditCreateBlock_Click(object sender, RoutedEventArgs e)
        {
            GetSheet().CreateBlock();
        }

        private void EditBreakBlock_Click(object sender, RoutedEventArgs e)
        {
            GetSheet().BreakBlock();
        }

        private void EditMoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (GetSheet().IsFocused)
            {
                GetSheet().MoveUp();
            }
        }

        private void EditMoveDown_Click(object sender, RoutedEventArgs e)
        {
            if (GetSheet().IsFocused)
            {
                GetSheet().MoveDown();
            }
        }

        private void EditMoveLeft_Click(object sender, RoutedEventArgs e)
        {
            if (GetSheet().IsFocused)
            {
                GetSheet().MoveLeft();
            }
        }

        private void EditMoveRight_Click(object sender, RoutedEventArgs e)
        {
            if (GetSheet().IsFocused)
            {
                GetSheet().MoveRight();
            }
        }

        private void EditToggleFill_Click(object sender, RoutedEventArgs e)
        {
            GetSheet().ToggleFill();
        }

        #endregion

        #region Mode Menu Events

        private void ModeNone_Click(object sender, RoutedEventArgs e)
        {
            GetSheet().ModeNone();
            UpdateModeMenu();
        }

        private void ModeSelection_Click(object sender, RoutedEventArgs e)
        {
            GetSheet().ModeSelection();
            UpdateModeMenu();
        }

        private void ModeInsert_Click(object sender, RoutedEventArgs e)
        {
            GetSheet().ModeInsert();
            UpdateModeMenu();
        }

        private void ModeLine_Click(object sender, RoutedEventArgs e)
        {
            GetSheet().ModeLine();
            UpdateModeMenu();
        }

        private void ModeRectangle_Click(object sender, RoutedEventArgs e)
        {
            GetSheet().ModeRectangle();
            UpdateModeMenu();
        }

        private void ModeEllipse_Click(object sender, RoutedEventArgs e)
        {
            GetSheet().ModeEllipse();
            UpdateModeMenu();
        }

        private void ModeText_Click(object sender, RoutedEventArgs e)
        {
            GetSheet().ModeText();
            UpdateModeMenu();
        }

        #endregion

        #region View Menu Events

        private void ViewHelpPanel_Click(object sender, RoutedEventArgs e)
        {
            ToggleHelpPanel();
        }

        private void ViewBlocksPanel_Click(object sender, RoutedEventArgs e)
        {
            ToggleBlocksPanel();
        }

        #endregion
    }
}
