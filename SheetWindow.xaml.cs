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
    #region SizeBorder

    public class SizeBorder : Border
    {
        #region Properties

        public Action<Size> ExecuteUpdateSize { get; set; }
        public Action ExecuteSizeChanged { get; set; }

        #endregion

        #region Constructor

        public SizeBorder()
        {
            SizeChanged += (sender, e) =>
            {
                if (Child != null && ExecuteSizeChanged != null)
                {
                    ExecuteSizeChanged();
                }
            };
        } 

        #endregion

        #region ArrangeOverride

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (ExecuteUpdateSize != null)
            {
                ExecuteUpdateSize(finalSize);
            }
            return base.ArrangeOverride(finalSize);
        } 

        #endregion
    } 

    #endregion

    public partial class SheetWindow : Window
    {
        #region Fields

        private ObservableCollection<IDatabaseController> DatabaseControllers = new ObservableCollection<IDatabaseController>();

        #endregion

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
            InitSheet();
            InitSizeBorder();
            InitSolution();
            InitDrop();
            UpdateModeMenu();
            InitDatabases();
        }

        private void InitSheet()
        {
            GetSheet().Library = Library;
        }

        private void InitSizeBorder()
        {
            SizeBorder.ExecuteUpdateSize = (size) => GetSheet().SetAutoFitSize(size);
            SizeBorder.ExecuteSizeChanged = () => GetSheet().AutoFit();
        }

        private void InitSolution()
        {
            Solution.Controller = GetSheet();
        }

        private void InitDrop()
        {
            AllowDrop = true;

            PreviewDrop += async (sender, e) =>
            {
                try
                {
                    if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
                    {
                        string[] paths = e.Data.GetData(DataFormats.FileDrop, true) as string[];
                        await Open(paths);
                    }
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                    Debug.Print(ex.StackTrace);
                }
            };
        }

        private void InitDatabases()
        {
            Databases.ItemsSource = DatabaseControllers;

            CreateTestDatabase();
        }

        #endregion

        #region Database

        private void CreateTestDatabase()
        {
            string[] columns = { "Index", "Designation", "Description", "Signal", "Condition" };

            var data = new List<string[]>();
            for (int i = 0; i < 10; i++)
            {
                string[] item = { i.ToString(), "Designation", "Description", "Signal", "Condition" };
                data.Add(item);
            }

            var controller = CreateDatabaseController("Test", columns, data);
            AddDatabaseController(controller);
        }

        public async void OpenDatabase()
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
                    await OpenDatabase(dlg.FileName);
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                    Debug.Print(ex.StackTrace);
                }
            }
        }

        public async Task OpenDatabase(string fileName)
        {
            var reader = new CsvDataReader();
            var fields = await Task.Run(() => reader.Read(fileName));
            var name = System.IO.Path.GetFileName(fileName);

            var controller = CreateDatabaseController(name, fields.FirstOrDefault(), fields.Skip(1).ToList());
            AddDatabaseController(controller);
        }

        private CsvDatabaseController CreateDatabaseController(string name, string[] columns, List<string[]> data)
        {
            var controller = new CsvDatabaseController(name);

            controller.Columns = columns;
            controller.Data = data;

            return controller;
        }

        private void AddDatabaseController(IDatabaseController controller)
        {
            DatabaseControllers.Add(controller);
        }

        #endregion

        #region Open

        private async Task Open(string[] paths)
        {
            var files = paths.Where(x => (System.IO.File.GetAttributes(x) & System.IO.FileAttributes.Directory) != System.IO.FileAttributes.Directory).OrderBy(f => f);
            string path = files.FirstOrDefault();
            string ext = System.IO.Path.GetExtension(path);

            if (string.Compare(ext, FileDialogSettings.SolutionExtension, true) == 0)
            {
                await OpenSolution(path);
            }
            else if (string.Compare(ext, FileDialogSettings.PageExtension, true) == 0)
            {
                await GetSheet().OpenTextFile(path);
            }
            else if (string.Compare(ext, FileDialogSettings.JsonPageExtension, true) == 0)
            {
                await GetSheet().OpenJsonFile(path);
            }
            else if (string.Compare(ext, FileDialogSettings.LibraryExtension, true) == 0)
            {
                await GetSheet().LoadLibrary(path);
            }
            else if (string.Compare(ext, FileDialogSettings.DatabaseExtension, true) == 0)
            {
                await OpenDatabase(path);
            }
        }

        #endregion

        #region Sheet

        private SheetControl GetSheet()
        {
            return Sheet;
        }

        #endregion

        #region Mode Menu

        private void UpdateModeMenu()
        {
            var mode = GetSheet().GetMode();
            ModeNone.IsChecked = mode == SheetMode.None ? true : false;
            ModeSelection.IsChecked = mode == SheetMode.Selection ? true : false;
            ModeInsert.IsChecked = mode == SheetMode.Insert ? true : false;
            ModeLine.IsChecked = mode == SheetMode.Line ? true : false;
            ModeRectangle.IsChecked = mode == SheetMode.Rectangle ? true : false;
            ModeEllipse.IsChecked = mode == SheetMode.Ellipse ? true : false;
            ModeText.IsChecked = mode == SheetMode.Text ? true : false;
            ModeImage.IsChecked = mode == SheetMode.Image ? true : false;
        }

        #endregion

        #region Key Events

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (GetSheet().GetMode() == SheetMode.TextEditor)
            {
                return;
            }

            bool ctrl = (Keyboard.Modifiers & ModifierKeys.Control) > 0;
            bool shift = (Keyboard.Modifiers & ModifierKeys.Shift) > 0;

            switch (e.Key)
            {
                // Ctrl+0: Zoom to Page Level
                case Key.D0:
                case Key.NumPad0:
                    if (ctrl)
                    {
                        GetSheet().AutoFit();
                    }
                    break;
                // Ctrl+1: Actual Size
                case Key.D1:
                case Key.NumPad1:
                    if (ctrl)
                    {
                        GetSheet().ActualSize();
                    }
                    break;
                // Ctrl+O: Open
                // Ctrl+Shift+O: Open Solution
                case Key.O:
                    if (ctrl && !shift)
                    {
                        GetSheet().Open();
                    }
                    else if (ctrl && shift)
                    {
                        OpenSolution();
                    }
                    break;
                // Ctrl+S: Save
                // Ctrl+Shift+S: Save Solution
                // S: Mode Selection
                case Key.S:
                    if (ctrl && !shift)
                    {
                        GetSheet().Save();
                    }
                    else if (ctrl && shift)
                    {
                        SaveSolution();
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
                        if (Solution.DataContext != null)
                        {
                            GetSheet().Export(Solution.DataContext as SolutionEntry);
                        }
                        else
                        {
                            GetSheet().Export();
                        }
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
                        GetSheet().LoadLibrary();
                    }
                    else
                    {
                        GetSheet().ModeLine();
                        UpdateModeMenu();
                    }
                    break;
                // Ctrl+D: Database
                // D: Mode Image
                case Key.D:
                    if (ctrl)
                    {
                        OpenDatabase();
                    }
                    else
                    {
                        GetSheet().ModeImage();
                        UpdateModeMenu();
                    }
                    break;
                // Ctrl+Z: Undo
                case Key.Z:
                    if (ctrl)
                    {
                        GetSheet().History.Undo();
                    }
                    break;
                // Ctrl+Y: Redo
                case Key.Y:
                    if (ctrl)
                    {
                        GetSheet().History.Redo();
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
                // Ctrl+Shift+C: Close Solution
                case Key.C:
                    if (ctrl && !shift)
                    {
                        GetSheet().Copy();
                    }
                    else if (ctrl && shift)
                    {
                        CloseSolution();
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
                        GetSheet().History.Register("Reset");
                        GetSheet().ResetPage();
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
                // Ctrl+Shift+N: New Solution
                // N: Mode None
                case Key.N:
                    if (ctrl && shift)
                    {
                        NewSolution();
                    }
                    else
                    {
                        GetSheet().ModeNone();
                        UpdateModeMenu();
                    }
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
                // Q: Invert Line Start
                case Key.Q:
                    GetSheet().InvertSelectedLineStart();
                    break;
                // W: Invert Line End
                case Key.W:
                    GetSheet().InvertSelectedLineEnd();
                    break;
            }
        }

        #endregion
        
        #region Solution

        public string SolutionPath { get; set; }

        public async void NewSolution()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = FileDialogSettings.SolutionFilter,
                FileName = "solution"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    GetSheet().ResetPage();

                    await NewSolution(dlg.FileName);
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                    Debug.Print(ex.StackTrace);
                }
            }
        }

        private async Task NewSolution(string path)
        {
            await Task.Run(() => EntryFactory.CreateEmpty(path));
            var solution = await Task.Run(() => EntrySerializer.Deserialize(path));

            if (solution != null)
            {
                Solution.DataContext = null;
                Solution.DataContext = solution;

                SolutionPath = path;
            }
        }

        public async void OpenSolution()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = FileDialogSettings.SolutionFilter
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    GetSheet().ResetPage();

                    await OpenSolution(dlg.FileName);
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                    Debug.Print(ex.StackTrace);
                }
            }
        }

        private async Task OpenSolution(string path)
        {
            var solution = await Task.Run(() => EntrySerializer.Deserialize(path));

            if (solution != null)
            {
                Solution.DataContext = null;
                Solution.DataContext = solution;

                SolutionPath = path;
            }
        }

        public void SaveSolution()
        {
            if (SolutionPath != null)
            {
                try
                {
                    var path = SolutionPath;
                    var solution = Solution.DataContext as SolutionEntry;

                    if (solution != null)
                    {
                        Solution.UpdateSelectedPage();
                        EntrySerializer.Serialize(solution, path);
                    }
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                    Debug.Print(ex.StackTrace);
                }
            }
        }

        private void CloseSolution()
        {
            SolutionPath = null;
            Solution.DataContext = null;

            GetSheet().ResetPage();
        }

        #endregion

        #region File Menu Events

        private void FileNewSolution_Click(object sender, RoutedEventArgs e)
        {
            NewSolution();
        }

        private void FileOpenSolution_Click(object sender, RoutedEventArgs e)
        {
            OpenSolution();
        }

        private void FileSaveSolution_Click(object sender, RoutedEventArgs e)
        {
            SaveSolution();
        }

        private void FileCloseSolution_Click(object sender, RoutedEventArgs e)
        {
            CloseSolution();
        }

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
            if (Solution.DataContext != null)
            {
                GetSheet().Export(Solution.DataContext as SolutionEntry);
            }
            else
            {
                GetSheet().Export();
            }
        }

        private void FileLibrary_Click(object sender, RoutedEventArgs e)
        {
            GetSheet().LoadLibrary();
        }

        private void FileDatabase_Click(object sender, RoutedEventArgs e)
        {
            OpenDatabase();
        }

        private void FileExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #region Edit Menu Events

        private void EditUndo_Click(object sender, RoutedEventArgs e)
        {
            GetSheet().History.Undo();
        }

        private void EditRedo_Click(object sender, RoutedEventArgs e)
        {
            GetSheet().History.Redo();
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
            GetSheet().History.Register("Reset");
            GetSheet().ResetPage();
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

        #region View Menu Events

        private void ViewZoomToPageLevel_Click(object sender, RoutedEventArgs e)
        {
            GetSheet().AutoFit();
        }

        private void ViewZoomActualSize_Click(object sender, RoutedEventArgs e)
        {
            GetSheet().ActualSize();
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
        
        private void ModeImage_Click(object sender, RoutedEventArgs e)
        {
            GetSheet().ModeImage();
            UpdateModeMenu();
        }

        #endregion

        #region Logic Menu Events

        private void LogicInvertLineStart_Click(object sender, RoutedEventArgs e)
        {
            GetSheet().InvertSelectedLineStart();
        }

        private void LogicInvertLineEnd_Click(object sender, RoutedEventArgs e)
        {
            GetSheet().InvertSelectedLineEnd();
        } 

        #endregion
    }
}
