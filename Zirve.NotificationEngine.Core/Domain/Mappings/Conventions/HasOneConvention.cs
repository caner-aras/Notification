using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace Zirve.NotificationEngine.Core.Domain.Mapping.Conventions
{
    public class HasOneConvention : IHasOneConvention
    {
        public void Apply(IOneToOneInstance instance)
        {
            instance.LazyLoad();
            instance.Cascade.All();
        }
    }
}
