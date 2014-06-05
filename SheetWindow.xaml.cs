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
using Simulation.Tests;
using Simulation.Binary;

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

    public partial class SheetWindow : Window, IMainWindow
    {
        #region IoC

        private readonly IServiceLocator _serviceLocator;
        private readonly IEntryController _entryController;
        private readonly IEntryFactory _entryFactory;
        private readonly IEntrySerializer _entrySerializer;

        private ISheetController _sheetController;
        private List<ISheetController> _sheetControllers;
        private List<IScopeServiceLocator> _scopeServiceLocators;

        public SheetWindow(IServiceLocator serviceLocator)
        {
            InitializeComponent();

            this._serviceLocator = serviceLocator;
            this._entryController = serviceLocator.GetInstance<IEntryController>();
            this._entryFactory = serviceLocator.GetInstance<IEntryFactory>();
            this._entrySerializer = serviceLocator.GetInstance<IEntrySerializer>();
 
            _scopeServiceLocators = new List<IScopeServiceLocator>();
            _sheetControllers = new List<ISheetController>();

            SinglePage();
            //MultiPage();

            var library = _serviceLocator.GetInstance<ILibraryView>();
            Library.Content = library;

            Init();

            Loaded += (sender, e) => _sheetController.View.Focus();
        }

        private void SinglePage()
        {
            var sheet = CreateSheetView();
            Sheet.Content = sheet;
            _sheetController = _sheetControllers.FirstOrDefault();
        }

        private void MultiPage()
        {
            //for (int i = 0; i < 5; i++)
            //{
            //    var sheet = CreateSheetView();
            //    var contentControl = new ContentControl();
            //    contentControl.Content = sheet;
            //    Sheets.Children.Add(contentControl);
            //}
            //_sheetController = _sheetControllers.FirstOrDefault();
        }

        private ISheetView CreateSheetView()
        {
            var locator = _serviceLocator.GetInstance<IScopeServiceLocator>();
            _scopeServiceLocators.Add(locator);

            var controller = locator.GetInstance<ISheetController>();
            _sheetControllers.Add(controller);

            controller.HistoryController = locator.GetInstance<IHistoryController>();
            controller.LibraryController = locator.GetInstance<ILibraryController>();
            controller.ZoomController = locator.GetInstance<IZoomController>();
            controller.CursorController = locator.GetInstance<ICursorController>();

            controller.EditorSheet = locator.GetInstance<ISheet>();
            controller.BackSheet = locator.GetInstance<ISheet>();
            controller.ContentSheet = locator.GetInstance<ISheet>();
            controller.OverlaySheet = locator.GetInstance<ISheet>();

            controller.View = locator.GetInstance<ISheetView>();

            return controller.View;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_scopeServiceLocators != null)
            {
                foreach (var locator in _scopeServiceLocators)
                {
                    locator.ReleaseScope();
                }
            }
        } 

        #endregion

        #region Fields

        private string _solutionPath;
        private ObservableCollection<IDatabaseController> _databaseControllers;

        #endregion

        #region Init

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
            SizeBorder.ExecuteUpdateSize = (size) => _sheetController.SetAutoFitSize(size.Width, size.Height);
            SizeBorder.ExecuteSizeChanged = () => _sheetController.ZoomController.AutoFit();
        }

        private void InitSolution()
        {
            Solution.Init(_sheetController, _entryController);
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
            _databaseControllers = new ObservableCollection<IDatabaseController>();
            Databases.Tabs.ItemsSource = _databaseControllers;

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
            _databaseControllers.Add(controller);
        }

        public async void OpenDatabase()
        {
            var dlg = _serviceLocator.GetInstance<IOpenFileDialog>();
            dlg.Filter = FileDialogSettings.DatabaseFilter;
            dlg.FilterIndex = 1;
            dlg.FileName = "";

            if (dlg.ShowDialog() == true)
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
            _databaseControllers.Add(controller);
        }

        private CsvDatabaseController CreateDatabaseController(string name, string[] columns, List<string[]> data)
        {
            var controller = new CsvDatabaseController(name);

            controller.Columns = columns;
            controller.Data = data;

            return controller;
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
                await _sheetController.OpenTextPage(path);
            }
            else if (string.Compare(ext, FileDialogSettings.JsonPageExtension, true) == 0)
            {
                await _sheetController.OpenJsonPage(path);
            }
            else if (string.Compare(ext, FileDialogSettings.LibraryExtension, true) == 0)
            {
                await _sheetController.LoadLibrary(path);
            }
            else if (string.Compare(ext, FileDialogSettings.DatabaseExtension, true) == 0)
            {
                await OpenDatabase(path);
            }
        }

        #endregion

        #region Mode Menu

        private void UpdateModeMenu()
        {
            var mode = _sheetController.GetMode();
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
            if (_sheetController.GetMode() == SheetMode.TextEditor)
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
                        _sheetController.ZoomController.AutoFit();
                    }
                    break;
                // Ctrl+1: Actual Size
                case Key.D1:
                case Key.NumPad1:
                    if (onlyCtrl)
                    {
                        _sheetController.ZoomController.ActualSize();
                    }
                    break;
                // N: Mode None
                // Ctrl+N: New Page
                // Ctrl+Shift+N: New Solution
                case Key.N:
                    if (none)
                    {
                        _sheetController.SetMode(SheetMode.None);
                        UpdateModeMenu();
                    }
                    if (onlyCtrl)
                    {
                        _sheetController.NewPage();
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
                        _sheetController.OpenPage();
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
                        _sheetController.SavePage();
                    }
                    else if (ctrlShift)
                    {
                        SaveSolution();
                    }
                    else if (none)
                    {
                        _sheetController.SetMode(SheetMode.Selection);
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
                            _sheetController.Export(Solution.DataContext as SolutionEntry);
                        }
                        else
                        {
                            _sheetController.ExportPage();
                        }
                    }
                    else if (none)
                    {
                        _sheetController.SetMode(SheetMode.Ellipse);
                        UpdateModeMenu();
                    }
                    break;
                // Ctrl+L: Library
                // L: Mode Line
                case Key.L:
                    if (onlyCtrl)
                    {
                        _sheetController.LoadLibrary();
                    }
                    else if (none)
                    {
                        _sheetController.SetMode(SheetMode.Line);
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
                        _sheetController.SetMode(SheetMode.Image);
                        UpdateModeMenu();
                    }
                    break;
                // Ctrl+Z: Undo
                case Key.Z:
                    if (onlyCtrl)
                    {
                        _sheetController.HistoryController.Undo();
                    }
                    break;
                // Ctrl+Y: Redo
                case Key.Y:
                    if (onlyCtrl)
                    {
                        _sheetController.HistoryController.Redo();
                    }
                    break;
                // Ctrl+X: Cut
                // Ctrl+Shift+X: Cut as Json
                case Key.X:
                    if (onlyCtrl)
                    {
                        _sheetController.CutText();
                    }
                    else if (ctrlShift)
                    {
                        _sheetController.CutJson();
                    }
                    break;
                // Ctrl+C: Copy
                // Ctrl+Shift+C: Copy as Json
                case Key.C:
                    if (onlyCtrl)
                    {
                        _sheetController.CopyText();
                    }
                    else if (ctrlShift)
                    {
                        _sheetController.CopyJson();
                    }
                    break;
                // Ctrl+V: Paste
                // Ctrl+Shift+V: Paste as Json
                case Key.V:
                    if (onlyCtrl)
                    {
                        _sheetController.PasteText();
                    }
                    break;
                // Del: Delete
                // Ctrl+Del: Reset
                case Key.Delete:
                    if (onlyCtrl)
                    {
                        _sheetController.HistoryController.Register("Reset");
                        _sheetController.ResetPage();
                    }
                    else if (none)
                    {
                        _sheetController.Delete();
                    }
                    break;
                // Ctrl+A: Select All
                case Key.A:
                    if (onlyCtrl)
                    {
                        _sheetController.SelecteAll();
                    }
                    break;
                // B: Create Block
                // Ctrl+B: Break Block
                case Key.B:
                    if (onlyCtrl)
                    {
                        _sheetController.BreakBlock();
                    }
                    else if (none)
                    {
                        e.Handled = true;
                        _sheetController.CreateBlock();
                    }
                    break;
                // Up: Move Up
                case Key.Up:
                    if (none && _sheetController.View.IsFocused)
                    {
                        _sheetController.MoveUp();
                        e.Handled = true;
                    }
                    break;
                // Down: Move Down
                case Key.Down:
                    if (none && _sheetController.View.IsFocused)
                    {
                        _sheetController.MoveDown();
                        e.Handled = true;
                    }
                    break;
                // Left: Move Left
                case Key.Left:
                    if (none && _sheetController.View.IsFocused)
                    {
                        _sheetController.MoveLeft();
                        e.Handled = true;
                    }
                    break;
                // Right: Move Right
                case Key.Right:
                    if (none && _sheetController.View.IsFocused)
                    {
                        _sheetController.MoveRight();
                        e.Handled = true;
                    }
                    break;
                // F: Toggle Fill
                case Key.F:
                    if (none)
                    {
                        _sheetController.ToggleFill();
                    }
                    break;
                // I: Mode Insert
                case Key.I:
                    if (none)
                    {
                        _sheetController.SetMode(SheetMode.Insert);
                        UpdateModeMenu(); 
                    }
                    break;
                // P: Mode Point
                case Key.P:
                    if (none)
                    {
                        _sheetController.SetMode(SheetMode.Point);
                        UpdateModeMenu(); 
                    }
                    break;
                // R: Mode Rectangle
                case Key.R:
                    if (none)
                    {
                        _sheetController.SetMode(SheetMode.Rectangle);
                        UpdateModeMenu(); 
                    }
                    break;
                // T: Mode Text
                case Key.T:
                    if (none)
                    {
                        _sheetController.SetMode(SheetMode.Text);
                        UpdateModeMenu(); 
                    }
                    break;
                // F5: Start Simulation
                case Key.F5:
                    if (none)
                    {
                        StartSimulation();
                    }
                    break;
                // F6: Stop Simulation
                case Key.F6:
                    if (none)
                    {
                        StopSimulation();
                    }
                    break;
                // F7: Restart Simulation
                case Key.F7:
                    if (none)
                    {
                        RestartSimulation();
                    }
                    break;
                // Q: Invert Line Start
                case Key.Q:
                    if (none)
                    {
                        _sheetController.InvertSelectedLineStart(); 
                    }
                    break;
                // W: Invert Line End
                case Key.W:
                    if (none)
                    {
                        _sheetController.InvertSelectedLineEnd(); 
                    }
                    break;
            }
        }

        #endregion
        
        #region Solution

        public async void NewSolution()
        {
            var dlg = _serviceLocator.GetInstance<ISaveFileDialog>();
            dlg.Filter = FileDialogSettings.SolutionFilter;
            dlg.FilterIndex = 1;
            dlg.FileName = "solution";

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    _sheetController.ResetPage();

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

                _solutionPath = path;
            }
        }

        public async void OpenSolution()
        {
            var dlg = _serviceLocator.GetInstance<IOpenFileDialog>();
            dlg.Filter = FileDialogSettings.SolutionFilter;
            dlg.FilterIndex = 1;
            dlg.FileName = "";
            
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    _sheetController.ResetPage();

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

                _solutionPath = path;
            }
        }

        public void SaveSolution()
        {
            if (_solutionPath != null)
            {
                try
                {
                    var path = _solutionPath;
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
            if (_solutionPath != null)
            {
                _solutionPath = null;
                Solution.DataContext = null;

                _sheetController.ResetPage();
            }
        }

        #endregion

        #region Simulation
        
        private TestDemoSolution demo = null;
        
        private void CreateDemoSimulation()
        {
            var solution = TestDemoSolution.CreateDemoSolution();
            demo = new TestDemoSolution(solution, 100);
            demo.EnableSimulationDebug(false);
            demo.EnableSimulationLog(false);
            demo.StartSimulation();
        } 
        
        private void StartSimulation()
        {
            try
            {
                if (demo == null)
                {
                    var block = _sheetController.GetContent();
                    var serializer = new TestSerializer();
                    var solution = serializer.Serialize(block);

                    var renamer = new TestRenamer();
                    renamer.AutoRename(solution);

                    /*
                    var writer = new BinarySolutionWriter();
                    writer.Save("test.bin", solution);

                    var reader = new BinarySolutionReader();
                    var binarySolution = reader.Open("test.bin");

                    var factory = new TestFactory();
                    var signals = (binarySolution.Children[0].Children[0] as Simulation.Core.Context).Children.Where(c => c is Simulation.Core.Signal).Cast<Simulation.Core.Signal>();
                    foreach(var signal in signals)
                    {
                        var tag = factory.CreateSignalTag(signal.ElementId.ToString(), "", "", "");
                        binarySolution.Tags.Add(tag);
                        signal.Tag = tag;
                    }

                    solution = binarySolution;
                    */

                    demo = new TestDemoSolution(solution, 100);
                    demo.EnableSimulationDebug(false);
                    demo.EnableSimulationLog(false);
                    demo.StartSimulation();
                    
                    var window = new Window() { Title = "Tags", Width = 300, Height = 500, WindowStartupLocation = WindowStartupLocation.CenterScreen };
                    window.Content = new TagsControl();
                    window.DataContext = solution.Tags;
                    window.Show();
                }
            }
            catch(Exception ex)
            {
                Debug.Print(ex.Message);
                Debug.Print(ex.StackTrace);
            }
        }
        
        private void StopSimulation()
        {
            try
            {
                if (demo != null)
                {
                    demo.StopSimulation();
                    demo = null;
                }
            }
            catch(Exception ex)
            {
                Debug.Print(ex.Message);
                Debug.Print(ex.StackTrace);
            }
        }
        
        private void RestartSimulation()
        {
            StopSimulation();
            StartSimulation();
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
            _sheetController.NewPage();
        }

        private void FileOpenPage_Click(object sender, RoutedEventArgs e)
        {
            _sheetController.OpenPage();
        }

        private void FileSavePage_Click(object sender, RoutedEventArgs e)
        {
            _sheetController.SavePage();
        }

        private void FileExport_Click(object sender, RoutedEventArgs e)
        {
            if (Solution.DataContext != null)
            {
                _sheetController.Export(Solution.DataContext as SolutionEntry);
            }
            else
            {
                _sheetController.ExportPage();
            }
        }

        private void FileLibrary_Click(object sender, RoutedEventArgs e)
        {
            _sheetController.LoadLibrary();
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
            _sheetController.HistoryController.Undo();
        }

        private void EditRedo_Click(object sender, RoutedEventArgs e)
        {
            _sheetController.HistoryController.Redo();
        }

        private void EditCut_Click(object sender, RoutedEventArgs e)
        {
            _sheetController.CutText();
        }

        private void EditCopy_Click(object sender, RoutedEventArgs e)
        {
            _sheetController.CopyText();
        }

        private void EditPaste_Click(object sender, RoutedEventArgs e)
        {
            _sheetController.PasteText();
        }

        private void EditDelete_Click(object sender, RoutedEventArgs e)
        {
            _sheetController.Delete();
        }

        private void EditReset_Click(object sender, RoutedEventArgs e)
        {
            _sheetController.HistoryController.Register("Reset");
            _sheetController.ResetPage();
        }

        private void EditSelectAll_Click(object sender, RoutedEventArgs e)
        {
            _sheetController.SelecteAll();
        }

        private void EditCreateBlock_Click(object sender, RoutedEventArgs e)
        {
            _sheetController.CreateBlock();
        }

        private void EditBreakBlock_Click(object sender, RoutedEventArgs e)
        {
            _sheetController.BreakBlock();
        }

        private void EditMoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (_sheetController.View.IsFocused)
            {
                _sheetController.MoveUp();
            }
        }

        private void EditMoveDown_Click(object sender, RoutedEventArgs e)
        {
            if (_sheetController.View.IsFocused)
            {
                _sheetController.MoveDown();
            }
        }

        private void EditMoveLeft_Click(object sender, RoutedEventArgs e)
        {
            if (_sheetController.View.IsFocused)
            {
                _sheetController.MoveLeft();
            }
        }

        private void EditMoveRight_Click(object sender, RoutedEventArgs e)
        {
            if (_sheetController.View.IsFocused)
            {
                _sheetController.MoveRight();
            }
        }

        private void EditToggleFill_Click(object sender, RoutedEventArgs e)
        {
            _sheetController.ToggleFill();
        }

        #endregion

        #region View Menu Events

        private void ViewZoomToPageLevel_Click(object sender, RoutedEventArgs e)
        {
            _sheetController.ZoomController.AutoFit();
        }

        private void ViewZoomActualSize_Click(object sender, RoutedEventArgs e)
        {
            _sheetController.ZoomController.ActualSize();
        }

        #endregion

        #region Mode Menu Events

        private void ModeNone_Click(object sender, RoutedEventArgs e)
        {
            _sheetController.SetMode(SheetMode.None);
            UpdateModeMenu();
        }

        private void ModeSelection_Click(object sender, RoutedEventArgs e)
        {
            _sheetController.SetMode(SheetMode.Selection);
            UpdateModeMenu();
        }

        private void ModeInsert_Click(object sender, RoutedEventArgs e)
        {
            _sheetController.SetMode(SheetMode.Insert);
            UpdateModeMenu();
        }

        private void ModePoint_Click(object sender, RoutedEventArgs e)
        {
            _sheetController.SetMode(SheetMode.Point);
            UpdateModeMenu();
        }
        
        private void ModeLine_Click(object sender, RoutedEventArgs e)
        {
            _sheetController.SetMode(SheetMode.Line);
            UpdateModeMenu();
        }

        private void ModeRectangle_Click(object sender, RoutedEventArgs e)
        {
            _sheetController.SetMode(SheetMode.Rectangle);
            UpdateModeMenu();
        }

        private void ModeEllipse_Click(object sender, RoutedEventArgs e)
        {
            _sheetController.SetMode(SheetMode.Ellipse);
            UpdateModeMenu();
        }

        private void ModeText_Click(object sender, RoutedEventArgs e)
        {
            _sheetController.SetMode(SheetMode.Text);
            UpdateModeMenu();
        }
        
        private void ModeImage_Click(object sender, RoutedEventArgs e)
        {
            _sheetController.SetMode(SheetMode.Image);
            UpdateModeMenu();
        }

        #endregion

        #region Simulation Menu Events

        private void SimulationStart_Click(object sender, RoutedEventArgs e)
        {
            StartSimulation();
        }

        private void SimulationStop_Click(object sender, RoutedEventArgs e)
        {
            StopSimulation();
        } 

        private void SimulationRestart_Click(object sender, RoutedEventArgs e)
        {
            RestartSimulation();
        } 
        
        #endregion
        
        #region Logic Menu Events

        private void LogicInvertLineStart_Click(object sender, RoutedEventArgs e)
        {
            _sheetController.InvertSelectedLineStart();
        }

        private void LogicInvertLineEnd_Click(object sender, RoutedEventArgs e)
        {
            _sheetController.InvertSelectedLineEnd();
        } 

        #endregion
    }
}
