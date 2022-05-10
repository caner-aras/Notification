using Zirve.NotificationEngine.Client.Enumerations;
using System;
using System.Collections.Generic;

namespace Zirve.NotificationEngine.Client.DTO
{
    public class EnqueueRequest
    {
        public Guid TrackId { get; set; }

        public NotificationPublishType NotificationPublishType { get; set; }

        public string MessageTargetIdentifier { get; set; }

        public string CCRecipients { get; set; }

        public string BCCRecipients { get; set; }

        public string Message { get; set; }

        public string MessageSubject { get; set; }

        public ICollection<Recipient> Recipients { get; set; }

        public ICollection<Attachment> Attachments { get; set; }

        public EmailPublishType EmailPublishType { get; set; }

        public ICollection<Parameter> Parameters { get; set; }

        public NotificationWorkingType NotificationWorkingType { get; set; }

        public string ExternalId { get; set; }

        public string SenderAddress { get; set; }
        public string ReplyToAddress { get; set; }

        public SmsPublishType SmsPublishType { get; set; }

    }
}
