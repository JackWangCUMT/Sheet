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
        #region IoC

        private readonly IServiceLocator _serviceLocator;
        private readonly IEntryController _entryController;
        private readonly IEntryFactory _entryFactory;
        private readonly IEntrySerializer _entrySerializer;

        public SheetWindow(IServiceLocator serviceLocator)
        {
            InitializeComponent();

            this._serviceLocator = serviceLocator;
            this._entryController = serviceLocator.GetInstance<IEntryController>();
            this._entryFactory = serviceLocator.GetInstance<IEntryFactory>();
            this._entrySerializer = serviceLocator.GetInstance<IEntrySerializer>();

            InitSheet(serviceLocator);

            Init();
        }

        #endregion

        #region Fields

        private ObservableCollection<IDatabaseController> DatabaseControllers = new ObservableCollection<IDatabaseController>();

        #endregion

        #region Init

        private void InitSheet(IServiceLocator serviceLocator)
        {
            Sheet = new SheetControl(serviceLocator)
            {
                Background = Brushes.Transparent,
                ClipToBounds = false,
                ZoomIndex = 9,
                PanX = 0,
                PanY = 0,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            SheetContent.Content = Sheet;

            GetSheetController().LibraryController = Library;
        } 

        private void Init()
        {
            InitSizeBorder();
            InitSolution();
            InitDrop();
            UpdateModeMenu();
            InitDatabases();
        }

        private void InitSizeBorder()
        {
            SizeBorder.ExecuteUpdateSize = (size) => GetSheetController().SetAutoFitSize(size.Width, size.Height);
            SizeBorder.ExecuteSizeChanged = () => GetSheetController().ZoomController.AutoFit();
        }

        private void InitSolution()
        {
            Solution.Init(GetSheetController(), _entryController);
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
            Databases.Tabs.ItemsSource = DatabaseControllers;

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
                await GetSheetController().OpenTextPage(path);
            }
            else if (string.Compare(ext, FileDialogSettings.JsonPageExtension, true) == 0)
            {
                await GetSheetController().OpenJsonPage(path);
            }
            else if (string.Compare(ext, FileDialogSettings.LibraryExtension, true) == 0)
            {
                await GetSheetController().LoadLibrary(path);
            }
            else if (string.Compare(ext, FileDialogSettings.DatabaseExtension, true) == 0)
            {
                await OpenDatabase(path);
            }
        }

        #endregion

        #region Sheet Controller

        private SheetControl Sheet;

        private ISheetController GetSheetController()
        {
            return Sheet.SheetController;
        }

        #endregion

        #region Mode Menu

        private void UpdateModeMenu()
        {
            var mode = GetSheetController().GetMode();
            ModeNone.IsChecked = mode == SheetMode.None ? true : false;
            ModeSelection.IsChecked = mode == SheetMode.Selection ? true : false;
            ModeInsert.IsChecked = mode == SheetMode.Insert ? true : false;
            ModePoint.IsChecked = mode == SheetMode.Point ? true : false;
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
            if (GetSheetController().GetMode() == SheetMode.TextEditor)
            {
                return;
            }

            bool none = Keyboard.Modifiers == ModifierKeys.None;
            bool onlyCtrl = Keyboard.Modifiers == ModifierKeys.Control;
            bool onlyShift = Keyboard.Modifiers == ModifierKeys.Shift;
            bool ctrlShift = Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift);

            switch (e.Key)
            {
                // Ctrl+0: Zoom to Page Level
                case Key.D0:
                case Key.NumPad0:
                    if (onlyCtrl)
                    {
                        GetSheetController().ZoomController.AutoFit();
                    }
                    break;
                // Ctrl+1: Actual Size
                case Key.D1:
                case Key.NumPad1:
                    if (onlyCtrl)
                    {
                        GetSheetController().ZoomController.ActualSize();
                    }
                    break;
                // N: Mode None
                // Ctrl+N: New Page
                // Ctrl+Shift+N: New Solution
                case Key.N:
                    if (none)
                    {
                        GetSheetController().SetMode(SheetMode.None);
                        UpdateModeMenu();
                    }
                    if (onlyCtrl)
                    {
                        GetSheetController().NewPage();
                    }
                    else if (ctrlShift)
                    {
                        NewSolution();
                    }
                    break;
                // Ctrl+O: Open Page
                // Ctrl+Shift+O: Open Solution
                case Key.O:
                    if (onlyCtrl)
                    {
                        GetSheetController().OpenPage();
                    }
                    else if (ctrlShift)
                    {
                        OpenSolution();
                    }
                    break;
                // Ctrl+S: Save Page
                // Ctrl+Shift+S: Save Solution
                // S: Mode Selection
                case Key.S:
                    if (onlyCtrl)
                    {
                        GetSheetController().SavePage();
                    }
                    else if (ctrlShift)
                    {
                        SaveSolution();
                    }
                    else if (none)
                    {
                        GetSheetController().SetMode(SheetMode.Selection);
                        UpdateModeMenu();
                    }
                    break;
                // Ctrl+E: Export
                // E: Mode Ellipse
                case Key.E:
                    if (onlyCtrl)
                    {
                        if (Solution.DataContext != null)
                        {
                            GetSheetController().Export(Solution.DataContext as SolutionEntry);
                        }
                        else
                        {
                            GetSheetController().ExportPage();
                        }
                    }
                    else if (none)
                    {
                        GetSheetController().SetMode(SheetMode.Ellipse);
                        UpdateModeMenu();
                    }
                    break;
                // Ctrl+L: Library
                // L: Mode Line
                case Key.L:
                    if (onlyCtrl)
                    {
                        GetSheetController().LoadLibrary();
                    }
                    else if (none)
                    {
                        GetSheetController().SetMode(SheetMode.Line);
                        UpdateModeMenu();
                    }
                    break;
                // Ctrl+D: Database
                // D: Mode Image
                case Key.D:
                    if (onlyCtrl)
                    {
                        OpenDatabase();
                    }
                    else if (none)
                    {
                        GetSheetController().SetMode(SheetMode.Image);
                        UpdateModeMenu();
                    }
                    break;
                // Ctrl+Z: Undo
                case Key.Z:
                    if (onlyCtrl)
                    {
                        GetSheetController().HistoryController.Undo();
                    }
                    break;
                // Ctrl+Y: Redo
                case Key.Y:
                    if (onlyCtrl)
                    {
                        GetSheetController().HistoryController.Redo();
                    }
                    break;
                // Ctrl+X: Cut
                // Ctrl+Shift+X: Cut as Json
                case Key.X:
                    if (onlyCtrl)
                    {
                        GetSheetController().CutAsText();
                    }
                    else if (ctrlShift)
                    {
                        GetSheetController().CutAsJson();
                    }
                    break;
                // Ctrl+C: Copy
                // Ctrl+Shift+C: Copy as Json
                case Key.C:
                    if (onlyCtrl)
                    {
                        GetSheetController().CopyAsText();
                    }
                    else if (ctrlShift)
                    {
                        GetSheetController().CopyAsJson();
                    }
                    break;
                // Ctrl+V: Paste
                // Ctrl+Shift+V: Paste as Json
                case Key.V:
                    if (onlyCtrl)
                    {
                        GetSheetController().PasteText();
                    }
                    break;
                // Del: Delete
                // Ctrl+Del: Reset
                case Key.Delete:
                    if (onlyCtrl)
                    {
                        GetSheetController().HistoryController.Register("Reset");
                        GetSheetController().ResetPage();
                    }
                    else if (none)
                    {
                        GetSheetController().Delete();
                    }
                    break;
                // Ctrl+A: Select All
                case Key.A:
                    if (onlyCtrl)
                    {
                        GetSheetController().SelecteAll();
                    }
                    break;
                // B: Create Block
                // Ctrl+B: Break Block
                case Key.B:
                    if (onlyCtrl)
                    {
                        GetSheetController().BreakBlock();
                    }
                    else if (none)
                    {
                        e.Handled = true;
                        GetSheetController().CreateBlock();
                    }
                    break;
                // Up: Move Up
                case Key.Up:
                    if (none && GetSheetController().IsSheetFocused())
                    {
                        GetSheetController().MoveUp();
                        e.Handled = true;
                    }
                    break;
                // Down: Move Down
                case Key.Down:
                    if (none && GetSheetController().IsSheetFocused())
                    {
                        GetSheetController().MoveDown();
                        e.Handled = true;
                    }
                    break;
                // Left: Move Left
                case Key.Left:
                    if (none && GetSheetController().IsSheetFocused())
                    {
                        GetSheetController().MoveLeft();
                        e.Handled = true;
                    }
                    break;
                // Right: Move Right
                case Key.Right:
                    if (none && GetSheetController().IsSheetFocused())
                    {
                        GetSheetController().MoveRight();
                        e.Handled = true;
                    }
                    break;
                // F: Toggle Fill
                case Key.F:
                    if (none)
                    {
                        GetSheetController().ToggleFill();
                    }
                    break;
                // I: Mode Insert
                case Key.I:
                    if (none)
                    {
                        GetSheetController().SetMode(SheetMode.Insert);
                        UpdateModeMenu(); 
                    }
                    break;
                // P: Mode Point
                case Key.P:
                    if (none)
                    {
                        GetSheetController().SetMode(SheetMode.Point);
                        UpdateModeMenu(); 
                    }
                    break;
                // R: Mode Rectangle
                case Key.R:
                    if (none)
                    {
                        GetSheetController().SetMode(SheetMode.Rectangle);
                        UpdateModeMenu(); 
                    }
                    break;
                // T: Mode Text
                case Key.T:
                    if (none)
                    {
                        GetSheetController().SetMode(SheetMode.Text);
                        UpdateModeMenu(); 
                    }
                    break;
                // Q: Invert Line Start
                case Key.Q:
                    if (none)
                    {
                        GetSheetController().InvertSelectedLineStart(); 
                    }
                    break;
                // W: Invert Line End
                case Key.W:
                    if (none)
                    {
                        GetSheetController().InvertSelectedLineEnd(); 
                    }
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
                    GetSheetController().ResetPage();

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
            await Task.Run(() => _entrySerializer.CreateEmpty(path));
            var solution = await Task.Run(() => _entrySerializer.Deserialize(path));

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
                    GetSheetController().ResetPage();

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
            var solution = await Task.Run(() => _entrySerializer.Deserialize(path));

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
                        _entrySerializer.Serialize(solution, path);
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
            if (SolutionPath != null)
            {
                SolutionPath = null;
                Solution.DataContext = null;

                GetSheetController().ResetPage();
            }
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

        private void FileNewPage_Click(object sender, RoutedEventArgs e)
        {
            GetSheetController().NewPage();
        }

        private void FileOpenPage_Click(object sender, RoutedEventArgs e)
        {
            GetSheetController().OpenPage();
        }

        private void FileSavePage_Click(object sender, RoutedEventArgs e)
        {
            GetSheetController().SavePage();
        }

        private void FileExport_Click(object sender, RoutedEventArgs e)
        {
            if (Solution.DataContext != null)
            {
                GetSheetController().Export(Solution.DataContext as SolutionEntry);
            }
            else
            {
                GetSheetController().ExportPage();
            }
        }

        private void FileLibrary_Click(object sender, RoutedEventArgs e)
        {
            GetSheetController().LoadLibrary();
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
            GetSheetController().HistoryController.Undo();
        }

        private void EditRedo_Click(object sender, RoutedEventArgs e)
        {
            GetSheetController().HistoryController.Redo();
        }

        private void EditCut_Click(object sender, RoutedEventArgs e)
        {
            GetSheetController().CutAsText();
        }

        private void EditCopy_Click(object sender, RoutedEventArgs e)
        {
            GetSheetController().CopyAsText();
        }

        private void EditPaste_Click(object sender, RoutedEventArgs e)
        {
            GetSheetController().PasteText();
        }

        private void EditDelete_Click(object sender, RoutedEventArgs e)
        {
            GetSheetController().Delete();
        }

        private void EditReset_Click(object sender, RoutedEventArgs e)
        {
            GetSheetController().HistoryController.Register("Reset");
            GetSheetController().ResetPage();
        }

        private void EditSelectAll_Click(object sender, RoutedEventArgs e)
        {
            GetSheetController().SelecteAll();
        }

        private void EditCreateBlock_Click(object sender, RoutedEventArgs e)
        {
            GetSheetController().CreateBlock();
        }

        private void EditBreakBlock_Click(object sender, RoutedEventArgs e)
        {
            GetSheetController().BreakBlock();
        }

        private void EditMoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (GetSheetController().IsSheetFocused())
            {
                GetSheetController().MoveUp();
            }
        }

        private void EditMoveDown_Click(object sender, RoutedEventArgs e)
        {
            if (GetSheetController().IsSheetFocused())
            {
                GetSheetController().MoveDown();
            }
        }

        private void EditMoveLeft_Click(object sender, RoutedEventArgs e)
        {
            if (GetSheetController().IsSheetFocused())
            {
                GetSheetController().MoveLeft();
            }
        }

        private void EditMoveRight_Click(object sender, RoutedEventArgs e)
        {
            if (GetSheetController().IsSheetFocused())
            {
                GetSheetController().MoveRight();
            }
        }

        private void EditToggleFill_Click(object sender, RoutedEventArgs e)
        {
            GetSheetController().ToggleFill();
        }

        #endregion

        #region View Menu Events

        private void ViewZoomToPageLevel_Click(object sender, RoutedEventArgs e)
        {
            GetSheetController().ZoomController.AutoFit();
        }

        private void ViewZoomActualSize_Click(object sender, RoutedEventArgs e)
        {
            GetSheetController().ZoomController.ActualSize();
        }

        #endregion

        #region Mode Menu Events

        private void ModeNone_Click(object sender, RoutedEventArgs e)
        {
            GetSheetController().SetMode(SheetMode.None);
            UpdateModeMenu();
        }

        private void ModeSelection_Click(object sender, RoutedEventArgs e)
        {
            GetSheetController().SetMode(SheetMode.Selection);
            UpdateModeMenu();
        }

        private void ModeInsert_Click(object sender, RoutedEventArgs e)
        {
            GetSheetController().SetMode(SheetMode.Insert);
            UpdateModeMenu();
        }

        private void ModePoint_Click(object sender, RoutedEventArgs e)
        {
            GetSheetController().SetMode(SheetMode.Point);
            UpdateModeMenu();
        }
        
        private void ModeLine_Click(object sender, RoutedEventArgs e)
        {
            GetSheetController().SetMode(SheetMode.Line);
            UpdateModeMenu();
        }

        private void ModeRectangle_Click(object sender, RoutedEventArgs e)
        {
            GetSheetController().SetMode(SheetMode.Rectangle);
            UpdateModeMenu();
        }

        private void ModeEllipse_Click(object sender, RoutedEventArgs e)
        {
            GetSheetController().SetMode(SheetMode.Ellipse);
            UpdateModeMenu();
        }

        private void ModeText_Click(object sender, RoutedEventArgs e)
        {
            GetSheetController().SetMode(SheetMode.Text);
            UpdateModeMenu();
        }
        
        private void ModeImage_Click(object sender, RoutedEventArgs e)
        {
            GetSheetController().SetMode(SheetMode.Image);
            UpdateModeMenu();
        }

        #endregion

        #region Logic Menu Events

        private void LogicInvertLineStart_Click(object sender, RoutedEventArgs e)
        {
            GetSheetController().InvertSelectedLineStart();
        }

        private void LogicInvertLineEnd_Click(object sender, RoutedEventArgs e)
        {
            GetSheetController().InvertSelectedLineEnd();
        } 

        #endregion
    }
}
