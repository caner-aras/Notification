using System.Linq;
using Zirve.NotificationEngine.Client.Enumerations;
using Zirve.NotificationEngine.Core.Constants;
using Zirve.NotificationEngine.Core.Exceptions;

namespace Zirve.NotificationEngine.Core.NotificationPublisher.Implementations.SmsPublisher
{
    public class SmsNotificationPublisherFactory
    {
        private readonly ISmsNotificationPublisherTypedFactory SmsNotificationPublisherTypedFactory;

        public SmsNotificationPublisherFactory(
           ISmsNotificationPublisherTypedFactory SmsNotificationPublisherTypedFactory)
        {
            this.SmsNotificationPublisherTypedFactory = SmsNotificationPublisherTypedFactory;
        }

        public ISmsNotificationPublisher GetSmsNotificationPublisher(SmsPublishType SmsPublishType)
        {
            ISmsNotificationPublisher SmsNotificationPublisher = this.SmsNotificationPublisherTypedFactory.GetSmsNotificationPublishers()
                .SingleOrDefault(x => x.SmsNotificationPublishType == SmsPublishType);

            if (SmsNotificationPublisher == null)
            {
                //sms için burayı değiştir
                throw new BusinessException(ResponseCode.EmailPublishTypeNotFound);
            }

            return SmsNotificationPublisher;
        }
    }
}
