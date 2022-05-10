using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.MicroKernel.SubSystems.Configuration;
using Zirve.NotificationEngine.Core.Domain.Repositories;

namespace Zirve.NotificationEngine.Host.WindsorInstallers
{
    public class RepositoryInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Classes.FromAssemblyInDirectory(new AssemblyFilter(""))
                .BasedOn(typeof(RepositoryBase<>))
                .WithService.DefaultInterfaces()
                .LifestyleTransient());
        }
    }
}
