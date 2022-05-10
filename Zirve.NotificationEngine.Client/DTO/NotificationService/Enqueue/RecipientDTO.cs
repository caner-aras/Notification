using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Zirve.NotificationEngine.Client.DTO.NotificationService.Enqueue
{
    [DataContract]
    public class RecipientDTO
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string TargetAddress { get; set; }

        [DataMember]
        public ICollection<MessageVariableDTO> MessageVariables { get; set; }
    }
}
