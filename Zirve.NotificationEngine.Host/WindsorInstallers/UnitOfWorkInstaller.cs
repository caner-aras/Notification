using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using PayFlex.Collection.Infrastructure;
using PayFlex.Collection.Infrastructure.Persistence.NHibernate;
using System;
using System.Configuration;
using System.IO;

namespace Zirve.NotificationEngine.Host.WindsorInstallers
{
    public class UnitOfWorkInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IUnitOfWorkFactory>()
                .ImplementedBy<UnitOfWorkFactory>()
                .LifeStyle.Singleton);

            IUnitOfWorkFactory unitOfWorkFactory = container.Kernel.Resolve<IUnitOfWorkFactory>();
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Nhibernate.config");
            unitOfWorkFactory.Initialize(path);

            if (ConfigurationManager.AppSettings["IsSetupDatabase"] == "1")
            {
                unitOfWorkFactory.BuildSchemaByDroping();
            }

            container.Register(
                Component.For(typeof(IRepository<>)).ImplementedBy(typeof(Repository<>)).LifestyleTransient());
        }
    }
}
