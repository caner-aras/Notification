using Zirve.NotificationEngine.Client.Enumerations;

namespace Zirve.NotificationEngine.Core.NotificationPublisher.Implementations.SmsPublisher
{
    public interface ISmsNotificationPublisher
    {
        SmsPublishType SmsNotificationPublishType { get; }

        PublishResponse Publish(PublishRequest request);

        InquiryResponse Inquiry(InquiryRequest request);
    }
}
