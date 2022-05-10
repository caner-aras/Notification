using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Facilities.TypedFactory;
using Zirve.NotificationEngine.Core.NotificationPublisher;

namespace Zirve.NotificationEngine.Host.WindsorInstallers
{
    public class NotificationPublisherInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Classes.FromAssemblyInDirectory(new AssemblyFilter(""))
                .BasedOn<INotificationPublisher>()
                .WithService.AllInterfaces()
                .LifestyleTransient(),
                Component.For<NotificationPublisherFactory>().LifestyleSingleton(),
                Component.For<INotificationPublisherTypedFactory>().AsFactory().LifeStyle.Singleton);
        }
    }
}
