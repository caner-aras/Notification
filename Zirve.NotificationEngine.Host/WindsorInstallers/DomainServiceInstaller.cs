using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.MicroKernel.SubSystems.Configuration;
using Zirve.NotificationEngine.Core.Domain.Services;

namespace Zirve.NotificationEngine.Host.WindsorInstallers
{
    public class DomainServiceInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Classes.FromAssemblyInDirectory(new AssemblyFilter(""))
                .BasedOn(typeof(IDomainService))
                .WithService.Self()
                .LifestyleTransient());
        }
    }
}
