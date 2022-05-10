using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.DynamicProxy;

namespace Zirve.NotificationEngine.Host.WindsorInstallers
{
    public class InterceptorInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Classes.FromAssemblyInDirectory(new AssemblyFilter(string.Empty))
                .BasedOn<IInterceptor>()
                .WithService.AllInterfaces()
                .LifestylePerThread());
        }
    }
}
