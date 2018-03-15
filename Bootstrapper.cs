using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;

// https://github.com/oriches/Simple.Wpf.Exceptions/blob/master/Simple.Wpf.Exceptions/BootStrapper.cs
namespace ImageHue
{
    using ViewModel;
    using Model;
    using Autofac.Core;

    public static class Bootstrapper
    {
        private static ILifetimeScope _rootScope;
        private static IChromeViewModel _mainViewModel;

        public static IChromeViewModel RootVisual
        {
            get
            {
                if(_rootScope == null)
                {
                    Start();
                }

                _mainViewModel = _rootScope.Resolve<IChromeViewModel>();
                return _mainViewModel;
            }
        }

        public static void Start()
        {
            if(_rootScope != null)
            {
                return;
            }

            var builder = new ContainerBuilder();
            var assemblies = new[] { Assembly.GetExecutingAssembly() };

            builder.RegisterAssemblyTypes(assemblies).Where(a => typeof(IViewModel).IsAssignableFrom(a)).AsImplementedInterfaces();
            builder.RegisterAssemblyTypes(assemblies).Where(a => typeof(IModel).IsAssignableFrom(a)).SingleInstance().AsImplementedInterfaces();

            _rootScope = builder.Build();
        }

        public static void Stop()
        {
            if (_rootScope == null) return;
            _rootScope.Dispose();
        }

        public static T Resolve<T>()
        {
            if(_rootScope == null)
            {
                throw new Exception("Root scope not initialized");
            }
            return _rootScope.Resolve<T>(new Parameter[0]);
        }

        public static T Resolve<T>(Parameter[] parameters)
        {
            if (_rootScope == null)
            {
                throw new Exception("Root scope not initialized");
            }

            return _rootScope.Resolve<T>(parameters);
        }
    }
}
