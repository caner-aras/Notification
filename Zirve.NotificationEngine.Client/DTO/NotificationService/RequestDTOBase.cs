using System;
using System.Runtime.Serialization;

namespace Zirve.NotificationEngine.Client.DTO
{
    [DataContract]
    public class RequestDTOBase
    {
        [DataMember]
        public Guid TrackId { get; set; }
    }
}
