namespace Zirve.NotificationEngine.Core.Domain.Models
{
    public class NotificationQueueParameter : EntityBase<long>
    {
        public virtual NotificationQueue NotificationQueue { get; protected set; }
        public virtual string Name { get; protected set; }
        public virtual string Value { get; protected set; }

        protected NotificationQueueParameter()
        {
        }

        public NotificationQueueParameter(
            NotificationQueue notificationQueue,
            string name,
            string value)
        {
            this.NotificationQueue = notificationQueue;
            this.Name = name;
            this.Value = value;
        }

        public NotificationQueueParameter(
            string name,
            string value)
        {
            this.Name = name;
            this.Value = value;
        }
    }
}
