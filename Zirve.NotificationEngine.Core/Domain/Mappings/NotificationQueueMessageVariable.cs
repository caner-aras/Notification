using FluentNHibernate.Mapping;
using Zirve.NotificationEngine.Core.Domain.Models;

namespace Zirve.NotificationEngine.Core.Domain.Mappings
{
    public partial class NotificationQueueMessageVariableMap : ClassMap<NotificationQueueMessageVariable>
    {
        public NotificationQueueMessageVariableMap()
        {
            Table("NotificationQueueMessageVariable");

            Id(x => x.Id).GeneratedBy.Identity();

            References(x => x.NotificationQueueRecipient).Column("NotificationQueueRecipientId")
                .Index("IX_NotificationQueueMessageVariable");

            Map(x => x.Name)
                .Length(100);

            Map(x => x.Value)
                .Length(500);
        }
    }
}
