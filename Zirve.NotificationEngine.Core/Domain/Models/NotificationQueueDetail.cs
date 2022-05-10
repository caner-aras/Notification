namespace Zirve.NotificationEngine.Core.Domain.Models
{
    public class NotificationQueueDetail : EntityBase<long>
    {
        public virtual NotificationQueue NotificationQueue { get; protected set; }
        public virtual string Message { get; protected set; }
        public virtual string RecipientInfo { get; protected set; }

        protected NotificationQueueDetail()
        {
        }

        public NotificationQueueDetail(
            NotificationQueue notificationQueue,
            string message,
            string recipientInfo)
        {
            this.NotificationQueue = notificationQueue;
            this.Message = message;
            this.RecipientInfo = recipientInfo;
        }
    }
}
