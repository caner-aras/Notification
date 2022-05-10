using Zirve.NotificationEngine.Core.Domain.Models;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;

namespace Zirve.NotificationEngine.Core.NotificationPublisher.Implementations.EmailPublisher.Implementations
{
    public class AWSSmtpEmailNotificationPublisher : IEmailNotificationPublisher
    {
        #region IEmailNotificationPublisher Members

        public Client.Enumerations.EmailPublishType EmailNotificationPublishType
        {
            get { return Zirve.NotificationEngine.Client.Enumerations.EmailPublishType.AWSSmtp; }
        }

        public PublishResponse Publish(PublishRequest request)
        {



            //String username = "AKIAVU5LDDRNB7RD5MGK";  // Replace with your SMTP username.
            //String password = "BOExCSdADEl7L3woFs/Pi3cJ9N7e6GeH4dzGkZNl9loC";  // Replace with your SMTP password.
            //String host = "email-smtp.us-west-2.amazonaws.com";
            //int port = 25;

            //using (var client = new System.Net.Mail.SmtpClient(host, port))
            //{
            //    client.Credentials = new System.Net.NetworkCredential(username, password);
            //    client.EnableSsl = true;

            //    client.Send
            //    (
            //              "FROM@EXAMPLE.COM",  // Replace with the sender address.
            //              "aburakbasaran@gmail.com",    // Replace with the recipient address.
            //              "Testing Amazon SES through SMTP",
            //              "This email was delivered through Amazon SES via the SMTP end point."
            //    );
            //}




            string senderEmailAddress = "burak.basaran@zirveyazilim.net";

            string senderName = "Burak Başaran";
            string senderHost = "email-smtp.us-west-2.amazonaws.com";
            string smtpUserName = "AKIAVU5LDDRNB7RD5MGK";
            string smtpPassword = "BOExCSdADEl7L3woFs/Pi3cJ9N7e6GeH4dzGkZNl9loC";
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


                SmtpClient smtp = new SmtpClient(senderHost, 587);
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(smtpUserName, smtpPassword);
                smtp.EnableSsl = int.Parse(ConfigurationManager.AppSettings["SmtpSSLEnabled"]) == 1;
                smtp.Send(message);
                ms.Dispose();


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
