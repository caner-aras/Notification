using FluentNHibernate.Mapping;
using Zirve.NotificationEngine.Client.Enumerations;
using Zirve.NotificationEngine.Core.Domain.Models;

namespace Zirve.NotificationEngine.Core.Domain.Mappings
{
    public partial class NotificationQueueMap : ClassMap<NotificationQueue>
    {
        public NotificationQueueMap()
        {
            Table("NotificationQueue");

            Id(x => x.Id).GeneratedBy.Identity();

            Map(x => x.IsProcessing);

            Map(x => x.CreatedOn);

            Map(x => x.TrackId);

            Map(x => x.NotificationPublishType)
                .CustomType<NotificationPublishType>();

            Map(x => x.RetryCount);

            Map(x => x.LastTryDateTime)
                .Nullable();

            Map(x => x.MessageTargetIdentifier)
                .Length(1000)
                .Nullable();

            Map(x => x.CCRecipients)
                .Length(1000)
                .Nullable();
            
            Map(x => x.BCCRecipients)
                .Length(1000)
                .Nullable();

            Map(x => x.MessageSubject)
                .Length(200);


            HasOne(x => x.EmailPublishType)
                .PropertyRef(e => e.NotificationQueue);
            //.ForeignKey("NotificationQueueId");

            HasMany(x => x.NotificationQueueDetail)
                .KeyColumn("NotificationQueueId");

            HasMany(x => x.Attachments)
                .KeyColumn("NotificationQueueId");

            HasMany(x => x.Parameters)
                .KeyColumn("NotificationQueueId");

            Map(x => x.ExternalId).Length(40).Nullable();

            Map(x => x.SenderAddress).Length(1000).Nullable();

            Map(x => x.ReplyToAddress).Length(1000).Nullable();
        }
    }
}
