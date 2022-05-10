using Zirve.NotificationEngine.Client.DTO.NotificationService.Enqueue;
using Zirve.NotificationEngine.Client.Enumerations;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Zirve.NotificationEngine.Client.DTO.NotificationService.NotificationInquiry
{
    public class NotificationInquiryRequestDTO : RequestDTOBase
    {
        [DataMember]
        public string ExternalId { get; set; }

        [DataMember]
        public string MessageId { get; set; }

        [DataMember]
        public EmailPublishType EmailPublishType { get; set; }

        [DataMember]
        public NotificationPublishType NotificationPublishType { get; set; }

        [DataMember]
        public ICollection<ParameterDTO> Parameters { get; set; }

        public SmsPublishType SmsPublishType { get; set; }
    }
}
