using Zirve.NotificationEngine.Client.Enumerations;

namespace Zirve.NotificationEngine.Core.NotificationPublisher.Implementations.SmsPublisher
{
    public class SmsPublisher : INotificationPublisher
    {
        private readonly SmsNotificationPublisherFactory SmsNotificationPublisherFactory;

        public SmsPublisher(
            SmsNotificationPublisherFactory SmsNotificationPublisherFactory)
        {
            this.SmsNotificationPublisherFactory = SmsNotificationPublisherFactory;
        }

        public NotificationPublishType NotificationPublishType
        {
            get { return NotificationPublishType.Sms; }
        }

        public PublishResponse Publish(PublishRequest request)
        {
            return this.SmsNotificationPublisherFactory.GetSmsNotificationPublisher(request.SmsPublishType).Publish(request);

        }


        public InquiryResponse Inquiry(InquiryRequest request)
        {
           return this.SmsNotificationPublisherFactory.GetSmsNotificationPublisher(request.SmsPublishType).Inquiry(request);
        }
    }
}
