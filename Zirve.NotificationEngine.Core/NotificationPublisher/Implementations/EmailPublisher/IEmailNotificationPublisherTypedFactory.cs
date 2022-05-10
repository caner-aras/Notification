using System.Collections.Generic;

namespace Zirve.NotificationEngine.Core.NotificationPublisher.Implementations.EmailPublisher
{
    public interface IEmailNotificationPublisherTypedFactory
    {
        ICollection<IEmailNotificationPublisher> GetEmailNotificationPublishers();
    }
}
