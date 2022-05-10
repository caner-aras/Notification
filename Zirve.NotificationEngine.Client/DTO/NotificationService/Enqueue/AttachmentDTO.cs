using System.Runtime.Serialization;

namespace Zirve.NotificationEngine.Client.DTO.NotificationService.Enqueue
{
    [DataContract]
    public class AttachmentDTO
    {
        [DataMember]
        public byte[] Body { get; set; }

        [DataMember]
        public string FileName { get; set; }
    }
}
