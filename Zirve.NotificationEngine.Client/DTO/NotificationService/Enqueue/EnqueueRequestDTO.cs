using Zirve.NotificationEngine.Client.Enumerations;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Zirve.NotificationEngine.Client.DTO.NotificationService.Enqueue
{
    [DataContract]
    public class EnqueueRequestDTO : RequestDTOBase
    {
        [DataMember]
        public NotificationPublishType NotificationPublishType { get; set; }

        [DataMember]
        public string MessageTargetIdentifier { get; set; }

        [DataMember]
        public string CCRecipients { get; set; }

        [DataMember]
        public string BCCRecipients { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string MessageSubject { get; set; }

        [DataMember]
        public ICollection<RecipientDTO> Recipients { get; set; }

        [DataMember]
        public ICollection<AttachmentDTO> Attachments { get; set; }

        [DataMember]
        public ICollection<ParameterDTO> Parameters { get; set; }

        [DataMember]
        public EmailPublishType EmailPublishType { get; set; }

        [DataMember]
        public NotificationWorkingType NotificationWorkingType { get; set; }

        [DataMember]
        public string ExternalId { get; set; }

        [DataMember]
        public string SenderAddress { get; set; }

        [DataMember]
        public string ReplyToAddress { get; set; }
    }
}
