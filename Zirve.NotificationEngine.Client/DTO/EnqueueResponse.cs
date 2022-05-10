using System;

namespace Zirve.NotificationEngine.Client.DTO
{
    public class EnqueueResponse
    {
        public string ResponseCode { get; set; }
 
        public string MessageId { get; set; }

 
        public string Explanation { get; set; } 

 
        public string SmartMessageReturnCode { get; set; }
    }
}
