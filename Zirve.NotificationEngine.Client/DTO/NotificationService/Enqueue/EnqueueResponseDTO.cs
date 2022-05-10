using System.Runtime.Serialization;

namespace Zirve.NotificationEngine.Client.DTO.NotificationService.Enqueue
{
    [DataContract]
    public class EnqueueResponseDTO : ResponseDTOBase
    {
        [DataMember]
        public string MessageId { get; set; }

        [DataMember]
        public string Explanation { get; set; } //EXP

        [DataMember]
        public string SmartMessageReturnCode { get; set; } //RTCD
    }
}
