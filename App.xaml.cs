using Autofac;
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
    #region AppServiceLocator

    public class AppServiceLocator : IServiceLocator
    {
        public T GetInstance<T>()
        {
            return App.Container.Resolve<T>();
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
            _scope = App.Container.BeginLifetimeScope();
        }
        
        public void ReleaseScope()
        {
            _scope.Dispose();
        }
    }

    #endregion

    public partial class App : Application
    {
        #region Constructor

        public App()
        {
            Init();
        } 

        #endregion

        #region IoC

        public static IContainer Container { get; private set; }

        private void Init()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<AppServiceLocator>().As<IServiceLocator>().SingleInstance();
            builder.RegisterType<AppScopeServiceLocator>().As<IScopeServiceLocator>().InstancePerDependency();

            builder.RegisterType<WpfClipboard>().As<IClipboard>().SingleInstance();
            builder.RegisterType<Base64>().As<IBase64>().SingleInstance();
            builder.RegisterType<NewtonsoftJsonSerializer>().As<IJsonSerializer>().SingleInstance();

            builder.RegisterType<EntryController>().As<IEntryController>().SingleInstance();
            builder.RegisterType<EntrySerializer>().As<IEntrySerializer>().SingleInstance();
            builder.RegisterType<EntryFactory>().As<IEntryFactory>().SingleInstance();

            builder.RegisterType<BlockController>().As<IBlockController>().SingleInstance();
            builder.RegisterType<BlockSerializer>().As<IBlockSerializer>().SingleInstance();
            builder.RegisterType<WpfBlockFactory>().As<IBlockFactory>().SingleInstance();
            builder.RegisterType<WpfBlockHelper>().As<IBlockHelper>().SingleInstance();

            builder.RegisterType<ItemController>().As<IItemController>().SingleInstance();
            builder.RegisterType<ItemSerializer>().As<IItemSerializer>().SingleInstance();

            builder.RegisterType<PointController>().As<IPointController>().SingleInstance();
            builder.RegisterType<SheetPageFactory>().As<IPageFactory>().SingleInstance();

            builder.RegisterType<WpfCanvasSheet>().As<ISheet>().InstancePerDependency();
            builder.RegisterType<SheetController>().As<ISheetController>().InstancePerLifetimeScope();

            //builder.RegisterType<TestHistoryController>().As<IHistoryController>().InstancePerDependency();
            //builder.RegisterType<TestLibraryController>().As<ILibraryController>().SingleInstance();
            //builder.RegisterType<TestZoomController>().As<IZoomController>().InstancePerLifetimeScope();
            //builder.RegisterType<TestCursorController>().As<ICursorController>().InstancePerLifetimeScope();
            //builder.RegisterType<SheetControl>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<SheetHistoryController>().As<IHistoryController>().InstancePerDependency();
            builder.RegisterType<LibraryControl>().AsSelf().As<ILibraryController>().SingleInstance();
            builder.RegisterType<SheetControl>()
                .AsSelf()
                .As<IZoomController>()
                .As<ICursorController>()
                .InstancePerLifetimeScope();
            builder.RegisterType<SheetWindow>().InstancePerLifetimeScope();

            using(Container = builder.Build())
            {
                using (var scope = Container.BeginLifetimeScope())
                {
                    var window = scope.Resolve<SheetWindow>();
                    window.Show();
                }
            }
        }

        #endregion
    }
}
