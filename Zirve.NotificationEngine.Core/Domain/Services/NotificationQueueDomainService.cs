using Zirve.NotificationEngine.Client.Enumerations;
using Zirve.NotificationEngine.Core.Domain.DomainObject;
using Zirve.NotificationEngine.Core.Domain.Models;
using Zirve.NotificationEngine.Core.Domain.Repositories;
using System;
using System.Collections.Generic;

namespace Zirve.NotificationEngine.Core.Domain.Services
{
    public class NotificationQueueDomainService : IDomainService
    {
        private readonly NotificationQueueRepository notificationQueueRepository;
        private readonly NotificationQueueArchiveRepository notificationQueueArchiveRepository;

        public NotificationQueueDomainService(
            NotificationQueueRepository notificationQueueRepository,
            NotificationQueueArchiveRepository notificationQueueArchiveRepository)
        {
            this.notificationQueueRepository = notificationQueueRepository;
            this.notificationQueueArchiveRepository = notificationQueueArchiveRepository;
        }

        public NotificationQueue CreateNotificationQueue(
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
            string replyToAddress
            )
        {
            NotificationQueue notificationQueue = new NotificationQueue(
                trackId,
                notificationPublishType,
                messageTargetIdentifier,
                ccRecipients,
                bccRecipients,
                messageSubject,
                message,
                attachments,
                parameters,
                recipients,
                emailPublishType,
                externalId,
                senderAddress,
                replyToAddress
                );

            return this.notificationQueueRepository.Add(notificationQueue);
        }

        public void Update(NotificationQueue notificationQueue)
        {
            this.notificationQueueRepository.Update(notificationQueue);
        }

        public ICollection<long> GetPendingQueue(int itemCount, int maxRetryCount)
        {
            return this.notificationQueueRepository.GetPendingNotificationQueue(itemCount, maxRetryCount);
        }

        public void CompleteNotification(NotificationQueue notificationQueue)
        {
            this.notificationQueueArchiveRepository.Add(new NotificationQueueArchive(notificationQueue));
            this.notificationQueueRepository.Delete(notificationQueue);
        }

        public NotificationQueue GetNotification(long id)
        {
            return this.notificationQueueRepository.GetSingle(id);
        }
    }
}
