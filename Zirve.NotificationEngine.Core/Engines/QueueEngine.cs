using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Zirve.NotificationEngine.Core.Domain.Services;
using Zirve.NotificationEngine.Core.Domain.Models;
using Zirve.NotificationEngine.Core.Constants;
using Zirve.NotificationEngine.Core.NotificationPublisher;
using Zirve.NotificationEngine.Client.Enumerations;
using PayFlex.Collection.Infrastructure;

namespace Zirve.NotificationEngine.Core.Engines
{
    public class QueueEngine
    {
        private readonly ILogger logger;
        private readonly NotificationQueueDomainService notificationQueueDomainService;
        private readonly NotificationPublisherFactory notificationPublisherFactory;
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly Timer timer;

        public QueueEngine(ILogger logger, 
            NotificationQueueDomainService notificationQueueDomainService,
            NotificationPublisherFactory notificationPublisherFactory,
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            this.logger = logger;
            this.notificationQueueDomainService = notificationQueueDomainService;
            this.notificationPublisherFactory = notificationPublisherFactory;
            this.unitOfWorkFactory = unitOfWorkFactory;

            this.timer = new Timer(double.Parse(ConfigurationManager.AppSettings["QueueIntervalMilliseconds"]));
            this.timer.Elapsed += timer_Elapsed;
        }

        public void Start()
        {
            timer.Start();
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (ConfigurationManager.AppSettings["HoldTimerOnProcess"] == "1")
            {
                timer.Stop();
            }

            try
            {
                this.InternalProcessQueue();
            }
            catch (Exception exception)
            {
                this.logger.Log("Queue process error", LogType.Error, exception);
            }

            if (ConfigurationManager.AppSettings["HoldTimerOnProcess"] == "1")
            {
                timer.Start();
            }
        }

        private void InternalProcessQueue()
        {
            ICollection<long> notifications;

            using (var uow = this.unitOfWorkFactory.Create())
            {
                uow.Begin();

                notifications = this.notificationQueueDomainService.GetPendingQueue(int.Parse(
                    ConfigurationManager.AppSettings["MaxItemsInPerRequest"]),
                    int.Parse(ConfigurationManager.AppSettings["MaxRetryCount"]));

                uow.Commit();
            }


            this.logger.Log(string.Format("({0}) notification found and start processing...", notifications.Count), LogType.Information);

            Parallel.ForEach(notifications, notification =>
            {

                this.ProcessItem(notification);
            });

            this.logger.Log(string.Format("({0}) notification processed.", notifications.Count), LogType.Information);

        }

        public void ProcessItem(long queueId)
        {
            this.logger.Log(string.Format("Notification ({0}) started processing...", queueId), LogType.Information);

            INotificationPublisher publisher = null;
            NotificationQueue queue = null;
            string senderAddress = string.Empty;
            string replyToAddress = string.Empty;
            string messageBody = string.Empty;
            List<NotificationQueueAttachment> attachmentList = new List<NotificationQueueAttachment>();
            List<NotificationQueueParameter> parameterList = new List<NotificationQueueParameter>();
            List<NotificationQueueRecipient> recipientList = new List<NotificationQueueRecipient>();
            NotificationQueueEmailPublishType emailPublishType = null;
            string externalId = string.Empty;
            using (var uow = this.unitOfWorkFactory.Create())
            {
                uow.Begin();

                queue = this.notificationQueueDomainService.GetNotification(queueId);

                publisher = this.notificationPublisherFactory.GetNotificationPublisher(queue.NotificationPublishType);

                messageBody = queue.GetMessageBody();

                attachmentList = queue.Attachments.ToList();

                parameterList = queue.Parameters.ToList();

                recipientList = queue.GetRecipientObjectsFromJson();

                emailPublishType = queue.EmailPublishType;

                externalId = queue.ExternalId;

                senderAddress = queue.SenderAddress;

                replyToAddress = queue.ReplyToAddress;

                uow.Commit();
            }

            PublishRequest publishRequest = new PublishRequest()
            {
                Message = messageBody,
                MessageSubject = queue.MessageSubject,
                MessageTargetIdentifier = queue.MessageTargetIdentifier,
                CCRecipients = queue.CCRecipients,
                BCCRecipients = queue.BCCRecipients,
                Attachments = attachmentList,
                Parameters = parameterList,
                Recipients = recipientList,
                EmailPublishType = emailPublishType == null ? EmailPublishType.Smtp : emailPublishType.EmailPublishType,
                ExternalId = externalId,
                SenderAddress = senderAddress,
                ReplyToAddress=replyToAddress
            };

            PublishResponse publishResponse = null;

            try
            {
                publishResponse = publisher.Publish(publishRequest);
            }
            catch (Exception exception)
            {
                publishResponse = new PublishResponse()
                {
                    ResponseCode = ResponseCode.Error
                };

                this.logger.Log(string.Format("Notification ({0}) failed. ResponseCode: {1}, Exception: {2}", queueId, publishResponse.ResponseCode, exception.ToString()), LogType.Information);
            }

            using (var uow = this.unitOfWorkFactory.Create())
            {
                uow.Begin();

                if (publishResponse.ResponseCode == ResponseCode.Success)
                {
                    this.notificationQueueDomainService.CompleteNotification(queue);
                    this.logger.Log(string.Format("Notification ({0}) completed succesfully.", queueId), LogType.Information);
                }
                else
                {
                    queue.IncreaseTryCount();
                    this.notificationQueueDomainService.Update(queue);
                }

                uow.Commit();
            }
        }
    }
}
