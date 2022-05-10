using FluentNHibernate.Mapping;
using Zirve.NotificationEngine.Core.Domain.Models;

namespace Zirve.NotificationEngine.Core.Domain.Mappings
{
    public partial class NotificationQueueRecipientMap : ClassMap<NotificationQueueRecipient>
    {
        public NotificationQueueRecipientMap()
        {
            Table("NotificationQueueRecipient");

            Id(x => x.Id).GeneratedBy.Identity();

            References(x => x.NotificationQueue).Column("NotificationQueueId")
                .Index("IX_NotificationQueueRecipient");

            Map(x => x.Name)
                    .Length(100);

            Map(x => x.TargetAddress)
                .Length(100);

            HasMany(x => x.MessageVariables)
                .KeyColumn("NotificationQueueRecipientId");
        }
    }
}
