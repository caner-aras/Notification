using System;
using System.Collections.Generic;
using System.Linq;
using PayFlex.Collection.Infrastructure;
using Zirve.NotificationEngine.Core.Domain.Services;
using Zirve.NotificationEngine.Client.ServiceInterfaces;
using Zirve.NotificationEngine.Client.DTO.NotificationService.Enqueue;
using Zirve.NotificationEngine.Core.Domain.Models;
using Zirve.NotificationEngine.Core.Domain.DomainObject;
using Zirve.NotificationEngine.Client.Enumerations;
using Zirve.NotificationEngine.Core.NotificationPublisher;
using Zirve.NotificationEngine.Client.DTO.NotificationService.NotificationInquiry;

namespace Zirve.NotificationEngine.Host.Services
{
    public class NotificationQueueService : INotificationQueueService
    {
        private readonly ILogger logger;
        private readonly NotificationQueueDomainService notificationQueueDomainService;
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly NotificationPublisherFactory notificationPublisherFactory;

        public NotificationQueueService(
            ILogger logger,
            NotificationQueueDomainService notificationQueueDomainService,
            NotificationPublisherFactory notificationPublisherFactory,
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            this.logger = logger;
            this.notificationQueueDomainService = notificationQueueDomainService;
            this.unitOfWorkFactory = unitOfWorkFactory;
            this.notificationPublisherFactory = notificationPublisherFactory;
        }

        public EnqueueResponseDTO Enqueue(EnqueueRequestDTO request)
        {
            var resp = new EnqueueResponseDTO();
            if (request.NotificationWorkingType == NotificationWorkingType.TryForwardAndStore || request.NotificationWorkingType == NotificationWorkingType.Forward)
            {
                INotificationPublisher publisher = this.notificationPublisherFactory.GetNotificationPublisher(request.NotificationPublishType);
                List<NotificationQueueAttachment> attachments = null;
                List<NotificationQueueParameter> parameters = null;
                List<NotificationQueueRecipient> recipients = null;

                if (request.Attachments != null)
                {
                    attachments = request.Attachments
                                         .Select(x => new NotificationQueueAttachment(x.FileName, x.Body))
                                         .ToList();
                }

                if (request.Parameters != null)
                {
                    parameters = request.Parameters
                                        .Select(x => new NotificationQueueParameter(x.Name, x.Value))
                                        .ToList();
                }

                if (request.Recipients != null)
                {
                    recipients = request.Recipients
                                        .Select(x => new NotificationQueueRecipient(x.Name, x.TargetAddress,
                                                                                    x.MessageVariables == null ? null : x.MessageVariables.Select(mv => new MessageVariableObject
                                                                                    {
                                                                                        Name = mv.Name,
                                                                                        Value = mv.Value
                                                                                    }).ToList()))
                                        .ToList();
                }
                else
                {
                    this.logger.Log(string.Format("Enqueue Request Recipents are null. TrackId :{0} , External Id : {1}", request.TrackId, request.ExternalId), LogType.Debug);
                }

                PublishRequest publishRequest = new PublishRequest
                {
                    Message = request.Message,
                    MessageSubject = request.MessageSubject,
                    MessageTargetIdentifier = request.MessageTargetIdentifier,
                    CCRecipients = request.CCRecipients,
                    BCCRecipients = request.BCCRecipients,
                    Recipients = recipients,
                    Attachments = attachments,
                    Parameters = parameters,
                    EmailPublishType = request.EmailPublishType,
                    ExternalId = request.ExternalId,
                    SenderAddress = request.SenderAddress,
                    ReplyToAddress = request.ReplyToAddress
                };

                try
                {
                    var publishResponse = publisher.Publish(publishRequest);
                    if (publishResponse !=null)
                    {
                        resp.ResponseCode = publishResponse.ResponseCode;
                        resp.Explanation = publishResponse.Explanation;
                        resp.MessageId = publishResponse.MessageId;
                        resp.SmartMessageReturnCode = publishResponse.SmartMessageReturnCode;
                    }
                }
                catch (Exception ex)
                {
                    if (request.NotificationWorkingType == NotificationWorkingType.TryForwardAndStore)
                        EnqueueNotificationQueue(request);
                    else
                    {
                        resp.ResponseCode = "0001";
                        this.logger.Log(string.Format("Publish Notification Exeption. TrackId :{0} , External Id : {1}", request.TrackId, request.ExternalId), LogType.Debug, ex);
                        throw (ex);
                    }
                }
            }
            else
            {
                EnqueueNotificationQueue(request);
            }

            return resp;
        }

        private void EnqueueNotificationQueue(EnqueueRequestDTO request)
        {
            using (var uow = this.unitOfWorkFactory.Create())
            {
                uow.Begin();

                ICollection<AttachmentObject> attachments = null;
                ICollection<ParameterObject> parameters = null;
                ICollection<RecipientObject> recipients = null;

                if (request.Attachments != null)
                {
                    attachments = request.Attachments
                        .Select(x => new AttachmentObject()
                        {
                            FileName = x.FileName,
                            Body = x.Body
                        })
                        .ToList();
                }

                if (request.Parameters != null)
                {
                    parameters = request.Parameters
                        .Select(x => new ParameterObject()
                        {
                            Name = x.Name,
                            Value = x.Value
                        })
                        .ToList();
                }

                if (request.Recipients != null)
                {
                    recipients = request.Recipients.Select(x => new RecipientObject()
                    {
                        Name = x.Name,
                        TargetAddress = x.TargetAddress,
                        MessageVariableObjects = x.MessageVariables == null ? null : x.MessageVariables.Select(mv => new MessageVariableObject()
                        {
                            Name = mv.Name,
                            Value = mv.Value
                        }).ToList()
                    })
                        .ToList();
                }
                this.notificationQueueDomainService.CreateNotificationQueue(
                    request.TrackId,
                    request.NotificationPublishType,
                    request.MessageTargetIdentifier,
                    request.CCRecipients,
                    request.BCCRecipients,
                    request.MessageSubject,
                    request.Message,
                    attachments,
                    parameters,
                    recipients,
                    request.EmailPublishType,
                    request.ExternalId,
                    request.SenderAddress,
                    request.ReplyToAddress
                    );

                uow.Commit();
            }
        }




        public NotificationInquiryResponseDTO NotificationInquiry(NotificationInquiryRequestDTO request)
        {
            
            if (request.NotificationPublishType == 0) //E-fatura ortamında gerekli olduğu için yazıldı.
            {
                request.NotificationPublishType = NotificationPublishType.Email;
            }
            INotificationPublisher publisher = this.notificationPublisherFactory.GetNotificationPublisher(request.NotificationPublishType);
            InquiryRequest inquryRequest = null;

            switch (request.NotificationPublishType)
            {
                case NotificationPublishType.Email:
                    inquryRequest = new InquiryRequest()
                    {
                        EmailPublishType = request.EmailPublishType,
                        ExternalId = request.ExternalId

                    };
                    break;
                case NotificationPublishType.Sms:
                    inquryRequest = new InquiryRequest()
                    {
                        SmsPublishType = request.SmsPublishType,
                        ExternalId = request.ExternalId,
                        MessageId = request.MessageId
                    };
                    break;
                default:
                    inquryRequest = new InquiryRequest()
                    {
                        EmailPublishType = request.EmailPublishType,
                        ExternalId = request.ExternalId

                    };
                    break;
            }


            if (request.Parameters != null && request.Parameters.Count > 0)
            {
                inquryRequest.Parameters = request.Parameters.Select(x => new NotificationQueueParameter(x.Name, x.Value)).ToList();
            }
            

            var response = publisher.Inquiry(inquryRequest);
            return new NotificationInquiryResponseDTO()
            {
                NotificationStatus = response.NotificationStatusType,
                NotificationStatusDetail = response.NotificationStatusDetail,
                ResponseCode = response.ResponseCode
            };
        }
    }
}
