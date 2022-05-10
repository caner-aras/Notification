using Zirve.NotificationEngine.Core.Domain.Models;
using System.Collections.Generic;
using Zirve.NotificationEngine.Client.Enumerations;

namespace Zirve.NotificationEngine.Core.NotificationPublisher
{
    public class PublishRequest
    {
        public string MessageTargetIdentifier { get; set; }
        public string CCRecipients { get; set; }
        public string BCCRecipients { get; set; }
        public string MessageSubject { get; set; }
        public string Message { get; set; }
        public List<NotificationQueueRecipient> Recipients { get; set; }
        public List<NotificationQueueAttachment> Attachments { get; set; }
        public List<NotificationQueueParameter> Parameters { get; set; }
        public EmailPublishType EmailPublishType { get; set; }
        public string ExternalId { get; set; }
        public string SenderAddress { get; set; }
        public string ReplyToAddress { get; set; }

        public SmsPublishType SmsPublishType { get; set; }

    }
}
