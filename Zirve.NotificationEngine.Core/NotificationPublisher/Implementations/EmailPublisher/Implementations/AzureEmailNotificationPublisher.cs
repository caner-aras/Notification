using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using SendGrid;

namespace Zirve.NotificationEngine.Core.NotificationPublisher.Implementations.EmailPublisher.Implementations
{
    public class AzureEmailNotificationPublisher : IEmailNotificationPublisher
    {
        #region IEmailNotificationPublisher Members

        public Client.Enumerations.EmailPublishType EmailNotificationPublishType
        {
            get { return Client.Enumerations.EmailPublishType.Azure; }
        }

        public PublishResponse Publish(PublishRequest request)
        {
            var username = ConfigurationManager.AppSettings["AzureUserName"] == null ?
                           "azure_7bc1ec41ffe6c06634e2bfb3942a148f@azure.com" : ConfigurationManager.AppSettings["AzureUserName"].ToString();
            var pswd = ConfigurationManager.AppSettings["AzurePassword"] == null ?
                            "XT3O1k8WOryssaS" : ConfigurationManager.AppSettings["AzurePassword"].ToString();

            var senderMailAdress = ConfigurationManager.AppSettings["AzureSenderMailAdress"] == null ?
                            "yigiterenler@yahoo.com" : ConfigurationManager.AppSettings["AzureSenderMailAdress"].ToString();
            var credentials = new NetworkCredential(username, pswd);

            SendGridMessage myMessage = new SendGridMessage();
            // Add the message properties.
            myMessage.From = new MailAddress(senderMailAdress);

            myMessage.AddTo(request.MessageTargetIdentifier);

            myMessage.Subject = request.MessageSubject;

            myMessage.Html = request.Message;
            if (request.Attachments != null && request.Attachments.Count > 0)
            {
                for (int i = 0; i < request.Attachments.Count; i++)
                {
                    using (MemoryStream attachmentFileStream = new MemoryStream(request.Attachments[i].Body))
                    {
                        myMessage.AddAttachment(attachmentFileStream, request.Attachments[i].FileName);
                    }
                }

            }

            var transportWeb = new Web(credentials);
            // Send the email.
            transportWeb.Deliver(myMessage);
            return new PublishResponse();
        }

        #endregion

        public InquiryResponse Inquiry(InquiryRequest request)
        {
            return new InquiryResponse() { NotificationStatusType = Zirve.NotificationEngine.Core.Constants.NotificationStatusCode.Completed, ResponseCode = Zirve.NotificationEngine.Core.Constants.ResponseCode.Success };
        }
    }
}
