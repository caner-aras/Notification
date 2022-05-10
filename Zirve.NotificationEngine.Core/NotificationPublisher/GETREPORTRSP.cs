using System.Collections.Generic;
using System.Xml.Serialization;

namespace Zirve.NotificationEngine.Core.NotificationPublisher
{

    [XmlRoot(ElementName = "RSP")]
    public class RSP
    {
        [XmlElement(ElementName = "MSGID")]
        public string MSGID { get; set; }
        [XmlElement(ElementName = "EID")]
        public string EID { get; set; }
        [XmlElement(ElementName = "CNO")]
        public string CNO { get; set; }
        [XmlElement(ElementName = "ERRMSG")]
        public string ERRMSG { get; set; }
        [XmlElement(ElementName = "ST")]
        public string ST { get; set; }
        [XmlElement(ElementName = "SDATE")]
        public string SDATE { get; set; }
        [XmlElement(ElementName = "SRTY")]
        public string SRTY { get; set; }
        [XmlElement(ElementName = "ERRCD")]
        public string ERRCD { get; set; }
    }

    [XmlRoot(ElementName = "RSP_LIST")]
    public class RSP_LIST
    {
        [XmlElement(ElementName = "RSP")]
        public List<RSP> RSP { get; set; }
    }

    [XmlRoot(ElementName = "GETREPORTRSP")]
    public class GETREPORTRSP
    {
        [XmlElement(ElementName = "RTCD")]
        public string RTCD { get; set; }
        [XmlElement(ElementName = "EXP")]
        public string EXP { get; set; }
        [XmlElement(ElementName = "RSP_LIST")]
        public RSP_LIST RSP_LIST { get; set; }
    }



}
