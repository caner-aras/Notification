using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.MicroKernel.SubSystems.Configuration;
using Zirve.NotificationEngine.Core.Engines;

namespace Zirve.NotificationEngine.Host.WindsorInstallers
{
    public class QueueEngineInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<QueueEngine>()
                    .ImplementedBy<QueueEngine>()
                    .LifeStyle.Singleton);
        }
    }
}
