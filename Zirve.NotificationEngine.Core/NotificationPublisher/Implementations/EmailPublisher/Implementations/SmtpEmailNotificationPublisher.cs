using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using Zirve.NotificationEngine.Core.Domain.Models;

namespace Zirve.NotificationEngine.Core.NotificationPublisher.Implementations.EmailPublisher.Implementations
{
    public class SmtpEmailNotificationPublisher : IEmailNotificationPublisher
    {
        #region IEmailNotificationPublisher Members

        public Client.Enumerations.EmailPublishType EmailNotificationPublishType
        {
            get { return Client.Enumerations.EmailPublishType.Smtp; }
        }

        public PublishResponse Publish(PublishRequest request)
        {

            string senderEmailAddress = string.IsNullOrEmpty(request.SenderAddress) ? ConfigurationManager.AppSettings["SenderEmailAddress"] : request.SenderAddress;

            string senderName = ConfigurationManager.AppSettings["SenderName"];
            string senderEmailUsername = ConfigurationManager.AppSettings["SenderEmailUsername"];
            string senderEmailPassword = ConfigurationManager.AppSettings["SenderEmailPassword"];
            //string senderEmailAddress = ConfigurationManager.AppSettings["SenderEmailAddress"];



            MemoryStream ms = new MemoryStream();
            using (MailMessage message = new MailMessage())
            {

                message.IsBodyHtml = true;
                message.From = new MailAddress(senderEmailAddress, senderName);
                message.To.Add(request.MessageTargetIdentifier.Replace(';', ','));

                if (!string.IsNullOrEmpty(request.ReplyToAddress))
                {
                    message.ReplyToList.Add(new MailAddress(request.ReplyToAddress));
                }

                if (!string.IsNullOrEmpty(request.CCRecipients))
                {
                    message.CC.Add(request.CCRecipients.Replace(';', ','));
                }

                if (!string.IsNullOrEmpty(request.BCCRecipients))
                {
                    message.Bcc.Add(request.BCCRecipients.Replace(';', ','));
                }

                message.Subject = request.MessageSubject;

                message.Body = request.Message;

                if (request.Attachments != null && request.Attachments.Count > 0)
                {
                    List<NotificationQueueAttachment> attachmentList = request.Attachments;

                    foreach (var attachment in attachmentList)
                    {
                        ms = new MemoryStream(attachment.Body);
                        string attachmentExtension = Path.GetExtension(attachment.FileName);
                        message.Attachments.Add(new Attachment(ms, attachment.FileName, MediaTypeNames.Application.Octet));
                    }
                }

                using (SmtpClient smtp = new SmtpClient())
                {
                    if (!string.IsNullOrWhiteSpace(senderEmailUsername) && !string.IsNullOrWhiteSpace(senderEmailPassword))
                    {
                        smtp.Credentials = new NetworkCredential(senderEmailUsername, senderEmailPassword);
                    }
                    smtp.Host = ConfigurationManager.AppSettings["MailServer"];
                    smtp.Port = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.EnableSsl = int.Parse(ConfigurationManager.AppSettings["SmtpSSLEnabled"]) == 1;

                    smtp.Send(message);
                    ms.Dispose();
                }
            }

            return new PublishResponse();
        }

        #endregion


        public InquiryResponse Inquiry(InquiryRequest request)
        {
            return new InquiryResponse() { NotificationStatusType = Zirve.NotificationEngine.Core.Constants.NotificationStatusCode.Completed, ResponseCode = Zirve.NotificationEngine.Core.Constants.ResponseCode.Success };
        }
    }
}
