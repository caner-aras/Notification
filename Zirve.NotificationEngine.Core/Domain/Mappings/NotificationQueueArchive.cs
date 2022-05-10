using FluentNHibernate.Mapping;
using Zirve.NotificationEngine.Core.Domain.Models;
using Zirve.NotificationEngine.Client.Enumerations;

namespace Zirve.NotificationEngine.Core.Domain.Mappings
{
    public partial class NotificationQueueArchiveMap : ClassMap<NotificationQueueArchive>
    {
        public NotificationQueueArchiveMap()
        {
            Table("NotificationQueueArchive");

            Id(x => x.Id).GeneratedBy.Assigned();

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

            Map(x => x.Message)
                .CustomType("StringClob")
                .CustomSqlType("nvarchar(max)");

            Map(x => x.RecipientInfo)
                .CustomType("StringClob")
                .CustomSqlType("nvarchar(max)")
                .Nullable();

            Map(x => x.SenderAddress).Length(1000).Nullable();

            Map(x => x.ReplyToAddress).Length(1000).Nullable();
        }
    }
}
