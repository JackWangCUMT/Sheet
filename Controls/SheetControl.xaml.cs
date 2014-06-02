using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sheet
{
    public partial class SheetControl : UserControl, IZoomController, ICursorController
    {
        #region IoC

        private readonly IServiceLocator _serviceLocator;

        public SheetControl(IServiceLocator serviceLocator)
        {
            InitializeComponent();

            this._serviceLocator = serviceLocator;

            this.SheetController = CreateSheetController(serviceLocator);
            Loaded += (sender, e) => this.SheetController.Init();
        }

        private ISheetController CreateSheetController(IServiceLocator serviceLocator)
        {
            var controller = new SheetController(serviceLocator);

            var serializer = serviceLocator.GetInstance<IItemSerializer>();

            controller.HistoryController = new PageHistoryController(controller, serializer);
            controller.ZoomController = this;
            controller.CursorController = this;

            controller.FocusSheet = () => this.Focus();
            controller.IsSheetFocused = () => this.IsFocused;

            controller.EditorSheet = new WpfCanvasSheet(EditorCanvas);
            controller.BackSheet = new WpfCanvasSheet(Root.Back);
            controller.ContentSheet = new WpfCanvasSheet(Root.Sheet);
            controller.OverlaySheet = new WpfCanvasSheet(Root.Overlay);

            return controller;
        }

        #endregion

        #region Properties

        public ISheetController SheetController { get; set; }

        #endregion

        #region AutoFit

        private int FindZoomIndex(double factor)
        {
            int index = -1;

            if (SheetController == null || SheetController.Options == null)
            {
                return index;
            }

            for (int i = 0; i < SheetController.Options.ZoomFactors.Length; i++)
            {
                if (SheetController.Options.ZoomFactors[i] > factor)
                {
                    index = i;
                    break;
                }
            }

            index = Math.Max(0, index);
            index = Math.Min(index, SheetController.Options.MaxZoomIndex);

            return index;
        }

        private void AutoFit(double finalWidth,  double finalHeight, double desiredWidth, double desiredHeight)
        {
            if (SheetController == null || SheetController.Options == null)
            {
                return;
            }

            // calculate factor
            double fwidth = finalWidth / SheetController.Options.PageWidth;
            double fheight = finalHeight / SheetController.Options.PageHeight;
            double factor = Math.Min(fwidth, fheight);
            double panX = (finalWidth - (SheetController.Options.PageWidth * factor)) / 2.0;
            double panY = (finalHeight - (SheetController.Options.PageHeight * factor)) / 2.0;
            double dx = Math.Max(0, (finalWidth - desiredWidth) / 2.0);
            double dy = Math.Max(0, (finalHeight - desiredHeight) / 2.0);

            // adjust zoom
            ZoomIndex = FindZoomIndex(factor);
            Zoom = factor;

            // adjust pan
            PanX = panX - dx;
            PanY = panY - dy;
        }

        #endregion

        #region IZoomController

        private int zoomIndex = -1;
        public int ZoomIndex
        {
            get { return zoomIndex; }
            set
            {
                if (SheetController == null || SheetController.Options == null)
                {
                    return;
                }

                if (value >= 0 && value <= SheetController.Options.MaxZoomIndex)
                {
                    zoomIndex = value;
                    Zoom = SheetController.Options.ZoomFactors[zoomIndex];
                }
            }
        }

        public double Zoom
        {
            get { return zoom.ScaleX; }
            set
            {
                if (SheetController == null || SheetController.Options == null)
                {
                    return;
                }

                if (IsLoaded)
                {
                    SheetController.AdjustPageThickness(value);
                }

                zoom.ScaleX = value;
                zoom.ScaleY = value;
                SheetController.Options.Zoom = value;
            }
        }

        public double PanX
        {
            get { return pan.X; }
            set
            {
                if (SheetController == null || SheetController.Options == null)
                {
                    return;
                }

                pan.X = value;
                SheetController.Options.PanX = value;
            }
        }

        public double PanY
        {
            get { return pan.Y; }
            set
            {
                if (SheetController == null || SheetController.Options == null)
                {
                    return;
                }

                pan.Y = value;
                SheetController.Options.PanY = value;
            }
        }

        public void AutoFit()
        {
            AutoFit(SheetController.LastFinalWidth, SheetController.LastFinalHeight, DesiredSize.Width, DesiredSize.Height);
        }

        public void ActualSize()
        {
            zoomIndex = SheetController.Options.DefaultZoomIndex;
            Zoom = SheetController.Options.ZoomFactors[zoomIndex];
            PanX = 0.0;
            PanY = 0.0;
        }

        #endregion

        #region ICursorController

        public void Set(SheetCursor cursor)
        {
            switch(cursor)
            {
                case SheetCursor.Normal:
                    Cursor = Cursors.Arrow;
                    break;
                case SheetCursor.Move:
                    Cursor = Cursors.SizeAll;
                    break;
                case SheetCursor.Pan:
                    Cursor = Cursors.ScrollAll;
                    break;
                default:
                    break;
            }
        }

        public SheetCursor Get()
        {
            return SheetCursor.Unknown;
        }

        #endregion

        #region Events
        
        private void LeftDown(MouseButtonEventArgs e)
        {
            bool onlyCtrl = Keyboard.Modifiers == ModifierKeys.Control;
            bool onlyShift = Keyboard.Modifiers == ModifierKeys.Shift;
            bool sourceIsThumb = ((e.OriginalSource as FrameworkElement).TemplatedParent) is Thumb;
            Point sheetPoint = e.GetPosition(SheetController.OverlaySheet.GetParent() as FrameworkElement);
            XImmutablePoint sheetPosition = new XImmutablePoint(sheetPoint.X, sheetPoint.Y);
            Point rootPoint = e.GetPosition(this);
            XImmutablePoint rootPosition = new XImmutablePoint(rootPoint.X, rootPoint.Y);

            var args = new InputArgs()
            {
                OnlyControl = onlyCtrl,
                OnlyShift = onlyShift,
                SourceType = sourceIsThumb ? ItemType.Thumb : ItemType.None,
                SheetPosition = sheetPosition,
                RootPosition = rootPosition,
                Handled = (handled) => e.Handled = handled,
                Delta = 0,
                Button = InputButton.Left,
                Clicks = 1
            };

            SheetController.LeftDown(args);
        }

        private void LeftUp(MouseButtonEventArgs e)
        {
            bool onlyCtrl = Keyboard.Modifiers == ModifierKeys.Control;
            bool onlyShift = Keyboard.Modifiers == ModifierKeys.Shift;
            bool sourceIsThumb = ((e.OriginalSource as FrameworkElement).TemplatedParent) is Thumb;
            Point sheetPoint = e.GetPosition(SheetController.OverlaySheet.GetParent() as FrameworkElement);
            XImmutablePoint sheetPosition = new XImmutablePoint(sheetPoint.X, sheetPoint.Y);
            Point rootPoint = e.GetPosition(this);
            XImmutablePoint rootPosition = new XImmutablePoint(rootPoint.X, rootPoint.Y);

            var args = new InputArgs()
            {
                OnlyControl = onlyCtrl,
                OnlyShift = onlyShift,
                SourceType = sourceIsThumb ? ItemType.Thumb : ItemType.None,
                SheetPosition = sheetPosition,
                RootPosition = rootPosition,
                Handled = (handled) => e.Handled = handled,
                Delta = 0,
                Button = InputButton.Left,
                Clicks = 1
            };

            SheetController.LeftUp(args);
        }

        private void Move(MouseEventArgs e)
        {
            bool onlyCtrl = Keyboard.Modifiers == ModifierKeys.Control;
            bool onlyShift = Keyboard.Modifiers == ModifierKeys.Shift;
            bool sourceIsThumb = ((e.OriginalSource as FrameworkElement).TemplatedParent) is Thumb;
            Point sheetPoint = e.GetPosition(SheetController.OverlaySheet.GetParent() as FrameworkElement);
            XImmutablePoint sheetPosition = new XImmutablePoint(sheetPoint.X, sheetPoint.Y);
            Point rootPoint = e.GetPosition(this);
            XImmutablePoint rootPosition = new XImmutablePoint(rootPoint.X, rootPoint.Y);

            var args = new InputArgs()
            {
                OnlyControl = onlyCtrl,
                OnlyShift = onlyShift,
                SourceType = sourceIsThumb ? ItemType.Thumb : ItemType.None,
                SheetPosition = sheetPosition,
                RootPosition = rootPosition,
                Handled = (handled) => e.Handled = handled,
                Delta = 0,
                Button = InputButton.Left,
                Clicks = 1
            };

            SheetController.Move(args);
        }

        private void RightDown(MouseButtonEventArgs e)
        {
            bool onlyCtrl = Keyboard.Modifiers == ModifierKeys.Control;
            bool onlyShift = Keyboard.Modifiers == ModifierKeys.Shift;
            bool sourceIsThumb = ((e.OriginalSource as FrameworkElement).TemplatedParent) is Thumb;
            Point sheetPoint = e.GetPosition(SheetController.OverlaySheet.GetParent() as FrameworkElement);
            XImmutablePoint sheetPosition = new XImmutablePoint(sheetPoint.X, sheetPoint.Y);
            Point rootPoint = e.GetPosition(this);
            XImmutablePoint rootPosition = new XImmutablePoint(rootPoint.X, rootPoint.Y);

            var args = new InputArgs()
            {
                OnlyControl = onlyCtrl,
                OnlyShift = onlyShift,
                SourceType = sourceIsThumb ? ItemType.Thumb : ItemType.None,
                SheetPosition = sheetPosition,
                RootPosition = rootPosition,
                Handled = (handled) => e.Handled = handled,
                Delta = 0,
                Button = InputButton.Left,
                Clicks = 1
            };

            SheetController.RightDown(args);
        }

        private void RightUp(MouseButtonEventArgs e)
        {
            bool onlyCtrl = Keyboard.Modifiers == ModifierKeys.Control;
            bool onlyShift = Keyboard.Modifiers == ModifierKeys.Shift;
            bool sourceIsThumb = ((e.OriginalSource as FrameworkElement).TemplatedParent) is Thumb;
            Point sheetPoint = e.GetPosition(SheetController.OverlaySheet.GetParent() as FrameworkElement);
            XImmutablePoint sheetPosition = new XImmutablePoint(sheetPoint.X, sheetPoint.Y);
            Point rootPoint = e.GetPosition(this);
            XImmutablePoint rootPosition = new XImmutablePoint(rootPoint.X, rootPoint.Y);

            var args = new InputArgs()
            {
                OnlyControl = onlyCtrl,
                OnlyShift = onlyShift,
                SourceType = sourceIsThumb ? ItemType.Thumb : ItemType.None,
                SheetPosition = sheetPosition,
                RootPosition = rootPosition,
                Handled = (handled) => e.Handled = handled,
                Delta = 0,
                Button = InputButton.Left,
                Clicks = 1
            };

            SheetController.RightUp(args);
        }

        private void Wheel(MouseWheelEventArgs e)
        {
            int d = e.Delta;
            var p = e.GetPosition(Layout);
            SheetController.Wheel(d, new XImmutablePoint(p.X, p.Y));
        }

        private void Down(MouseButtonEventArgs e)
        {
            bool onlyCtrl = Keyboard.Modifiers == ModifierKeys.Control;
            bool onlyShift = Keyboard.Modifiers == ModifierKeys.Shift;
            bool sourceIsThumb = ((e.OriginalSource as FrameworkElement).TemplatedParent) is Thumb;
            Point sheetPoint = e.GetPosition(SheetController.OverlaySheet.GetParent() as FrameworkElement);
            XImmutablePoint sheetPosition = new XImmutablePoint(sheetPoint.X, sheetPoint.Y);
            Point rootPoint = e.GetPosition(this);
            XImmutablePoint rootPosition = new XImmutablePoint(rootPoint.X, rootPoint.Y);

            var args = new InputArgs()
            {
                OnlyControl = onlyCtrl,
                OnlyShift = onlyShift,
                SourceType = sourceIsThumb ? ItemType.Thumb : ItemType.None,
                SheetPosition = sheetPosition,
                RootPosition = rootPosition,
                Handled = (handled) => e.Handled = handled,
                Delta = 0,
                Clicks = e.ClickCount
            };

            switch(e.ChangedButton)
            {
                case MouseButton.Left: args.Button = InputButton.Left; break;
                case MouseButton.Middle: args.Button = InputButton.Middle; break;
                case MouseButton.Right: args.Button = InputButton.Right; break;
                default: args.Button = InputButton.None; break;
            }

            SheetController.Down(args);
        }

        private void UserControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Focus();
            LeftDown(e);
        }

        private void UserControl_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            LeftUp(e);
        }

        private void UserControl_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Move(e);
        }

        private void UserControl_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Focus();
            RightDown(e);
        }

        private void UserControl_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            RightUp(e);
        }

        private void UserControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Wheel(e);
        }

        private void UserControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Down(e);
        }

        #endregion

        #region Drop

        public const string BlockDropFormat = "Block";
        public const string DataDropFormat = "Data";

        private void UserControl_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(BlockDropFormat) || !e.Data.GetDataPresent(DataDropFormat) || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void UserControl_Drop(object sender, DragEventArgs e)
        {
            Point point = e.GetPosition(SheetController.OverlaySheet.GetParent() as FrameworkElement);
            XImmutablePoint position = new XImmutablePoint(point.X, point.Y);

            if (e.Data.GetDataPresent(BlockDropFormat))
            {
                var blockItem = e.Data.GetData(BlockDropFormat) as BlockItem;
                if (blockItem != null)
                {
                    SheetController.Insert(blockItem, position, true);
                    e.Handled = true;
                }
            }
            else if (e.Data.GetDataPresent(DataDropFormat))
            {
                var dataItem = e.Data.GetData(DataDropFormat) as DataItem;
                if (dataItem != null)
                {
                    SheetController.TryToBindData(position, dataItem);
                    e.Handled = true;
                }
            }
        }

        #endregion
    }
}
