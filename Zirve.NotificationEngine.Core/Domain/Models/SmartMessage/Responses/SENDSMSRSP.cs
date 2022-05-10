using System.Collections.Generic;

namespace Zirve.NotificationEngine.Core.Domain.Models.SmartMessage
{
    public class SENDSMSRSP
    {
        /// <summary>
        /// This will be negative or one. One mean is succes. Negative results means is fail
        /// </summary>
        public string RTCD { get; set; }
        /// <summary>
        /// Result Explain
        /// </summary>
        public string EXP { get; set; }
        /// <summary>
        /// Total count of messages
        /// </summary>
        public string TOTALCOUNT { get; set; }
        /// <summary>
        /// total count of success messages
        /// </summary>
        public string SUCCESSCOUNT { get; set; }
        /// <summary>
        /// total count of failed messages
        /// </summary>
        public string FAILEDCOUNT { get; set; }
        /// <summary>
        ///  List of Responses
        /// </summary>
        public List<RSP> RSP_LIST { get; set; }
    }
    public class RSP
    {
        /// <summary>
        /// Uniq key of the message
        /// </summary>
        public string MSGID { get; set; }
        /// <summary>
        /// Foreign key of the message to be sent
        /// </summary>
        public string EID { get; set; }
        /// <summary>
        /// Customer number
        /// </summary>
        public string CNO { get; set; }

        /// <summary>
        /// will contain any errors that may occur during the sending of the message.
        /// </summary>
        public List<object> RESULT { get; set; }
    }

}
