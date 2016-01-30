using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Genesis.ViewModels;
using Action = Caliburn.Micro.Action;
using Message = Caliburn.Micro.Message;

namespace Genesis.Bootstrap
{
    public class AppBootstrapper : BootstrapperBase
    {
        private WindsorContainer container;

        public AppBootstrapper()
        {
            Initialize();

            ViewLocator.AddSubNamespaceMapping("ViewModels.*", "Views");

            LogManager.GetLog = type => new DebugLog(type);

            ActionMessage.SetMethodBinding = context => {
                var source = context.Source;

                DependencyObject currentElement = source;

                while (currentElement != null)
                {

                    if (Action.HasTargetSet(currentElement))
                    {
                        var target = Message.GetHandler(currentElement);
                        if (target != null)
                        {
                            var method = ActionMessage.GetTargetMethod(context.Message, target);
                            if (method != null)
                            {
                                context.Method = method;
                                context.Target = target;
                                context.View = currentElement;
                                return;
                            }
                        }
                        else
                        {
                            context.View = currentElement;
                            return;
                        }
                    }
                    else
                    {
                        //in a tree view, the Action.Target DP (if using Model.Bind for instance) only get's set inside a content presenter,
                        //which is on a different branch insinde a Tree View Item, and will not be found recursively. This amends the original
                        //strategy and if no Action.Target property is set, the DataContext is considered as a target too.

                        var target = currentElement.GetValue(FrameworkElement.DataContextProperty);
                        var method = ActionMessage.GetTargetMethod(context.Message, target);

                        if (method != null)
                        {
                            context.Target = target;
                            context.Method = method;
                            context.View = source;
                            return;
                        }
                    }

                    currentElement = VisualTreeHelper.GetParent(currentElement);
                }

                if (source != null && source.DataContext != null)
                {
                    var target = source.DataContext;
                    var method = ActionMessage.GetTargetMethod(context.Message, target);

                    if (method != null)
                    {
                        context.Target = target;
                        context.Method = method;
                        context.View = source;
                    }
                }
            };
        }

        protected override void Configure()
        {
            container = new WindsorContainer();

            container.AddFacility<EventRegistrationFacility>();
            container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel));

            container.Register(
                Component.For<IWindowManager>().ImplementedBy<WindowManager>().LifestyleSingleton(),
                Component.For<IEventAggregator>().ImplementedBy<EventAggregator>().LifestyleSingleton(),
                Classes.FromThisAssembly().BasedOn(typeof(IScreen)).WithServiceDefaultInterfaces().LifestyleSingleton()
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