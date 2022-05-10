using System.Collections.Generic;

namespace Zirve.NotificationEngine.Core.NotificationPublisher
{
    public interface INotificationPublisherTypedFactory
    {
        ICollection<INotificationPublisher> GetNotificationPublishers();
    }
}
