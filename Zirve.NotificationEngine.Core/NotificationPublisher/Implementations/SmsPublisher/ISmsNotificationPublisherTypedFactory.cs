using System.Collections.Generic;

namespace Zirve.NotificationEngine.Core.NotificationPublisher.Implementations.SmsPublisher
{
    public interface ISmsNotificationPublisherTypedFactory
    {
        ICollection<ISmsNotificationPublisher> GetSmsNotificationPublishers();
    }
}
