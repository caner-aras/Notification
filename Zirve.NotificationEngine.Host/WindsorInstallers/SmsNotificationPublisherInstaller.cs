using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Facilities.TypedFactory;
using Zirve.NotificationEngine.Core.NotificationPublisher.Implementations.SmsPublisher;

namespace Zirve.NotificationEngine.Host.WindsorInstallers
{
    public class SmsNotificationPublisherInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Classes.FromAssemblyInDirectory(new AssemblyFilter(""))
                .BasedOn<ISmsNotificationPublisher>()
                .WithService.AllInterfaces()
                .LifestyleTransient(),
                Component.For<SmsNotificationPublisherFactory>().LifestyleSingleton(),
                Component.For<ISmsNotificationPublisherTypedFactory>().AsFactory().LifeStyle.Singleton);
        }
    }
}
