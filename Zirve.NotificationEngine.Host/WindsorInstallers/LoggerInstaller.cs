using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.MicroKernel.SubSystems.Configuration;
using PayFlex.Collection.Infrastructure;
using PayFlex.Collection.Infrastructure.Logging.Log4net;
using System.IO;

namespace Zirve.NotificationEngine.Host.WindsorInstallers
{
    public class LoggerInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<ILogger>()
                    .ImplementedBy<Log4netLogger>()
                    .LifeStyle.PerThread);

            string logFilePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, string.Empty, "log4net.config");
            Log4netLogger.Init(logFilePath);

            ILogger logger = container.Resolve<ILogger>();
        }
    }
}
