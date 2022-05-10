namespace Zirve.NotificationEngine.Core.Domain.Models
{
    public class NotificationQueueAttachment : EntityBase<long>
    {
        public virtual NotificationQueue NotificationQueue { get; protected set; }
        public virtual byte[] Body { get; protected set; }
        public virtual string FileName { get; protected set; }

        protected NotificationQueueAttachment()
        {
        }

        public NotificationQueueAttachment(
            NotificationQueue notificationQueue, 
            string fileName, 
            byte[] body)
        {
            this.NotificationQueue = notificationQueue;
            this.Body = body;
            this.FileName = fileName;
        }

        public NotificationQueueAttachment(
           string fileName,
           byte[] body)
        {
            this.Body = body;
            this.FileName = fileName;
        }
    }
}
