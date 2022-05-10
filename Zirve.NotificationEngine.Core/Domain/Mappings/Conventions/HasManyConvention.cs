using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace Zirve.NotificationEngine.Core.Domain.Mapping.Conventions
{
    public class HasManyCacheConvention : IHasManyConvention
    {
        public void Apply(IOneToManyCollectionInstance instance)
        {
            //instance.Cache.ReadWrite();
        }
    }

    public class HasManyCascadeConvention : IHasManyConvention
    {
        public void Apply(IOneToManyCollectionInstance instance)
        {
            instance.AsSet();
            instance.Inverse();
            instance.LazyLoad();
            instance.Cascade.AllDeleteOrphan();
            instance.Generic();
        }
    }
}
