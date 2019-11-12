using Microsoft.Practices.Unity;
using OperationView.Views;
using Prism.Modularity;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationView
{
    public class OperationViewModule : IModule
    {
        private IRegionManager _regionManager;
        private IUnityContainer _container;

        public OperationViewModule(IUnityContainer container, IRegionManager regionManager)
        {
            _container = container;
            _regionManager = regionManager;
        }

        public void Initialize()
        {
            _regionManager.RegisterViewWithRegion("OperationR", typeof(OperationViewContainer));
        }
    }
}
