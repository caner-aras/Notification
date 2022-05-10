using FluentNHibernate.Mapping;
using Zirve.NotificationEngine.Client.Enumerations;
using Zirve.NotificationEngine.Core.Domain.Models;

namespace Zirve.NotificationEngine.Core.Domain.Mappings
{
    public class NotificationQueueEmailPublishTypeMap : ClassMap<NotificationQueueEmailPublishType>
    {
        public NotificationQueueEmailPublishTypeMap()
        {
            Table("NotificationQueueEmailPublishType");

            Id(x => x.Id).GeneratedBy.Identity();

            References(x => x.NotificationQueue).Column("NotificationQueueId")
                .Index("IX_NotificationQueueEmailPublishType");

            Map(x => x.EmailPublishType)
                .CustomType<EmailPublishType>();
        }
    }
}
