using Castle.Facilities.TypedFactory;
using Castle.Facilities.WcfIntegration;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Zirve.NotificationEngine.Core.Engines;
using System;
using System.ServiceModel.Description;

namespace Zirve.NotificationEngine.Host.Infrascructure
{
    public class Bootstrapper
    {
        public Bootstrapper()
        {
            IWindsorContainer container = new WindsorContainer();

            container.AddFacility<TypedFactoryFacility>().AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero);

            ServiceMetadataBehavior metadata = new ServiceMetadataBehavior();

            ServiceDebugBehavior returnFaults = new ServiceDebugBehavior() { IncludeExceptionDetailInFaults = true };

            container.Register(
                Component.For<IServiceBehavior>().Instance(metadata),
                Component.For<IServiceBehavior>().Instance(returnFaults));

            container.Install(FromAssembly.This());

            QueueEngine queueEngine = container.Resolve<QueueEngine>();
            queueEngine.Start();
        }
    }
}
