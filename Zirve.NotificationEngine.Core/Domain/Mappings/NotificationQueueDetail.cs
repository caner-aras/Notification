using FluentNHibernate.Mapping;
using Zirve.NotificationEngine.Core.Domain.Models;

namespace Zirve.NotificationEngine.Core.Domain.Mappings
{
    public partial class NotificationQueueDetailMap : ClassMap<NotificationQueueDetail>
    {
        public NotificationQueueDetailMap()
        {
            Table("NotificationQueueDetail");

            Id(x => x.Id).GeneratedBy.Identity();

            References(x => x.NotificationQueue).Column("NotificationQueueId")
                .Index("IX_NotificationDetail");

            Map(x => x.Message).CustomType("StringClob").CustomSqlType("nvarchar(max)");

            Map(x => x.RecipientInfo).CustomType("StringClob").CustomSqlType("nvarchar(max)").Nullable();
        }
    }
}
