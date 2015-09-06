using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Genesis.ViewModels;

namespace Genesis.Bootstrap
{
    public class AppBootstrapper : BootstrapperBase
    {
        private WindsorContainer container;

        public AppBootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            container = new WindsorContainer();

            container.AddFacility<EventRegistrationFacility>();
            container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel));

            container.Register(
                Component.For<IWindowManager>().ImplementedBy<WindowManager>().LifestyleSingleton(),
                Component.For<IEventAggregator>().ImplementedBy<EventAggregator>().LifestyleSingleton(),
                Classes.FromThisAssembly().InSameNamespaceAs<IShellViewModel>()
                       .If(t => t.GetInterfaces().Any(i => i == typeof (IScreen))).WithServiceDefaultInterfaces().LifestyleSingleton()
                );

            container.Install(FromAssembly.This());
        }

        protected override object GetInstance(Type serviceType, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return container.Resolve(serviceType);
            }
            return container.Resolve(key, serviceType);
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return container.ResolveAll(serviceType).Cast<object>();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<IShellViewModel>();
            base.OnStartup(sender, e);
        }
    }
}