using Zirve.NotificationEngine.Client.Enumerations;
using Zirve.NotificationEngine.Core.Constants;
using Zirve.NotificationEngine.Core.Exceptions;
using System.Linq;

namespace Zirve.NotificationEngine.Core.NotificationPublisher
{
    public class NotificationPublisherFactory
    {
        private readonly INotificationPublisherTypedFactory notificationPublisherTypedFactory;

        public NotificationPublisherFactory(
            INotificationPublisherTypedFactory notificationPublisherTypedFactory)
        {
            this.notificationPublisherTypedFactory = notificationPublisherTypedFactory;
        }

        public INotificationPublisher GetNotificationPublisher(NotificationPublishType notificationPublishType)
        {
            INotificationPublisher notificationPublisher = this.notificationPublisherTypedFactory.GetNotificationPublishers()
                .SingleOrDefault(x => x.NotificationPublishType == notificationPublishType);

            if (notificationPublisher == null)
            {
                throw new BusinessException(ResponseCode.NotificationPublisherNotFound);
            }

            return notificationPublisher;
        }
    }
}
