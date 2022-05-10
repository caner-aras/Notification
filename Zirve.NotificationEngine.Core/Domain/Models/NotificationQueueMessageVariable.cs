namespace Zirve.NotificationEngine.Core.Domain.Models
{
    public class NotificationQueueMessageVariable : EntityBase<long>
    {
        public virtual NotificationQueueRecipient NotificationQueueRecipient { get; protected set; }
        public virtual string Name { get; protected set; }
        public virtual string Value { get; protected set; }

        protected NotificationQueueMessageVariable()
        {
        }

        public NotificationQueueMessageVariable(
            NotificationQueueRecipient notificationQueueRecipient,
            string name,
            string value)
        {
            this.NotificationQueueRecipient = notificationQueueRecipient;
            this.Name = name;
            this.Value = value;
        }

        public NotificationQueueMessageVariable(
            string name,
            string value)
        {
            this.Name = name;
            this.Value = value;
        }
    }
}
