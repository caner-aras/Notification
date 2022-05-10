using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace Zirve.NotificationEngine.Core.Domain.Mapping.Conventions
{
    public class PropertyConvention : IPropertyConvention
    {
        public void Apply(IPropertyInstance instance)
        {
            instance.Not.Nullable();
        }
    }
}
