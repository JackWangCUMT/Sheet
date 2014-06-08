using Autofac;
using Sheet.Block;
using Sheet.Entry;
using Sheet.Item;
using Sheet.Test;
using Sheet.UI.Views;
using Sheet.UI.Windows;
using Sheet.Util;
using Sheet.Util.Core;
using Sheet.WPF;
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
    #region IServiceLocator

    public interface IServiceLocator
    {
        T GetInstance<T>();
    }

    #endregion

    #region IScopeServiceLocator

    public interface IScopeServiceLocator
    {
        T GetInstance<T>();
        void CreateScope();
        void ReleaseScope();
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

    public class UtilModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Base64>().As<IBase64>().SingleInstance();
            builder.RegisterType<NewtonsoftJsonSerializer>().As<IJsonSerializer>().SingleInstance();
        }
    }

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
            builder.RegisterType<PointController>().As<IPointController>().SingleInstance();
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

    public class SheetModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SheetPageFactory>().As<IPageFactory>().SingleInstance();
            builder.RegisterType<SheetController>().As<ISheetController>().InstancePerLifetimeScope();
        }
    }

    public class WpfModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<WpfClipboard>().As<IClipboard>().SingleInstance();
            
            builder.RegisterType<WpfOpenFileDialog>().As<IOpenFileDialog>().InstancePerDependency();
            builder.RegisterType<WpfSaveFileDialog>().As<ISaveFileDialog>().InstancePerDependency();
            
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

    #region App

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

            builder.RegisterModule<UtilModule>();
            builder.RegisterModule<EntryModule>();
            builder.RegisterModule<BlockModule>();
            builder.RegisterModule<ItemModule>();
            builder.RegisterModule<SheetModule>();
            builder.RegisterModule<WpfModule>();

            ShowMainWindow(builder);
        }

        private void ShowMainWindow(ContainerBuilder builder)
        {
            using (Container = builder.Build())
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

    #endregion
}
