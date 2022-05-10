using Zirve.NotificationEngine.Client.Enumerations;
using System;

namespace Zirve.NotificationEngine.Core.Domain.Models
{
    public class NotificationQueueArchive : EntityBase<long>
    {
        public virtual Guid TrackId { get; protected set; }
        public virtual NotificationPublishType NotificationPublishType { get; protected set; }
        public virtual int RetryCount { get; protected set; }
        public virtual DateTime? LastTryDateTime { get; protected set; }

        public virtual string MessageTargetIdentifier { get; protected set; }
        public virtual string CCRecipients { get; protected set; }
        public virtual string BCCRecipients { get; protected set; }
        public virtual string MessageSubject { get; protected set; }
        public virtual string Message { get; protected set; }
        public virtual string RecipientInfo { get; protected set; }
        public virtual string SenderAddress { get; protected set; }
        public virtual string ReplyToAddress { get; set; }
        protected NotificationQueueArchive()
        {
        }

        public NotificationQueueArchive(
            NotificationQueue notificationQueue)
        {
            this.Id = notificationQueue.Id;
            this.CreatedOn = notificationQueue.CreatedOn;
            this.TrackId = notificationQueue.TrackId;
            this.NotificationPublishType = notificationQueue.NotificationPublishType;
            this.RetryCount = notificationQueue.RetryCount;
            this.LastTryDateTime = notificationQueue.LastTryDateTime;
            this.MessageTargetIdentifier = notificationQueue.MessageTargetIdentifier;
            this.CCRecipients = notificationQueue.CCRecipients;
            this.BCCRecipients = notificationQueue.BCCRecipients;
            this.MessageSubject = notificationQueue.MessageSubject;
            this.Message = notificationQueue.GetMessageBody();
            this.RecipientInfo = notificationQueue.GetRecipientInfo();
            this.SenderAddress = notificationQueue.SenderAddress;
            this.ReplyToAddress = ReplyToAddress;
        }
    }
}