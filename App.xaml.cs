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
    public partial class App : Application
    {
        #region Constructor

        public App()
        {
            Init();
        } 

        #endregion

        #region IoC

        private void Init()
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(new WpfBlockHelper()).As<IBlockHelper>().SingleInstance();
            builder.RegisterInstance(new WpfBlockFactory()).As<IBlockFactory>().SingleInstance();
            builder.RegisterInstance(new NewtonsoftJsonSerializer()).As<IJsonSerializer>().SingleInstance();
            builder.RegisterInstance(new EntryFactory()).As<IEntryFactory>().SingleInstance();

            builder.RegisterType<BlockSerializer>().As<IBlockSerializer>().SingleInstance();
            builder.RegisterType<PointController>().As<IPointController>().SingleInstance();
            builder.RegisterType<BlockController>().As<IBlockController>().SingleInstance();

            builder.RegisterType<ItemSerializer>().As<IItemSerializer>().SingleInstance();
            builder.RegisterType<ItemController>().As<IItemController>().SingleInstance();

            builder.RegisterType<EntrySerializer>().As<IEntrySerializer>().SingleInstance();
            builder.RegisterType<EntryController>().As<IEntryController>().SingleInstance();

            builder.RegisterType<SheetWindow>();

            using (var container = builder.Build())
            {
                var window = container.Resolve<SheetWindow>();
                window.Show();
            }
        } 

        #endregion
    }
}
