using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace Zirve.NotificationEngine.Core.Domain.Mapping.Conventions
{
    public class ReferenceConvention : IReferenceConvention
    {
        public void Apply(IManyToOneInstance instance)
        {
            instance.Not.Nullable();
        }
    }
}
