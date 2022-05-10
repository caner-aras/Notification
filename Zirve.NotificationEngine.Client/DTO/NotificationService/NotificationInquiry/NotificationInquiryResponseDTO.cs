using System.Runtime.Serialization;
using System.ServiceModel;

namespace Zirve.NotificationEngine.Client.DTO.NotificationService.NotificationInquiry
{
    [ServiceContract]
    public class NotificationInquiryResponseDTO : ResponseDTOBase
    {
        [DataMember]
        public int NotificationStatus { get; set; }

        [DataMember]
        public string NotificationStatusDetail { get; set; }
    }
}
