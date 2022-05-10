using FluentNHibernate.Mapping;
using NHibernate.Type;
using Zirve.NotificationEngine.Core.Domain.Models;

namespace Zirve.NotificationEngine.Core.Domain.Mappings
{
    public partial class NotificationQueueAttachmentMap : ClassMap<NotificationQueueAttachment>
    {
        public NotificationQueueAttachmentMap()
        {
            Table("NotificationQueueAttachment");

            Id(x => x.Id).GeneratedBy.Identity();

            References(x => x.NotificationQueue).Column("NotificationQueueId")
                .Index("IX_NotificationAttachment");

            Map(x => x.Body)
                .CustomType<BinaryBlobType>();

            Map(x => x.FileName)
                .Length(500);
        }
    }
}
