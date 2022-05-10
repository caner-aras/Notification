using Zirve.NotificationEngine.Client.Enumerations;

namespace Zirve.NotificationEngine.Core.NotificationPublisher
{
    public interface INotificationPublisher
    {
        NotificationPublishType NotificationPublishType { get; }

        PublishResponse Publish(PublishRequest request);

        InquiryResponse Inquiry(InquiryRequest request);
    }
}
