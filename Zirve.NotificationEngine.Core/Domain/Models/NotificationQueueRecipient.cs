using System.Collections.Generic;
using System.Linq;
using Zirve.NotificationEngine.Core.Domain.DomainObject;

namespace Zirve.NotificationEngine.Core.Domain.Models
{
    public class NotificationQueueRecipient : EntityBase<long>
    {
        public virtual NotificationQueue NotificationQueue { get; protected set; }
        public virtual string Name { get; protected set; }
        public virtual string TargetAddress { get; protected set; }
        public virtual ICollection<NotificationQueueMessageVariable> MessageVariables { get; protected set; }

        protected NotificationQueueRecipient()
        {
        }

        public NotificationQueueRecipient(
            NotificationQueue notificationQueue,
            string name,
            string targetAddress,
            ICollection<MessageVariableObject> messageVariables)
        {
            this.NotificationQueue = notificationQueue;
            this.Name = name;
            this.TargetAddress = targetAddress;

            if (messageVariables != null)
            {
                this.MessageVariables = messageVariables
                    .Select(x => new NotificationQueueMessageVariable(x.Name, x.Value))
                    .ToList();
            }
        }

        public NotificationQueueRecipient(
            string name,
            string targetAddress,
            ICollection<MessageVariableObject> messageVariables)
        {
            this.Name = name;
            this.TargetAddress = targetAddress;

            if (messageVariables != null)
            {
                this.MessageVariables = messageVariables
                    .Select(x => new NotificationQueueMessageVariable(x.Name, x.Value))
                    .ToList();
            }
        }
    }
}
