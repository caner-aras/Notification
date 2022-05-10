using Zirve.NotificationEngine.Client.DTO;
using Zirve.NotificationEngine.Client.DTO.NotificationService.Enqueue;
using Zirve.NotificationEngine.Client.DTO.NotificationService.NotificationInquiry;
using Zirve.NotificationEngine.Client.ServiceInterfaces;
using System;
using System.Linq;
using System.ServiceModel;

namespace Zirve.NotificationEngine.Client
{
    public class NotificationClient : INotificationClient
    {
        protected string notificationQueueServiceAddress;
        protected int notificationQueueServiceTimeoutInSeconds;

        public void Init(
            string notificationQueueServiceAddress,
            int notificationQueueServiceTimeoutInSeconds)
        {
            this.notificationQueueServiceAddress = notificationQueueServiceAddress;
            this.notificationQueueServiceTimeoutInSeconds = notificationQueueServiceTimeoutInSeconds;
        }

        public EnqueueResponse Enqueue(EnqueueRequest request)
        {
            ChannelFactory<INotificationQueueService> channelFactory = null;

            EnqueueResponse response = new EnqueueResponse();

            try
            {
                if (this.notificationQueueServiceAddress.ToLower().Contains("net.tcp://"))
                {
                    channelFactory = new ChannelFactory<INotificationQueueService>(
                    new NetTcpBinding(SecurityMode.None)
                    {
                        MaxReceivedMessageSize = Int32.MaxValue,
                        ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas() { MaxStringContentLength = Int32.MaxValue },
                        ReceiveTimeout = TimeSpan.FromSeconds(this.notificationQueueServiceTimeoutInSeconds),
                        SendTimeout = TimeSpan.FromSeconds(this.notificationQueueServiceTimeoutInSeconds),
                        CloseTimeout = TimeSpan.FromSeconds(this.notificationQueueServiceTimeoutInSeconds),
                        OpenTimeout = TimeSpan.FromSeconds(this.notificationQueueServiceTimeoutInSeconds),
                    },
                    new EndpointAddress(this.notificationQueueServiceAddress));
                }
                else if (this.notificationQueueServiceAddress.ToLower().Contains("http"))
                {
                    channelFactory = new ChannelFactory<INotificationQueueService>(
                    new BasicHttpBinding(BasicHttpSecurityMode.None)
                    {
                        MaxReceivedMessageSize = Int32.MaxValue,
                        ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas() { MaxStringContentLength = Int32.MaxValue },
                        ReceiveTimeout = TimeSpan.FromSeconds(this.notificationQueueServiceTimeoutInSeconds),
                        SendTimeout = TimeSpan.FromSeconds(this.notificationQueueServiceTimeoutInSeconds),
                        CloseTimeout = TimeSpan.FromSeconds(this.notificationQueueServiceTimeoutInSeconds),
                        OpenTimeout = TimeSpan.FromSeconds(this.notificationQueueServiceTimeoutInSeconds),
                    },
                    new EndpointAddress(this.notificationQueueServiceAddress));
                }
                else
                {
                    throw new NotImplementedException("Client adresi hatalı ve ya girilmemiş");
                }

                INotificationQueueService service = channelFactory.CreateChannel();

                EnqueueRequestDTO enqueueRequestDTO = new EnqueueRequestDTO();
                enqueueRequestDTO.TrackId = request.TrackId;
                enqueueRequestDTO.NotificationPublishType = request.NotificationPublishType;
                enqueueRequestDTO.MessageTargetIdentifier = request.MessageTargetIdentifier;
                enqueueRequestDTO.CCRecipients = request.CCRecipients;
                enqueueRequestDTO.BCCRecipients = request.BCCRecipients;
                enqueueRequestDTO.MessageSubject = request.MessageSubject;
                enqueueRequestDTO.Message = request.Message;
                enqueueRequestDTO.EmailPublishType = request.EmailPublishType;
                enqueueRequestDTO.NotificationWorkingType = request.NotificationWorkingType;
                enqueueRequestDTO.ExternalId = request.ExternalId;
                enqueueRequestDTO.SenderAddress = request.SenderAddress;
                enqueueRequestDTO.ReplyToAddress = request.ReplyToAddress;

                if (request.Attachments != null)
                {
                    enqueueRequestDTO.Attachments = request.Attachments
                                                           .Select(x => new AttachmentDTO
                                                           {
                                                               FileName = x.FileName,
                                                               Body = x.Body
                                                           }).ToList();
                }
                if (request.Parameters != null)
                {
                    enqueueRequestDTO.Parameters = request.Parameters
                                                          .Select(x => new ParameterDTO
                                                          {
                                                              Name = x.Name,
                                                              Value = x.Value
                                                          }).ToList();
                }
                if (request.Recipients != null)
                {
                    enqueueRequestDTO.Recipients = request.Recipients
                                                          .Select(x => new RecipientDTO
                                                          {
                                                              Name = x.Name,
                                                              TargetAddress = x.TargetAddress,
                                                              MessageVariables =
                                                                      x.MessageVariables == null ? null : x.MessageVariables
                                                                                                           .Select(mv => new MessageVariableDTO()
                                                                                                           {
                                                                                                               Name = mv.Name,
                                                                                                               Value = mv.Value
                                                                                                           }).ToList()
                                                          })
                                                          .ToList();
                }

                EnqueueResponseDTO serviceResponse = service.Enqueue(enqueueRequestDTO);
                if (serviceResponse !=null)
                {
                    response.ResponseCode = serviceResponse.ResponseCode;
                    response.Explanation = serviceResponse.Explanation;
                    response.MessageId = serviceResponse.MessageId;
                    response.SmartMessageReturnCode = serviceResponse.SmartMessageReturnCode;
                }
               
            }
            finally
            {
                if (channelFactory != null && channelFactory.State != System.ServiceModel.CommunicationState.Faulted)
                {
                    try
                    {
                        channelFactory.Close();
                    }
                    catch (Exception)
                    {
                        channelFactory.Abort();
                    }
                }
            }

            return response;
        }

        public NotificationInquiryResponse NotificationInquiry(NotificationInquiryRequest request)
        {
            ChannelFactory<INotificationQueueService> channelFactory = null;

            NotificationInquiryResponse response = new NotificationInquiryResponse();

            try
            {
                channelFactory = new ChannelFactory<INotificationQueueService>(
                    new NetTcpBinding(SecurityMode.None)
                    {
                        MaxReceivedMessageSize = Int32.MaxValue,
                        ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas() { MaxStringContentLength = Int32.MaxValue },
                        ReceiveTimeout = TimeSpan.FromSeconds(this.notificationQueueServiceTimeoutInSeconds),
                        SendTimeout = TimeSpan.FromSeconds(this.notificationQueueServiceTimeoutInSeconds),
                        CloseTimeout = TimeSpan.FromSeconds(this.notificationQueueServiceTimeoutInSeconds),
                        OpenTimeout = TimeSpan.FromSeconds(this.notificationQueueServiceTimeoutInSeconds),
                    },
                    new EndpointAddress(this.notificationQueueServiceAddress));

                INotificationQueueService service = channelFactory.CreateChannel();

                NotificationInquiryRequestDTO notificationInquiryRequestDTO = new NotificationInquiryRequestDTO();

                notificationInquiryRequestDTO.ExternalId = request.ExternalId;
                notificationInquiryRequestDTO.EmailPublishType = request.EmailPublishType;
                notificationInquiryRequestDTO.SmsPublishType = request.SmsPublishType;
                notificationInquiryRequestDTO.NotificationPublishType = request.NotificationPublishType;
                notificationInquiryRequestDTO.MessageId = request.MessageId;

                if (request.Parameters != null)
                {
                    notificationInquiryRequestDTO.Parameters = request.Parameters
                                                          .Select(x => new ParameterDTO
                                                          {
                                                              Name = x.Name,
                                                              Value = x.Value
                                                          }).ToList();
                }

                NotificationInquiryResponseDTO serviceResponse = service.NotificationInquiry(notificationInquiryRequestDTO);

                response.ResponseCode = serviceResponse.ResponseCode;
                response.NotificationStatus = serviceResponse.NotificationStatus;
                response.NotificationStatusDetail = serviceResponse.NotificationStatusDetail;
            }
            finally
            {
                if (channelFactory != null && channelFactory.State != System.ServiceModel.CommunicationState.Faulted)
                {
                    try
                    {
                        channelFactory.Close();
                    }
                    catch (Exception)
                    {
                        channelFactory.Abort();
                    }
                }
            }

            return response;
        }
    }
}
