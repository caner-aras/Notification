using Zirve.NotificationEngine.Client.Enumerations;

namespace Zirve.NotificationEngine.Core.NotificationPublisher.Implementations.EmailPublisher
{
    public interface IEmailNotificationPublisher
    {
        EmailPublishType EmailNotificationPublishType { get; }

        PublishResponse Publish(PublishRequest request);

        InquiryResponse Inquiry(InquiryRequest request);
    }
}
