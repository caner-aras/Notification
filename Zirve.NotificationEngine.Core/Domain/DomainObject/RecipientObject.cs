using System.Collections.Generic;

namespace Zirve.NotificationEngine.Core.Domain.DomainObject
{
    public class RecipientObject
    {
        public string Name { get; set; }
        public string TargetAddress { get; set; }
        public ICollection<MessageVariableObject> MessageVariableObjects  { get; set; }
    }
}
