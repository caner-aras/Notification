using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Facilities.TypedFactory;
using Zirve.NotificationEngine.Core.NotificationPublisher.Implementations.EmailPublisher;

namespace Zirve.NotificationEngine.Host.WindsorInstallers
{
    public class EmailNotificationPublisherInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Classes.FromAssemblyInDirectory(new AssemblyFilter(""))
                .BasedOn<IEmailNotificationPublisher>()
                .WithService.AllInterfaces()
                .LifestyleTransient(),
                Component.For<EmailNotificationPublisherFactory>().LifestyleSingleton(),
                Component.For<IEmailNotificationPublisherTypedFactory>().AsFactory().LifeStyle.Singleton);
        }
    }
}
