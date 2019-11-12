using Microsoft.Practices.Unity;
using Prism.Logging;
using Prism.Modularity;
using Prism.Regions;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CommandTransmission
{
    class Bootstrapper : UnityBootstrapper
    {
        private RegionAdapterMappings baseRegionAdapterMappings;
        private IRegionBehaviorFactory iRegionBehaviorFactory;

        protected override ILoggerFacade CreateLogger()
        {
            return base.CreateLogger();
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            //return new DirectoryModuleCatalog() { ModulePath = @".\Modules" };
            return base.CreateModuleCatalog();
        }

        protected override void ConfigureModuleCatalog()
        {

        }

        protected override IUnityContainer CreateContainer()
        {
            return base.CreateContainer();
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
        }

        protected override void ConfigureServiceLocator()
        {
            base.ConfigureServiceLocator();
        }

        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();

        }

        protected override RegionAdapterMappings ConfigureRegionAdapterMappings()
        {
            baseRegionAdapterMappings = base.ConfigureRegionAdapterMappings();
            return baseRegionAdapterMappings;
        }

        protected override IRegionBehaviorFactory ConfigureDefaultRegionBehaviors()
        {
            iRegionBehaviorFactory = base.ConfigureDefaultRegionBehaviors();
            return iRegionBehaviorFactory;
        }

        protected override void RegisterFrameworkExceptionTypes()
        {
            base.RegisterFrameworkExceptionTypes();
        }

        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void InitializeShell()
        {
        }

        protected override void InitializeModules()
        {
            base.InitializeModules();
            ((MainWindow)Shell).Show();
        }
    }
}
