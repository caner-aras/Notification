using System.Runtime.Serialization;

namespace Zirve.NotificationEngine.Client.DTO.NotificationService.Enqueue
{
    [DataContract]
    public class MessageVariableDTO
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Value { get; set; }
    }
}
