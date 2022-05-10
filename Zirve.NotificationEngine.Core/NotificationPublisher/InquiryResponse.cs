namespace Zirve.NotificationEngine.Core.NotificationPublisher
{
    public class InquiryResponse
    {
        public string ResponseCode { get; set; }

        public InquiryResponse()
        {
            this.ResponseCode = Constants.ResponseCode.Success;
        }

        public int NotificationStatusType { get; set; }

        public string NotificationStatusDetail { get; set; }
    }
}
