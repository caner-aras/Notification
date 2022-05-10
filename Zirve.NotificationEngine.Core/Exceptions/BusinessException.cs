using System;

namespace Zirve.NotificationEngine.Core.Exceptions
{
    public class BusinessException : Exception
    {
        public string ResponseCode { get; set; }

        public BusinessException(string responseCode)
            : base(string.Empty)
        {
            this.ResponseCode = responseCode;
        }
    }
}
