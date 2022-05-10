using Zirve.NotificationEngine.Client.Enumerations;
using System.Collections.Generic;

namespace Zirve.NotificationEngine.Client.DTO
{
    public class NotificationInquiryRequest
    {
        public string ExternalId { get; set; }

        public string MessageId { get; set; }

        public EmailPublishType EmailPublishType { get; set; }

        public NotificationPublishType NotificationPublishType { get; set; }

        public SmsPublishType SmsPublishType { get; set; }

        public ICollection<Parameter> Parameters { get; set; }
    }
}
