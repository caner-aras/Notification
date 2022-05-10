using Zirve.NotificationEngine.Client.Enumerations;

namespace Zirve.NotificationEngine.Core.NotificationPublisher.Implementations.EmailPublisher
{
    public class EmailPublisher : INotificationPublisher
    {
        private readonly EmailNotificationPublisherFactory emailNotificationPublisherFactory;

        public EmailPublisher(
            EmailNotificationPublisherFactory emailNotificationPublisherFactory)
        {
            this.emailNotificationPublisherFactory = emailNotificationPublisherFactory;
        }

        public NotificationPublishType NotificationPublishType
        {
            get { return NotificationPublishType.Email; }
        }

        public PublishResponse Publish(PublishRequest request)
        {
            return this.emailNotificationPublisherFactory.GetEmailNotificationPublisher(request.EmailPublishType).Publish(request);

        }


        public InquiryResponse Inquiry(InquiryRequest request)
        {
            return this.emailNotificationPublisherFactory.GetEmailNotificationPublisher(request.EmailPublishType).Inquiry(request);
        }
    }
}
