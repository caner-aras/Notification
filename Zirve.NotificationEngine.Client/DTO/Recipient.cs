using System.Collections.Generic;

namespace Zirve.NotificationEngine.Client.DTO
{
    public class Recipient
    {
        public string Name { get; set; }
        public string TargetAddress { get; set; }
        public ICollection<MessageVariable> MessageVariables { get; set; }
    }
}
