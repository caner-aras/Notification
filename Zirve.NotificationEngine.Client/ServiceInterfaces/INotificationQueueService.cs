using Zirve.NotificationEngine.Client.DTO.NotificationService.Enqueue;
using Zirve.NotificationEngine.Client.DTO.NotificationService.NotificationInquiry;
using System.ServiceModel;

namespace Zirve.NotificationEngine.Client.ServiceInterfaces
{
    [ServiceContract]
    public interface INotificationQueueService
    {
        [OperationContract]
        EnqueueResponseDTO Enqueue(EnqueueRequestDTO request);

        [OperationContract]
        NotificationInquiryResponseDTO NotificationInquiry(NotificationInquiryRequestDTO request);
    }
}
