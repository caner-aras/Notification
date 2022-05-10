using Zirve.NotificationEngine.Client.DTO;

namespace Zirve.NotificationEngine.Client
{
    public interface INotificationClient
    {
        void Init(
            string notificationQueueServiceAddress,
            int notificationQueueServiceTimeoutInSeconds);

        EnqueueResponse Enqueue(EnqueueRequest request);

        NotificationInquiryResponse NotificationInquiry(NotificationInquiryRequest request);


    }
}
