using Autofac;
using Microsoft.Win32;
using Sheet.Block;
using Sheet.Item;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Sheet
{
    #region OpenFileDialog
    
    public interface IOpenFileDialog
    {
        string Filter { get; set; }
        string FileName { get; set; }
        string[] FileNames{ get; set; }
        int FilterIndex { get; set; }
        bool ShowDialog();
    }
    
    public class Win32OpenFileDialog : IOpenFileDialog
    {
        private readonly OpenFileDialog dlg;
        
        public Win32OpenFileDialog()
        {
            dlg = new OpenFileDialog();
        }
            
        public string Filter { get; set; }
        public string FileName { get; set; }
        public string[] FileNames { get; set; }
        public int FilterIndex { get; set; }
            
        public bool ShowDialog()
        {
            FileNames = null;
            
            dlg.Filter = Filter;
            dlg.FilterIndex = FilterIndex;
            dlg.FileName = FileName;
            
            var result = dlg.ShowDialog();
            if (result.HasValue && result.Value == true)
            {
                FileName = dlg.FileName;
                FileNames = dlg.FileNames;
                FilterIndex = dlg.FilterIndex;
                return true;
            }
            return false;
        }
    }
    
    #endregion

    #region SaveFileDialog
    
    public interface ISaveFileDialog
    {
        string Filter { get; set; }
        string FileName { get; set; }
        string[] FileNames { get; set; }
        int FilterIndex { get; set; }
        bool ShowDialog();
    }
    
    public class Win32SaveFileDialog : ISaveFileDialog
    {
        private readonly SaveFileDialog dlg;
        
        public Win32SaveFileDialog()
        {
            dlg = new SaveFileDialog();
        }
            
        public string Filter { get; set; }
        public string FileName { get; set; }
        public string[] FileNames { get; set; }
        public int FilterIndex { get; set; }
            
        public bool ShowDialog()
        {
            FileNames = null;
        
            dlg.Filter = Filter;
            dlg.FilterIndex = FilterIndex;
            dlg.FileName = FileName;
            
            var result = dlg.ShowDialog();
            if (result.HasValue && result.Value == true)
            {
                FileName = dlg.FileName;
                FileNames = dlg.FileNames;
                FilterIndex = dlg.FilterIndex;
                return true;
            }
            return false;
        }
    }
    
    #endregion

    #region AppServiceLocator

    public class AppServiceLocator : IServiceLocator
    {
        public T GetInstance<T>()
        {
            return App.Scope.Resolve<T>();
        }
    }

    #endregion

    #region AppScopeServiceLocator

    public class AppScopeServiceLocator : IScopeServiceLocator
    {
        ILifetimeScope _scope;
        
        public AppScopeServiceLocator()
        {
            CreateScope();
        }
        
        public T GetInstance<T>()
        {
            return _scope.Resolve<T>();
        }
        
        public void CreateScope()
        {
            _scope = App.Scope.BeginLifetimeScope();
        }
        
        public void ReleaseScope()
        {
            _scope.Dispose();
        }
    }

    #endregion

    #region Modules

    public class EntryModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<EntryController>().As<IEntryController>().SingleInstance();
            builder.RegisterType<EntrySerializer>().As<IEntrySerializer>().SingleInstance();
            builder.RegisterType<EntryFactory>().As<IEntryFactory>().SingleInstance();
        }
    }

    public class BlockModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<BlockController>().As<IBlockController>().SingleInstance();
            builder.RegisterType<BlockSerializer>().As<IBlockSerializer>().SingleInstance();
        }
    }

    public class ItemModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ItemController>().As<IItemController>().SingleInstance();
            builder.RegisterType<ItemSerializer>().As<IItemSerializer>().SingleInstance();
        }
    }

    public class WpfModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<WpfClipboard>().As<IClipboard>().SingleInstance();
            
            builder.RegisterType<Win32OpenFileDialog>().As<IOpenFileDialog>().InstancePerDependency();
            builder.RegisterType<Win32SaveFileDialog>().As<ISaveFileDialog>().InstancePerDependency();
            
            builder.RegisterType<WpfBlockFactory>().As<IBlockFactory>().SingleInstance();
            builder.RegisterType<WpfBlockHelper>().As<IBlockHelper>().SingleInstance();
            builder.RegisterType<WpfCanvasSheet>().As<ISheet>().InstancePerDependency();

            builder.RegisterType<SheetHistoryController>().As<IHistoryController>().InstancePerDependency();

            builder.RegisterType<LibraryControl>()
                .As<ILibraryView>()
                .As<ILibraryController>()
                .SingleInstance();

            builder.RegisterType<SheetControl>()
                .As<ISheetView>()
                .As<IZoomController>()
                .As<ICursorController>()
                .InstancePerLifetimeScope();

            builder.RegisterType<SheetWindow>().As<IMainWindow>().InstancePerLifetimeScope();
        }
    } 

    public class TestModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<TestHistoryController>().As<IHistoryController>().InstancePerDependency();
            builder.RegisterType<TestLibraryController>().As<ILibraryController>().SingleInstance();
            builder.RegisterType<TestZoomController>().As<IZoomController>().InstancePerLifetimeScope();
            builder.RegisterType<TestCursorController>().As<ICursorController>().InstancePerLifetimeScope();
        }
    }

    #endregion

    public partial class App : Application
    {
        #region Constructor

        public App()
        {
            if (Environment.UserInteractive)
            {
                Debug.Listeners.Add(new TextWriterTraceListener(System.Console.Out));
            }

            Init();
        }

        #endregion

        #region IoC

        public static IContainer Container { get; private set; }
        public static ILifetimeScope Scope { get; private set; }

        private void Init()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<AppServiceLocator>().As<IServiceLocator>().SingleInstance();
            builder.RegisterType<AppScopeServiceLocator>().As<IScopeServiceLocator>().InstancePerDependency();

            builder.RegisterType<Base64>().As<IBase64>().SingleInstance();
            builder.RegisterType<NewtonsoftJsonSerializer>().As<IJsonSerializer>().SingleInstance();

            builder.RegisterModule<EntryModule>();
            builder.RegisterModule<BlockModule>();
            builder.RegisterModule<ItemModule>();

            builder.RegisterType<PointController>().As<IPointController>().SingleInstance();
            builder.RegisterType<SheetPageFactory>().As<IPageFactory>().SingleInstance();
            builder.RegisterType<SheetController>().As<ISheetController>().InstancePerLifetimeScope();

            builder.RegisterModule<WpfModule>();

            using(Container = builder.Build())
            {
                using (Scope = Container.BeginLifetimeScope())
                {
                    using (var window = Scope.Resolve<IMainWindow>())
                    {
                        window.ShowDialog();
                    }
                }
            }
        }

        #endregion
    }
}
