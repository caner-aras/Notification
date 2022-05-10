using System.Linq;
using Zirve.NotificationEngine.Client.Enumerations;
using Zirve.NotificationEngine.Core.Constants;
using Zirve.NotificationEngine.Core.Exceptions;

namespace Zirve.NotificationEngine.Core.NotificationPublisher.Implementations.EmailPublisher
{
    public class EmailNotificationPublisherFactory
    {
        private readonly IEmailNotificationPublisherTypedFactory emailNotificationPublisherTypedFactory;

        public EmailNotificationPublisherFactory(
           IEmailNotificationPublisherTypedFactory emailNotificationPublisherTypedFactory)
        {
            this.emailNotificationPublisherTypedFactory = emailNotificationPublisherTypedFactory;
        }

        public IEmailNotificationPublisher GetEmailNotificationPublisher(EmailPublishType emailPublishType)
        {
            IEmailNotificationPublisher emailNotificationPublisher = this.emailNotificationPublisherTypedFactory.GetEmailNotificationPublishers()
                .SingleOrDefault(x => x.EmailNotificationPublishType == emailPublishType);

            if (emailNotificationPublisher == null)
            {
                throw new BusinessException(ResponseCode.EmailPublishTypeNotFound);
            }

            return emailNotificationPublisher;
        }
    }
}
