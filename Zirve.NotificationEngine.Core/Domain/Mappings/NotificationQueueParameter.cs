﻿using FluentNHibernate.Mapping;
using Zirve.NotificationEngine.Core.Domain.Models;

namespace Zirve.NotificationEngine.Core.Domain.Mappings
{
    public partial class NotificationQueueParameterMap : ClassMap<NotificationQueueParameter>
    {
        public NotificationQueueParameterMap()
        {
            Table("NotificationQueueParameter");

            Id(x => x.Id).GeneratedBy.Identity();

            References(x => x.NotificationQueue).Column("NotificationQueueId")
                .Index("IX_NotificationQueueParameter");

            Map(x => x.Name)
                    .Length(100);

            Map(x => x.Value)
                .Length(500);
        }
    }
}
