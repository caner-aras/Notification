using Zirve.NotificationEngine.Core.Domain.Models;
using System.Collections.Generic;
using Zirve.NotificationEngine.Client.Enumerations;

namespace Zirve.NotificationEngine.Core.NotificationPublisher
{
    public class InquiryRequest
    {
        public string ExternalId { get; set; }

        public string MessageId { get; set; }

        public EmailPublishType EmailPublishType { get; set; }

        public SmsPublishType SmsPublishType { get; set; }

        public List<NotificationQueueParameter> Parameters { get; set; }
    }
}
