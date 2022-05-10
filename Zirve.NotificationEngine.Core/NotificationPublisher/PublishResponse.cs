namespace Zirve.NotificationEngine.Core.NotificationPublisher
{
    public class PublishResponse
    {
        public string ResponseCode { get; set; }
        public string MessageId { get; set; }

        public string Explanation { get; set; } 
        public string SmartMessageReturnCode { get; set; } 
        public PublishResponse()
        {
            this.ResponseCode = Constants.ResponseCode.Success;
        }
    }
}
