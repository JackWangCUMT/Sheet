using Autofac;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Sheet
{
    #region AppInterfaceLocator

    public class AppInterfaceLocator : IInterfaceLocator
    {
        public T GetInterface<T>()
        {
            return App.Container.Resolve<T>();
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

            builder.RegisterInstance(new Base64()).As<IBase64>().SingleInstance();
            builder.RegisterInstance(new WpfBlockHelper()).As<IBlockHelper>().SingleInstance();
            builder.RegisterInstance(new WpfBlockFactory()).As<IBlockFactory>().SingleInstance();
            builder.RegisterInstance(new NewtonsoftJsonSerializer()).As<IJsonSerializer>().SingleInstance();
            builder.RegisterInstance(new EntryFactory()).As<IEntryFactory>().SingleInstance();

            builder.RegisterType<BlockSerializer>().As<IBlockSerializer>().SingleInstance();
            builder.RegisterType<ItemSerializer>().As<IItemSerializer>().SingleInstance();
            builder.RegisterType<EntrySerializer>().As<IEntrySerializer>().SingleInstance();
            builder.RegisterType<BlockController>().As<IBlockController>().SingleInstance();
            builder.RegisterType<ItemController>().As<IItemController>().SingleInstance();
            builder.RegisterType<EntryController>().As<IEntryController>().SingleInstance();
            builder.RegisterType<PointController>().As<IPointController>().SingleInstance();
            builder.RegisterType<LogicContentPageFactory>().As<IPageFactory>().SingleInstance();

            builder.RegisterType<AppInterfaceLocator>().As<IInterfaceLocator>().SingleInstance();

            builder.RegisterType<SheetWindow>();

            Container = builder.Build();

            var window = Container.Resolve<SheetWindow>();
            window.Show();
        } 

        #endregion
    }
}
