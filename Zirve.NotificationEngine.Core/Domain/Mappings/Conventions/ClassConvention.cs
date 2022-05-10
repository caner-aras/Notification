using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using System.Configuration;

namespace Zirve.NotificationEngine.Core.Domain.Mapping.Conventions
{
    public class ClassConvention : IClassConvention
    {
        public void Apply(IClassInstance instance)
        {
            instance.Schema(ConfigurationManager.AppSettings["DatabaseSchemeName"]);

            instance.LazyLoad();
        }
    }
}
