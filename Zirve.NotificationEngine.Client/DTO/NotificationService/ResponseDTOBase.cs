using System.Runtime.Serialization;

namespace Zirve.NotificationEngine.Client.DTO
{
    [DataContract]
    public class ResponseDTOBase
    {
        [DataMember]
        public string ResponseCode { get; set; }
    }
}
