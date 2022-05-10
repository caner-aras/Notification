using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Zirve.NotificationEngine.Client.Enumerations;
using Zirve.NotificationEngine.Core.Domain.DomainObject;

namespace Zirve.NotificationEngine.Core.Domain.Models
{
    public class NotificationQueue : EntityBase<long>
    {
        public virtual bool IsProcessing { get; protected set; }
        public virtual Guid TrackId { get; protected set; }
        public virtual NotificationPublishType NotificationPublishType { get; protected set; }
        public virtual int RetryCount { get; protected set; }
        public virtual DateTime? LastTryDateTime { get; protected set; }
        public virtual string MessageTargetIdentifier { get; protected set; }
        public virtual string CCRecipients { get; protected set; }
        public virtual string BCCRecipients { get; protected set; }
        public virtual string MessageSubject { get; protected set; }
        public virtual ICollection<NotificationQueueDetail> NotificationQueueDetail { get; protected set; }
        public virtual ICollection<NotificationQueueAttachment> Attachments { get; protected set; }
        public virtual ICollection<NotificationQueueParameter> Parameters { get; protected set; }
        public virtual NotificationQueueEmailPublishType EmailPublishType { get; protected set; }
        public virtual string ExternalId { get; protected set; }
        public virtual string SenderAddress { get; set; } //burada migration yapılmalıdır
        public virtual string ReplyToAddress { get; protected set; }
        protected NotificationQueue()
        {
            this.NotificationQueueDetail = new HashSet<NotificationQueueDetail>();
        }

        public NotificationQueue(
            Guid trackId,
            NotificationPublishType notificationPublishType,
            string messageTargetIdentifier,
            string ccRecipients,
            string bccRecipients,
            string messageSubject,
            string message,
            ICollection<AttachmentObject> attachments,
            ICollection<ParameterObject> parameters,
            ICollection<RecipientObject> recipients,
            EmailPublishType emailPublishType,
            string externalId,
            string senderAddress,
            string replytoAddress
            )
            : this()
        {
            this.TrackId = trackId;
            this.NotificationPublishType = notificationPublishType;
            this.MessageTargetIdentifier = messageTargetIdentifier;
            this.CCRecipients = ccRecipients;
            this.BCCRecipients = bccRecipients;
            this.MessageSubject = messageSubject;
            
            var recipientJson = this.GetRecipientJsonFromObjects(recipients);
            this.NotificationQueueDetail.Add(new NotificationQueueDetail(this, message, recipientJson));

            this.EmailPublishType = new NotificationQueueEmailPublishType(this, emailPublishType);
            if (attachments != null)
            {
                this.Attachments = attachments
                        .Select(x => new NotificationQueueAttachment(this, x.FileName, x.Body))
                        .ToList();
            }
            if (parameters != null)
            {
                this.Parameters = parameters
                        .Select(x => new NotificationQueueParameter(this, x.Name, x.Value))
                        .ToList();
            }
            this.SenderAddress = senderAddress;
            this.ReplyToAddress = replytoAddress;
            this.ExternalId = externalId;
        }

        public virtual void IncreaseTryCount()
        {
            this.RetryCount += 1;
            this.LastTryDateTime = DateTime.Now;
            this.IsProcessing = false;
        }

        public virtual void DoIsProcessing()
        {
            this.LastTryDateTime = DateTime.Now;
            this.IsProcessing = true;
        }

        public virtual string GetMessageBody()
        {
            return this.NotificationQueueDetail.Single().Message;
        }

        public virtual string GetRecipientInfo()
        {
            return this.NotificationQueueDetail.Single().RecipientInfo;
        }

        public virtual List<NotificationQueueRecipient> GetRecipientObjectsFromJson()
        {
            List<NotificationQueueRecipient> recipients = null;

            var recipientJson = this.NotificationQueueDetail.Single().RecipientInfo;

            if (!string.IsNullOrWhiteSpace(recipientJson))
            {
                var recipientObjects = JsonConvert.DeserializeObject<ICollection<RecipientObject>>(recipientJson);
                if (recipientObjects != null)
                {
                    recipients = recipientObjects.Select(x => new NotificationQueueRecipient(this, x.Name, x.TargetAddress, x.MessageVariableObjects))
                                                 .ToList();
                }
            }

            return recipients;
        }

        public virtual string GetRecipientJsonFromObjects(ICollection<RecipientObject> recipients)
        {
            var recipientJson = JsonConvert.SerializeObject(recipients);
            return recipientJson;
        }
    }
}
