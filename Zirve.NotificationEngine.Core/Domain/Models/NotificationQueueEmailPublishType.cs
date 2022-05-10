using Zirve.NotificationEngine.Client.Enumerations;
namespace Zirve.NotificationEngine.Core.Domain.Models
{
    public class NotificationQueueEmailPublishType : EntityBase<long>
    {
        public virtual NotificationQueue NotificationQueue { get; protected set; }
        public virtual EmailPublishType EmailPublishType { get; protected set; }

        protected NotificationQueueEmailPublishType()
        {
        }

        public NotificationQueueEmailPublishType(
            NotificationQueue notificationQueue,
            EmailPublishType emailPublishType)
        {
            this.NotificationQueue = notificationQueue;
            this.EmailPublishType = emailPublishType;
        }
    }
}
