using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace Zirve.NotificationEngine.Core.Domain.Mapping.Conventions
{
    public class HasManyToManyCascadeConvention : IHasManyToManyConvention
    {
        public void Apply(IManyToManyCollectionInstance instance)
        {
            instance.AsSet();
            instance.LazyLoad();
            instance.Cascade.AllDeleteOrphan();
            instance.Generic();
        }
    }
}
