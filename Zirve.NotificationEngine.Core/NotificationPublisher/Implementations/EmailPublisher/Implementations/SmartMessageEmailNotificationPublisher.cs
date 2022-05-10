using System;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Xml.Linq;
using Zirve.NotificationEngine.Core.Constants;
using Zirve.NotificationEngine.Core.Exceptions;
using PayFlex.Collection.Infrastructure;

namespace Zirve.NotificationEngine.Core.NotificationPublisher.Implementations.EmailPublisher.Implementations
{
    public class SmartMessageEmailNotificationPublisher : IEmailNotificationPublisher
    {
        private static ILogger _logger;


        public SmartMessageEmailNotificationPublisher(ILogger logger)
        {
            _logger = logger;
        }

        #region IEmailNotificationPublisher Members

        public PublishResponse Publish(PublishRequest request)
        {
            // Serkan: Gelen isteğin tek mi yoksa bulk mu olduğunu buradan anlayacağız
            var isBulk = string.IsNullOrEmpty(request.MessageTargetIdentifier);

            #region Validations

            if (string.IsNullOrEmpty(ConfigParams.SmartMessageUserName)
                || string.IsNullOrEmpty(ConfigParams.SmartMessagePassword)
                || string.IsNullOrEmpty(ConfigParams.SmartMessageEndPoint))
            {
                throw new BusinessException(ResponseCode.MissingConfigurationParameterError);
            }

            if (request.Parameters == null)
            {
                throw new BusinessException(ResponseCode.ParameterListNullError);
            }

            var jobParam = request.Parameters.FirstOrDefault(x => x.Name == "JobID");
            if (jobParam == null)
            {
                throw new BusinessException(ResponseCode.ParameterIncorrectError);
            }

            var fromNameParam = request.Parameters.FirstOrDefault(x => x.Name == "FromName");
            if (fromNameParam == null)
            {
                throw new BusinessException(ResponseCode.ParameterIncorrectError);
            }

            var fromEmailParam = request.Parameters.FirstOrDefault(x => x.Name == "FromEmail");
            if (fromEmailParam == null)
            {
                throw new BusinessException(ResponseCode.ParameterIncorrectError);
            }

            var institutionNameParam = request.Parameters.FirstOrDefault(x => x.Name == "InstitutionName");
            if (institutionNameParam == null)
            {
                throw new BusinessException(ResponseCode.ParameterIncorrectError);
            }

            // Serkan: bu parametreyi optional bırakıyorum
            var plannedDateParam = request.Parameters.FirstOrDefault(x => x.Name == "PlannedDate");

            #endregion

            var token = string.Empty;
            bool useToken = true;
            try
            {
                token = GetSmartMessageToken();
            }
            catch (Exception e)
            {
                _logger.Log("E-mail Publish Using Username, Password", LogType.Information);

                useToken = false;
            }

            #region Build Email Request XML

            var xmlBuilder = new StringBuilder();

            xmlBuilder.Append("<SENDEML>");
            xmlBuilder.Append("<VERSION>1.0</VERSION>");

            #region Build Common Fields

            if (useToken)
            {
                xmlBuilder.AppendFormat("<TOKEN>{0}</TOKEN>", token);

            }
            else
            {
                xmlBuilder.AppendFormat("<USR>{0}</USR>", ConfigParams.SmartMessageUserName);
                xmlBuilder.AppendFormat("<PWD>{0}</PWD>", ConfigParams.SmartMessagePassword);
            }
            xmlBuilder.AppendFormat("<JID>{0}</JID>", jobParam.Value);
            xmlBuilder.AppendFormat("<CG>{0}</CG>", "Standart");
            xmlBuilder.AppendFormat("<MSG>{0}</MSG>", "<![CDATA[" + request.Message + "]]>");
            xmlBuilder.AppendFormat("<SBJ>{0}</SBJ>", request.MessageSubject);
            xmlBuilder.AppendFormat("<OBOE>{0}</OBOE>", fromEmailParam.Value);
            xmlBuilder.AppendFormat("<OBON>{0}</OBON>", fromNameParam.Value);

            if (plannedDateParam != null)
            {
                DateTime plannedDate = DateTime.Parse(plannedDateParam.Value);
                xmlBuilder.AppendFormat("<PDATE>{0}</PDATE>", plannedDate.ToString("yyyy-MM-ddTHH:mm:ss"));
            }

            #endregion

            #region Build Attachments

            if (request.Attachments != null && request.Attachments.Any())
            {
                xmlBuilder.Append("<ATT_LIST>");
                foreach (var attachment in request.Attachments)
                {
                    xmlBuilder.Append("<ATT>");
                    xmlBuilder.AppendFormat("<FN>{0}</FN>", attachment.FileName);
                    xmlBuilder.AppendFormat("<DATA>{0}</DATA>", Convert.ToBase64String(attachment.Body));
                    xmlBuilder.Append("</ATT>");
                }
                xmlBuilder.Append("</ATT_LIST>");
            }

            #endregion

            #region Build Recipient Info

            xmlBuilder.Append("<RCPT_LIST>");

            if (!isBulk)
            {
                var emails = request.MessageTargetIdentifier.Replace(";", ",").Split(',');
                for (int i = 0; i < emails.Length; i++)
                {
                    xmlBuilder.Append("<RCPT>");
                    xmlBuilder.AppendFormat("<TA>{0}</TA>", emails[i]);
                    xmlBuilder.AppendFormat("<TONAME>{0}</TONAME>", emails[i].Substring(0, emails[i].LastIndexOf("@", StringComparison.Ordinal)));
                    xmlBuilder.AppendFormat("<EID>{0}</EID>", request.ExternalId);
                    xmlBuilder.Append("</RCPT>");
                }

            }
            else
            {
                foreach (var recipient in request.Recipients)
                {
                    xmlBuilder.Append("<RCPT>");
                    xmlBuilder.AppendFormat("<TA>{0}</TA>", recipient.TargetAddress);
                    xmlBuilder.AppendFormat("<TONAME>{0}</TONAME>", recipient.Name);
                    xmlBuilder.AppendFormat("<EID>{0}</EID>", request.ExternalId);

                    #region Build Message Variables

                    if (recipient.MessageVariables != null && recipient.MessageVariables.Any())
                    {
                        var variableStr = string.Empty;
                        foreach (var messageVariable in recipient.MessageVariables)
                        {
                            variableStr += messageVariable.Name + "+=" + messageVariable.Value + "|~";
                        }
                        variableStr = variableStr.Length > 2 ? variableStr.Remove(variableStr.Length - 2, 2) : string.Empty;

                        xmlBuilder.AppendFormat("<VAR>{0}</VAR>", variableStr);
                    }

                    #endregion

                    xmlBuilder.Append("</RCPT>");
                }
            }
            xmlBuilder.Append("</RCPT_LIST>");

            #endregion

            xmlBuilder.Append("</SENDEML>");

            #endregion

            SendSmartMessageEmail(xmlBuilder.ToString());

            return new PublishResponse();

        }

        public InquiryResponse Inquiry(InquiryRequest request)
        {
            #region Validations

            if (string.IsNullOrEmpty(ConfigParams.SmartMessageUserName)
                || string.IsNullOrEmpty(ConfigParams.SmartMessagePassword)
                || string.IsNullOrEmpty(ConfigParams.SmartMessageEndPoint))
            {
                throw new BusinessException(ResponseCode.MissingConfigurationParameterError);
            }

            if (request.Parameters == null)
            {
                throw new BusinessException(ResponseCode.ParameterListNullError);
            }

            var jobParam = request.Parameters.FirstOrDefault(x => x.Name == "JobID");
            if (jobParam == null)
            {
                throw new BusinessException(ResponseCode.ParameterIncorrectError);
            }

            #endregion

            string statusDetail = String.Empty;

            var token = string.Empty;
            bool useToken = true;
            try
            {
                token = GetSmartMessageToken();
            }
            catch (Exception e)
            {
                _logger.Log("E-mail Inquiry Using Username, Password", LogType.Information);
                useToken = false;
            }


            #region Build Email Request XML

            var xmlBuilder = new StringBuilder();

            xmlBuilder.Append("<GETREPORT>");

            #region Build Common Fields

            if (useToken)
            {
                xmlBuilder.AppendFormat("<TOKEN>{0}</TOKEN>", token);

            }
            else
            {
                xmlBuilder.AppendFormat("<USR>{0}</USR>", ConfigParams.SmartMessageUserName);
                xmlBuilder.AppendFormat("<PWD>{0}</PWD>", ConfigParams.SmartMessagePassword);
            }
            xmlBuilder.AppendFormat("<JID>{0}</JID>", jobParam.Value);
            xmlBuilder.AppendFormat("<CG>{0}</CG>", "Standart");
            xmlBuilder.Append("<MSGLIST>");
            xmlBuilder.Append("</MSGLIST>");
            xmlBuilder.Append("<EIDLIST>");
            xmlBuilder.AppendFormat("<EID>{0}</EID>", request.ExternalId);
            xmlBuilder.Append("</EIDLIST>");

            #endregion

            xmlBuilder.Append("</GETREPORT>");
            #endregion

            var notificationStatus = Inquiry(xmlBuilder.ToString(), out statusDetail);
            return new InquiryResponse() { NotificationStatusType = notificationStatus, NotificationStatusDetail = statusDetail, ResponseCode = ResponseCode.Success };

        }

        public Client.Enumerations.EmailPublishType EmailNotificationPublishType
        {
            get { return Client.Enumerations.EmailPublishType.SmartMessage; }
        }

        #region Helper Methods

        static bool XmlParseForResponse(string XmlData)
        {
            XDocument document = XDocument.Parse(XmlData);
            if (document.Root.Element("RTCD").Value == "1" && document.Root.Element("EXP").Value == "OK")
                return true;
            else return false;
        }

        static string XMLParse(string Val)
        {
            XDocument document = XDocument.Parse(Val);
            return document.Root.Element("TOKEN").Value;
        }

        static ServiceSoapClient GetSmartMessageSoapClient()
        {
            BasicHttpBinding bindingValue = new BasicHttpBinding(BasicHttpSecurityMode.None);
            bindingValue.MaxReceivedMessageSize = Int32.MaxValue;
            bindingValue.ReaderQuotas.MaxBytesPerRead = Int32.MaxValue;
            bindingValue.ReaderQuotas.MaxArrayLength = Int32.MaxValue;
            bindingValue.ReaderQuotas.MaxDepth = Int32.MaxValue;
            bindingValue.ReaderQuotas.MaxStringContentLength = Int32.MaxValue;
            bindingValue.ReaderQuotas.MaxNameTableCharCount = Int32.MaxValue;
            bindingValue.ReceiveTimeout = TimeSpan.FromSeconds(60);
            bindingValue.SendTimeout = TimeSpan.FromSeconds(60);
            bindingValue.OpenTimeout = TimeSpan.FromSeconds(60);
            bindingValue.CloseTimeout = TimeSpan.FromSeconds(60);
            EndpointAddress address = new EndpointAddress(ConfigParams.SmartMessageEndPoint);
            var client = new ServiceSoapClient(bindingValue, address);
            return client;
        }

        static string GetSmartMessageToken()
        {
            string token;

            using (var client = GetSmartMessageSoapClient())
            {
                string xmlData = string.Format("<GETTOKEN><VERSION>1.0</VERSION><USR>{0}</USR><PWD>{1}</PWD></GETTOKEN>",
                                               ConfigParams.SmartMessageUserName, ConfigParams.SmartMessagePassword);
                _logger.Log(String.Format("Smart Message GETTOKEN  Request: {0}", xmlData), LogType.Information);

                string serviceGetTokenResponse = client.GETTOKEN(xmlData);

                if (!XmlParseForResponse(serviceGetTokenResponse))
                {
                    _logger.Log(String.Format("Smart Message GETTOKEN ERRROR  Response: {0}", serviceGetTokenResponse), LogType.Information);

                    throw new BusinessException(ResponseCode.SmartMessageGetTokenError);
                }
                token = XMLParse(serviceGetTokenResponse);

                _logger.Log(String.Format("Smart Message GETTOKEN Success  Request: {0}", serviceGetTokenResponse), LogType.Information);

            }

            return token;
        }

        static void SendSmartMessageEmail(string requestXml)
        {
            using (var client = GetSmartMessageSoapClient())
            {
                if (Convert.ToBoolean(ConfigParams.SmartMessageLogRequest))
                {
                    _logger.Log(String.Format("Smart Message SENDEML  Request: {0}", requestXml), LogType.Information);

                }
                _logger.Log("Smart Message SENDEML  Mail Sending", LogType.Information);

                var serviceGetTokenResponse = client.SENDEML(requestXml);

                if (!XmlParseForResponse(serviceGetTokenResponse))
                {
                    _logger.Log("ERROR  SENDEML", LogType.Information); 
                    _logger.Log(String.Format("Smart Message SENDEML ERRROR Request: {0}", requestXml), LogType.Information);
                    _logger.Log(String.Format("Smart Message SENDEML ERRROR Response  {0}", serviceGetTokenResponse), LogType.Information); 

                    throw new BusinessException(ResponseCode.SmartMessageSendEmailServiceError);
                }

                _logger.Log(String.Format("Smart Message SENDEML  Response: {0}", serviceGetTokenResponse), LogType.Information);
            }
        }

        static int Inquiry(string requestXml, out string statusDetail)
        {
            statusDetail = String.Empty;

            int statusCode = NotificationStatusCode.Pending;
            using (var client = GetSmartMessageSoapClient())
            {
                _logger.Log(String.Format("Smart Message GETREPORT Inquiry Request: {0}", requestXml), LogType.Information);

                var serviceGetTokenResponse = client.GETREPORT(requestXml);
                _logger.Log(String.Format("Smart Message GETREPORT Inquiry Response: {0}", serviceGetTokenResponse), LogType.Information);

                XDocument document = XDocument.Parse(serviceGetTokenResponse);

                if (!string.IsNullOrEmpty(document.Root.Element("RSP_LIST").Value) && !string.IsNullOrEmpty(document.Root.Element("RSP_LIST").Element("RSP").Value) && !string.IsNullOrEmpty(document.Root.Element("RSP_LIST").Element("RSP").Element("ST").Value))
                {
                    string status = document.Root.Element("RSP_LIST").Element("RSP").Element("ST").Value;

                    if (status == "QUE" || status == "RQU" || status == "PND" || status == "UKN")
                    {
                        statusCode = NotificationStatusCode.Pending;
                    }
                    else if (status == "OPN")
                    {
                        statusCode = NotificationStatusCode.Readed;
                    }
                    else if (status == "SNT" || status == "ANK" || status == "JNK")
                    {
                        statusCode = NotificationStatusCode.Completed;
                    }
                    else if (status == "SBN" || status == "EXP" || status == "FLT" || status == "FLO" || status == "HBN" || status == "FLD" ||
                             status == "BLK" || status == "ABN" || status == "EBN" || status == "DUP")
                    {
                        statusCode = NotificationStatusCode.Error;
                    }

                    #region SmartMessage Detay ksımının alınması

                    switch (status)
                    {
                        case "QUE":
                            statusDetail = "Gönderim kuyruğunda bekleyen mesaj";
                            break;
                        case "RQU":
                            statusDetail = "Tekrar denenmek için gönderim kuyruğunda bekleyen mesaj";
                            break;
                        case "PND":
                            statusDetail = "Henüz raporu oluşmadı şeklinde rapor alınmış mesaj";
                            break;
                        case "UKN":
                            statusDetail = "Uzun zamandır bilgi alınmadığı için rapor durumu bilinmeyen mesaj";
                            break;
                        case "OPN":
                            statusDetail = "E-Posta için okunmuş, SMS için iletilmiş mesaj";
                            break;
                        case "SNT":
                            statusDetail = "SmartMessage tarafından SMTP veya operatöre teslim edilmiş mesaj";
                            break;
                        case "ANK":
                            statusDetail = "Önemsiz e-posta olarak işaretlenmiş mesaj";
                            break;
                        case "JNK":
                            statusDetail = "Hedef okuyucu tarafından önemsiz E-Posta olarak işaretlenmiş mesaj";
                            break;
                        case "SBN":
                            statusDetail = "Mailbox kotasının dolu olması vb. geçici bir nedenden dolayı ulaştırılamamış mesaj";
                            break;
                        case "EXP":
                            statusDetail = "Sistem kaynaklı sebeplerden zaman aşımına uğramış mesaj";
                            break;
                        case "FLT":
                            statusDetail = "Belli bir zamanda gönderilebilecek mesaj sayısı limitine takılan mesaj";
                            break;
                        case "FLO":
                            statusDetail = "Optimizasyon kampanyasından gönderilen mesaj, gerçek kampanyadan gönderilemeyeceği için, gerçek kampanyada FLO statüsüne sahip mesaj";
                            break;
                        case "HBN":
                            statusDetail = "Üye E-Posta adresi ya da cep telefonu numarasının geçerli olmaması vb. kalıcı bir nedenden dolayı ulaştırılamamış mesaj";
                            break;
                        case "FLD":
                            statusDetail = "Gönderim de hata almış mesaj";
                            break;
                        case "BLK":
                            statusDetail = "Kullanıcı tarafından yasaklanmış alan adı filtresine takılmış mesaj";
                            break;
                        case "ABN":
                            statusDetail = "Bouncer tarafından belirlenen BadMail";
                            break;
                        case "EBN":
                            statusDetail = "Kullanıcı tarafından belirlenen BadMail";
                            break;
                        case "DUP":
                            statusDetail = "Yinelenen gönderim adresi filtresine takılmış mesaj";
                            break;
                        default:
                            statusDetail = "Gönderiliyor";
                            break;
                    }

                    #endregion


                }
                else
                {
                    //Tekrar sorgulanabilsin diye 
                    statusCode = NotificationStatusCode.Pending;
                }
            }

            return statusCode;
        }

        #endregion

        #endregion

        #region Proxy

        //------------------------------------------------------------------------------
        // <auto-generated>
        //     This code was generated by a tool.
        //     Runtime Version:4.0.30319.18052
        //
        //     Changes to this file may cause incorrect behavior and will be lost if
        //     the code is regenerated.
        // </auto-generated>
        //------------------------------------------------------------------------------



        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ServiceModel.ServiceContractAttribute(Namespace = "http://odc.com.tr/webservices", ConfigurationName = "ServiceSoap")]
        public interface ServiceSoap
        {

            // CODEGEN: Generating message contract since element name data from namespace http://odc.com.tr/webservices is not marked nillable
            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/GETTOKEN", ReplyAction = "*")]
            GETTOKENResponse GETTOKEN(GETTOKENRequest request);

            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/GETTOKEN", ReplyAction = "*")]
            System.Threading.Tasks.Task<GETTOKENResponse> GETTOKENAsync(GETTOKENRequest request);

            // CODEGEN: Generating message contract since element name data from namespace http://odc.com.tr/webservices is not marked nillable
            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/CANCELMESSAGE", ReplyAction = "*")]
            CANCELMESSAGEResponse CANCELMESSAGE(CANCELMESSAGERequest request);

            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/CANCELMESSAGE", ReplyAction = "*")]
            System.Threading.Tasks.Task<CANCELMESSAGEResponse> CANCELMESSAGEAsync(CANCELMESSAGERequest request);

            // CODEGEN: Generating message contract since element name data from namespace http://odc.com.tr/webservices is not marked nillable
            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/GETREPORT", ReplyAction = "*")]
            GETREPORTResponse GETREPORT(GETREPORTRequest request);

            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/GETREPORT", ReplyAction = "*")]
            System.Threading.Tasks.Task<GETREPORTResponse> GETREPORTAsync(GETREPORTRequest request);

            // CODEGEN: Generating message contract since element name data from namespace http://odc.com.tr/webservices is not marked nillable
            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/SENDEML", ReplyAction = "*")]
            SENDEMLResponse SENDEML(SENDEMLRequest request);

            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/SENDEML", ReplyAction = "*")]
            System.Threading.Tasks.Task<SENDEMLResponse> SENDEMLAsync(SENDEMLRequest request);

            // CODEGEN: Generating message contract since element name data from namespace http://odc.com.tr/webservices is not marked nillable
            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/SENDSMS", ReplyAction = "*")]
            SENDSMSResponse SENDSMS(SENDSMSRequest request);

            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/SENDSMS", ReplyAction = "*")]
            System.Threading.Tasks.Task<SENDSMSResponse> SENDSMSAsync(SENDSMSRequest request);

            // CODEGEN: Generating message contract since element name data from namespace http://odc.com.tr/webservices is not marked nillable
            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/SENDTCKNSMS", ReplyAction = "*")]
            SENDTCKNSMSResponse SENDTCKNSMS(SENDTCKNSMSRequest request);

            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/SENDTCKNSMS", ReplyAction = "*")]
            System.Threading.Tasks.Task<SENDTCKNSMSResponse> SENDTCKNSMSAsync(SENDTCKNSMSRequest request);

            // CODEGEN: Generating message contract since element name data from namespace http://odc.com.tr/webservices is not marked nillable
            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/SENDWAP", ReplyAction = "*")]
            SENDWAPResponse SENDWAP(SENDWAPRequest request);

            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/SENDWAP", ReplyAction = "*")]
            System.Threading.Tasks.Task<SENDWAPResponse> SENDWAPAsync(SENDWAPRequest request);

            // CODEGEN: Generating message contract since element name data from namespace http://odc.com.tr/webservices is not marked nillable
            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/SENDMMS", ReplyAction = "*")]
            SENDMMSResponse SENDMMS(SENDMMSRequest request);

            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/SENDMMS", ReplyAction = "*")]
            System.Threading.Tasks.Task<SENDMMSResponse> SENDMMSAsync(SENDMMSRequest request);

            // CODEGEN: Generating message contract since element name data from namespace http://odc.com.tr/webservices is not marked nillable
            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/CREATEBLACKLIST", ReplyAction = "*")]
            CREATEBLACKLISTResponse CREATEBLACKLIST(CREATEBLACKLISTRequest request);

            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/CREATEBLACKLIST", ReplyAction = "*")]
            System.Threading.Tasks.Task<CREATEBLACKLISTResponse> CREATEBLACKLISTAsync(CREATEBLACKLISTRequest request);

            // CODEGEN: Generating message contract since element name data from namespace http://odc.com.tr/webservices is not marked nillable
            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/GETBLACKLIST", ReplyAction = "*")]
            GETBLACKLISTResponse GETBLACKLIST(GETBLACKLISTRequest request);

            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/GETBLACKLIST", ReplyAction = "*")]
            System.Threading.Tasks.Task<GETBLACKLISTResponse> GETBLACKLISTAsync(GETBLACKLISTRequest request);

            // CODEGEN: Generating message contract since element name data from namespace http://odc.com.tr/webservices is not marked nillable
            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/REMOVEBLACKLIST", ReplyAction = "*")]
            REMOVEBLACKLISTResponse REMOVEBLACKLIST(REMOVEBLACKLISTRequest request);

            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/REMOVEBLACKLIST", ReplyAction = "*")]
            System.Threading.Tasks.Task<REMOVEBLACKLISTResponse> REMOVEBLACKLISTAsync(REMOVEBLACKLISTRequest request);

            // CODEGEN: Generating message contract since element name data from namespace http://odc.com.tr/webservices is not marked nillable
            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/CREATEMEMBER", ReplyAction = "*")]
            CREATEMEMBERResponse CREATEMEMBER(CREATEMEMBERRequest request);

            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/CREATEMEMBER", ReplyAction = "*")]
            System.Threading.Tasks.Task<CREATEMEMBERResponse> CREATEMEMBERAsync(CREATEMEMBERRequest request);

            // CODEGEN: Generating message contract since element name data from namespace http://odc.com.tr/webservices is not marked nillable
            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/GETMEMBERFIELD", ReplyAction = "*")]
            GETMEMBERFIELDResponse GETMEMBERFIELD(GETMEMBERFIELDRequest request);

            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/GETMEMBERFIELD", ReplyAction = "*")]
            System.Threading.Tasks.Task<GETMEMBERFIELDResponse> GETMEMBERFIELDAsync(GETMEMBERFIELDRequest request);

            // CODEGEN: Generating message contract since element name data from namespace http://odc.com.tr/webservices is not marked nillable
            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/UPDATEMEMBER", ReplyAction = "*")]
            UPDATEMEMBERResponse UPDATEMEMBER(UPDATEMEMBERRequest request);

            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/UPDATEMEMBER", ReplyAction = "*")]
            System.Threading.Tasks.Task<UPDATEMEMBERResponse> UPDATEMEMBERAsync(UPDATEMEMBERRequest request);

            // CODEGEN: Generating message contract since element name data from namespace http://odc.com.tr/webservices is not marked nillable
            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/CREATECAMPAIGN", ReplyAction = "*")]
            CREATECAMPAIGNResponse CREATECAMPAIGN(CREATECAMPAIGNRequest request);

            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/CREATECAMPAIGN", ReplyAction = "*")]
            System.Threading.Tasks.Task<CREATECAMPAIGNResponse> CREATECAMPAIGNAsync(CREATECAMPAIGNRequest request);

            // CODEGEN: Generating message contract since element name data from namespace http://odc.com.tr/webservices is not marked nillable
            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/GETUNSUBSCRIBE", ReplyAction = "*")]
            GETUNSUBSCRIBEResponse GETUNSUBSCRIBE(GETUNSUBSCRIBERequest request);

            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/GETUNSUBSCRIBE", ReplyAction = "*")]
            System.Threading.Tasks.Task<GETUNSUBSCRIBEResponse> GETUNSUBSCRIBEAsync(GETUNSUBSCRIBERequest request);

            // CODEGEN: Generating message contract since element name data from namespace http://odc.com.tr/webservices is not marked nillable
            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/GETLINKCLICKHISTORY", ReplyAction = "*")]
            GETLINKCLICKHISTORYResponse GETLINKCLICKHISTORY(GETLINKCLICKHISTORYRequest request);

            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/GETLINKCLICKHISTORY", ReplyAction = "*")]
            System.Threading.Tasks.Task<GETLINKCLICKHISTORYResponse> GETLINKCLICKHISTORYAsync(GETLINKCLICKHISTORYRequest request);

            // CODEGEN: Generating message contract since element name data from namespace http://odc.com.tr/webservices is not marked nillable
            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/GETMESSAGEHISTORY", ReplyAction = "*")]
            GETMESSAGEHISTORYResponse GETMESSAGEHISTORY(GETMESSAGEHISTORYRequest request);

            [System.ServiceModel.OperationContractAttribute(Action = "http://odc.com.tr/webservices/GETMESSAGEHISTORY", ReplyAction = "*")]
            System.Threading.Tasks.Task<GETMESSAGEHISTORYResponse> GETMESSAGEHISTORYAsync(GETMESSAGEHISTORYRequest request);
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class GETTOKENRequest
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "GETTOKEN", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public GETTOKENRequestBody Body;

            public GETTOKENRequest()
            {
            }

            public GETTOKENRequest(GETTOKENRequestBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class GETTOKENRequestBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string data;

            public GETTOKENRequestBody()
            {
            }

            public GETTOKENRequestBody(string data)
            {
                this.data = data;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class GETTOKENResponse
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "GETTOKENResponse", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public GETTOKENResponseBody Body;

            public GETTOKENResponse()
            {
            }

            public GETTOKENResponse(GETTOKENResponseBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class GETTOKENResponseBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string GETTOKENResult;

            public GETTOKENResponseBody()
            {
            }

            public GETTOKENResponseBody(string GETTOKENResult)
            {
                this.GETTOKENResult = GETTOKENResult;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class CANCELMESSAGERequest
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "CANCELMESSAGE", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public CANCELMESSAGERequestBody Body;

            public CANCELMESSAGERequest()
            {
            }

            public CANCELMESSAGERequest(CANCELMESSAGERequestBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class CANCELMESSAGERequestBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string data;

            public CANCELMESSAGERequestBody()
            {
            }

            public CANCELMESSAGERequestBody(string data)
            {
                this.data = data;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class CANCELMESSAGEResponse
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "CANCELMESSAGEResponse", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public CANCELMESSAGEResponseBody Body;

            public CANCELMESSAGEResponse()
            {
            }

            public CANCELMESSAGEResponse(CANCELMESSAGEResponseBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class CANCELMESSAGEResponseBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string CANCELMESSAGEResult;

            public CANCELMESSAGEResponseBody()
            {
            }

            public CANCELMESSAGEResponseBody(string CANCELMESSAGEResult)
            {
                this.CANCELMESSAGEResult = CANCELMESSAGEResult;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class GETREPORTRequest
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "GETREPORT", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public GETREPORTRequestBody Body;

            public GETREPORTRequest()
            {
            }

            public GETREPORTRequest(GETREPORTRequestBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class GETREPORTRequestBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string data;

            public GETREPORTRequestBody()
            {
            }

            public GETREPORTRequestBody(string data)
            {
                this.data = data;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class GETREPORTResponse
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "GETREPORTResponse", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public GETREPORTResponseBody Body;

            public GETREPORTResponse()
            {
            }

            public GETREPORTResponse(GETREPORTResponseBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class GETREPORTResponseBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string GETREPORTResult;

            public GETREPORTResponseBody()
            {
            }

            public GETREPORTResponseBody(string GETREPORTResult)
            {
                this.GETREPORTResult = GETREPORTResult;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class SENDEMLRequest
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "SENDEML", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public SENDEMLRequestBody Body;

            public SENDEMLRequest()
            {
            }

            public SENDEMLRequest(SENDEMLRequestBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class SENDEMLRequestBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string data;

            public SENDEMLRequestBody()
            {
            }

            public SENDEMLRequestBody(string data)
            {
                this.data = data;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class SENDEMLResponse
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "SENDEMLResponse", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public SENDEMLResponseBody Body;

            public SENDEMLResponse()
            {
            }

            public SENDEMLResponse(SENDEMLResponseBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class SENDEMLResponseBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string SENDEMLResult;

            public SENDEMLResponseBody()
            {
            }

            public SENDEMLResponseBody(string SENDEMLResult)
            {
                this.SENDEMLResult = SENDEMLResult;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class SENDSMSRequest
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "SENDSMS", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public SENDSMSRequestBody Body;

            public SENDSMSRequest()
            {
            }

            public SENDSMSRequest(SENDSMSRequestBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class SENDSMSRequestBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string data;

            public SENDSMSRequestBody()
            {
            }

            public SENDSMSRequestBody(string data)
            {
                this.data = data;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class SENDSMSResponse
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "SENDSMSResponse", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public SENDSMSResponseBody Body;

            public SENDSMSResponse()
            {
            }

            public SENDSMSResponse(SENDSMSResponseBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class SENDSMSResponseBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string SENDSMSResult;

            public SENDSMSResponseBody()
            {
            }

            public SENDSMSResponseBody(string SENDSMSResult)
            {
                this.SENDSMSResult = SENDSMSResult;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class SENDTCKNSMSRequest
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "SENDTCKNSMS", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public SENDTCKNSMSRequestBody Body;

            public SENDTCKNSMSRequest()
            {
            }

            public SENDTCKNSMSRequest(SENDTCKNSMSRequestBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class SENDTCKNSMSRequestBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string data;

            public SENDTCKNSMSRequestBody()
            {
            }

            public SENDTCKNSMSRequestBody(string data)
            {
                this.data = data;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class SENDTCKNSMSResponse
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "SENDTCKNSMSResponse", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public SENDTCKNSMSResponseBody Body;

            public SENDTCKNSMSResponse()
            {
            }

            public SENDTCKNSMSResponse(SENDTCKNSMSResponseBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class SENDTCKNSMSResponseBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string SENDTCKNSMSResult;

            public SENDTCKNSMSResponseBody()
            {
            }

            public SENDTCKNSMSResponseBody(string SENDTCKNSMSResult)
            {
                this.SENDTCKNSMSResult = SENDTCKNSMSResult;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class SENDWAPRequest
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "SENDWAP", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public SENDWAPRequestBody Body;

            public SENDWAPRequest()
            {
            }

            public SENDWAPRequest(SENDWAPRequestBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class SENDWAPRequestBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string data;

            public SENDWAPRequestBody()
            {
            }

            public SENDWAPRequestBody(string data)
            {
                this.data = data;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class SENDWAPResponse
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "SENDWAPResponse", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public SENDWAPResponseBody Body;

            public SENDWAPResponse()
            {
            }

            public SENDWAPResponse(SENDWAPResponseBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class SENDWAPResponseBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string SENDWAPResult;

            public SENDWAPResponseBody()
            {
            }

            public SENDWAPResponseBody(string SENDWAPResult)
            {
                this.SENDWAPResult = SENDWAPResult;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class SENDMMSRequest
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "SENDMMS", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public SENDMMSRequestBody Body;

            public SENDMMSRequest()
            {
            }

            public SENDMMSRequest(SENDMMSRequestBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class SENDMMSRequestBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string data;

            public SENDMMSRequestBody()
            {
            }

            public SENDMMSRequestBody(string data)
            {
                this.data = data;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class SENDMMSResponse
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "SENDMMSResponse", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public SENDMMSResponseBody Body;

            public SENDMMSResponse()
            {
            }

            public SENDMMSResponse(SENDMMSResponseBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class SENDMMSResponseBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string SENDMMSResult;

            public SENDMMSResponseBody()
            {
            }

            public SENDMMSResponseBody(string SENDMMSResult)
            {
                this.SENDMMSResult = SENDMMSResult;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class CREATEBLACKLISTRequest
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "CREATEBLACKLIST", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public CREATEBLACKLISTRequestBody Body;

            public CREATEBLACKLISTRequest()
            {
            }

            public CREATEBLACKLISTRequest(CREATEBLACKLISTRequestBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class CREATEBLACKLISTRequestBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string data;

            public CREATEBLACKLISTRequestBody()
            {
            }

            public CREATEBLACKLISTRequestBody(string data)
            {
                this.data = data;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class CREATEBLACKLISTResponse
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "CREATEBLACKLISTResponse", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public CREATEBLACKLISTResponseBody Body;

            public CREATEBLACKLISTResponse()
            {
            }

            public CREATEBLACKLISTResponse(CREATEBLACKLISTResponseBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class CREATEBLACKLISTResponseBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string CREATEBLACKLISTResult;

            public CREATEBLACKLISTResponseBody()
            {
            }

            public CREATEBLACKLISTResponseBody(string CREATEBLACKLISTResult)
            {
                this.CREATEBLACKLISTResult = CREATEBLACKLISTResult;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class GETBLACKLISTRequest
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "GETBLACKLIST", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public GETBLACKLISTRequestBody Body;

            public GETBLACKLISTRequest()
            {
            }

            public GETBLACKLISTRequest(GETBLACKLISTRequestBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class GETBLACKLISTRequestBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string data;

            public GETBLACKLISTRequestBody()
            {
            }

            public GETBLACKLISTRequestBody(string data)
            {
                this.data = data;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class GETBLACKLISTResponse
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "GETBLACKLISTResponse", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public GETBLACKLISTResponseBody Body;

            public GETBLACKLISTResponse()
            {
            }

            public GETBLACKLISTResponse(GETBLACKLISTResponseBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class GETBLACKLISTResponseBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string GETBLACKLISTResult;

            public GETBLACKLISTResponseBody()
            {
            }

            public GETBLACKLISTResponseBody(string GETBLACKLISTResult)
            {
                this.GETBLACKLISTResult = GETBLACKLISTResult;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class REMOVEBLACKLISTRequest
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "REMOVEBLACKLIST", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public REMOVEBLACKLISTRequestBody Body;

            public REMOVEBLACKLISTRequest()
            {
            }

            public REMOVEBLACKLISTRequest(REMOVEBLACKLISTRequestBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class REMOVEBLACKLISTRequestBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string data;

            public REMOVEBLACKLISTRequestBody()
            {
            }

            public REMOVEBLACKLISTRequestBody(string data)
            {
                this.data = data;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class REMOVEBLACKLISTResponse
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "REMOVEBLACKLISTResponse", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public REMOVEBLACKLISTResponseBody Body;

            public REMOVEBLACKLISTResponse()
            {
            }

            public REMOVEBLACKLISTResponse(REMOVEBLACKLISTResponseBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class REMOVEBLACKLISTResponseBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string REMOVEBLACKLISTResult;

            public REMOVEBLACKLISTResponseBody()
            {
            }

            public REMOVEBLACKLISTResponseBody(string REMOVEBLACKLISTResult)
            {
                this.REMOVEBLACKLISTResult = REMOVEBLACKLISTResult;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class CREATEMEMBERRequest
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "CREATEMEMBER", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public CREATEMEMBERRequestBody Body;

            public CREATEMEMBERRequest()
            {
            }

            public CREATEMEMBERRequest(CREATEMEMBERRequestBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class CREATEMEMBERRequestBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string data;

            public CREATEMEMBERRequestBody()
            {
            }

            public CREATEMEMBERRequestBody(string data)
            {
                this.data = data;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class CREATEMEMBERResponse
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "CREATEMEMBERResponse", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public CREATEMEMBERResponseBody Body;

            public CREATEMEMBERResponse()
            {
            }

            public CREATEMEMBERResponse(CREATEMEMBERResponseBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class CREATEMEMBERResponseBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string CREATEMEMBERResult;

            public CREATEMEMBERResponseBody()
            {
            }

            public CREATEMEMBERResponseBody(string CREATEMEMBERResult)
            {
                this.CREATEMEMBERResult = CREATEMEMBERResult;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class GETMEMBERFIELDRequest
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "GETMEMBERFIELD", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public GETMEMBERFIELDRequestBody Body;

            public GETMEMBERFIELDRequest()
            {
            }

            public GETMEMBERFIELDRequest(GETMEMBERFIELDRequestBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class GETMEMBERFIELDRequestBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string data;

            public GETMEMBERFIELDRequestBody()
            {
            }

            public GETMEMBERFIELDRequestBody(string data)
            {
                this.data = data;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class GETMEMBERFIELDResponse
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "GETMEMBERFIELDResponse", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public GETMEMBERFIELDResponseBody Body;

            public GETMEMBERFIELDResponse()
            {
            }

            public GETMEMBERFIELDResponse(GETMEMBERFIELDResponseBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class GETMEMBERFIELDResponseBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string GETMEMBERFIELDResult;

            public GETMEMBERFIELDResponseBody()
            {
            }

            public GETMEMBERFIELDResponseBody(string GETMEMBERFIELDResult)
            {
                this.GETMEMBERFIELDResult = GETMEMBERFIELDResult;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class UPDATEMEMBERRequest
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "UPDATEMEMBER", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public UPDATEMEMBERRequestBody Body;

            public UPDATEMEMBERRequest()
            {
            }

            public UPDATEMEMBERRequest(UPDATEMEMBERRequestBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class UPDATEMEMBERRequestBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string data;

            public UPDATEMEMBERRequestBody()
            {
            }

            public UPDATEMEMBERRequestBody(string data)
            {
                this.data = data;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class UPDATEMEMBERResponse
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "UPDATEMEMBERResponse", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public UPDATEMEMBERResponseBody Body;

            public UPDATEMEMBERResponse()
            {
            }

            public UPDATEMEMBERResponse(UPDATEMEMBERResponseBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class UPDATEMEMBERResponseBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string UPDATEMEMBERResult;

            public UPDATEMEMBERResponseBody()
            {
            }

            public UPDATEMEMBERResponseBody(string UPDATEMEMBERResult)
            {
                this.UPDATEMEMBERResult = UPDATEMEMBERResult;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class CREATECAMPAIGNRequest
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "CREATECAMPAIGN", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public CREATECAMPAIGNRequestBody Body;

            public CREATECAMPAIGNRequest()
            {
            }

            public CREATECAMPAIGNRequest(CREATECAMPAIGNRequestBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class CREATECAMPAIGNRequestBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string data;

            public CREATECAMPAIGNRequestBody()
            {
            }

            public CREATECAMPAIGNRequestBody(string data)
            {
                this.data = data;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class CREATECAMPAIGNResponse
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "CREATECAMPAIGNResponse", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public CREATECAMPAIGNResponseBody Body;

            public CREATECAMPAIGNResponse()
            {
            }

            public CREATECAMPAIGNResponse(CREATECAMPAIGNResponseBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class CREATECAMPAIGNResponseBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string CREATECAMPAIGNResult;

            public CREATECAMPAIGNResponseBody()
            {
            }

            public CREATECAMPAIGNResponseBody(string CREATECAMPAIGNResult)
            {
                this.CREATECAMPAIGNResult = CREATECAMPAIGNResult;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class GETUNSUBSCRIBERequest
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "GETUNSUBSCRIBE", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public GETUNSUBSCRIBERequestBody Body;

            public GETUNSUBSCRIBERequest()
            {
            }

            public GETUNSUBSCRIBERequest(GETUNSUBSCRIBERequestBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class GETUNSUBSCRIBERequestBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string data;

            public GETUNSUBSCRIBERequestBody()
            {
            }

            public GETUNSUBSCRIBERequestBody(string data)
            {
                this.data = data;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class GETUNSUBSCRIBEResponse
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "GETUNSUBSCRIBEResponse", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public GETUNSUBSCRIBEResponseBody Body;

            public GETUNSUBSCRIBEResponse()
            {
            }

            public GETUNSUBSCRIBEResponse(GETUNSUBSCRIBEResponseBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class GETUNSUBSCRIBEResponseBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string GETUNSUBSCRIBEResult;

            public GETUNSUBSCRIBEResponseBody()
            {
            }

            public GETUNSUBSCRIBEResponseBody(string GETUNSUBSCRIBEResult)
            {
                this.GETUNSUBSCRIBEResult = GETUNSUBSCRIBEResult;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class GETLINKCLICKHISTORYRequest
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "GETLINKCLICKHISTORY", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public GETLINKCLICKHISTORYRequestBody Body;

            public GETLINKCLICKHISTORYRequest()
            {
            }

            public GETLINKCLICKHISTORYRequest(GETLINKCLICKHISTORYRequestBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class GETLINKCLICKHISTORYRequestBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string data;

            public GETLINKCLICKHISTORYRequestBody()
            {
            }

            public GETLINKCLICKHISTORYRequestBody(string data)
            {
                this.data = data;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class GETLINKCLICKHISTORYResponse
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "GETLINKCLICKHISTORYResponse", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public GETLINKCLICKHISTORYResponseBody Body;

            public GETLINKCLICKHISTORYResponse()
            {
            }

            public GETLINKCLICKHISTORYResponse(GETLINKCLICKHISTORYResponseBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class GETLINKCLICKHISTORYResponseBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string GETLINKCLICKHISTORYResult;

            public GETLINKCLICKHISTORYResponseBody()
            {
            }

            public GETLINKCLICKHISTORYResponseBody(string GETLINKCLICKHISTORYResult)
            {
                this.GETLINKCLICKHISTORYResult = GETLINKCLICKHISTORYResult;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class GETMESSAGEHISTORYRequest
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "GETMESSAGEHISTORY", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public GETMESSAGEHISTORYRequestBody Body;

            public GETMESSAGEHISTORYRequest()
            {
            }

            public GETMESSAGEHISTORYRequest(GETMESSAGEHISTORYRequestBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class GETMESSAGEHISTORYRequestBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string data;

            public GETMESSAGEHISTORYRequestBody()
            {
            }

            public GETMESSAGEHISTORYRequestBody(string data)
            {
                this.data = data;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class GETMESSAGEHISTORYResponse
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Name = "GETMESSAGEHISTORYResponse", Namespace = "http://odc.com.tr/webservices", Order = 0)]
            public GETMESSAGEHISTORYResponseBody Body;

            public GETMESSAGEHISTORYResponse()
            {
            }

            public GETMESSAGEHISTORYResponse(GETMESSAGEHISTORYResponseBody Body)
            {
                this.Body = Body;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.Runtime.Serialization.DataContractAttribute(Namespace = "http://odc.com.tr/webservices")]
        public partial class GETMESSAGEHISTORYResponseBody
        {

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
            public string GETMESSAGEHISTORYResult;

            public GETMESSAGEHISTORYResponseBody()
            {
            }

            public GETMESSAGEHISTORYResponseBody(string GETMESSAGEHISTORYResult)
            {
                this.GETMESSAGEHISTORYResult = GETMESSAGEHISTORYResult;
            }
        }

        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        public interface ServiceSoapChannel : ServiceSoap, System.ServiceModel.IClientChannel
        {
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        public partial class ServiceSoapClient : System.ServiceModel.ClientBase<ServiceSoap>, ServiceSoap
        {

            public ServiceSoapClient()
            {
            }

            public ServiceSoapClient(string endpointConfigurationName) :
                base(endpointConfigurationName)
            {
            }

            public ServiceSoapClient(string endpointConfigurationName, string remoteAddress) :
                base(endpointConfigurationName, remoteAddress)
            {
            }

            public ServiceSoapClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) :
                base(endpointConfigurationName, remoteAddress)
            {
            }

            public ServiceSoapClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) :
                base(binding, remoteAddress)
            {
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            GETTOKENResponse ServiceSoap.GETTOKEN(GETTOKENRequest request)
            {
                return base.Channel.GETTOKEN(request);
            }

            public string GETTOKEN(string data)
            {
                GETTOKENRequest inValue = new GETTOKENRequest();
                inValue.Body = new GETTOKENRequestBody();
                inValue.Body.data = data;
                GETTOKENResponse retVal = ((ServiceSoap)(this)).GETTOKEN(inValue);
                return retVal.Body.GETTOKENResult;
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            System.Threading.Tasks.Task<GETTOKENResponse> ServiceSoap.GETTOKENAsync(GETTOKENRequest request)
            {
                return base.Channel.GETTOKENAsync(request);
            }

            public System.Threading.Tasks.Task<GETTOKENResponse> GETTOKENAsync(string data)
            {
                GETTOKENRequest inValue = new GETTOKENRequest();
                inValue.Body = new GETTOKENRequestBody();
                inValue.Body.data = data;
                return ((ServiceSoap)(this)).GETTOKENAsync(inValue);
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            CANCELMESSAGEResponse ServiceSoap.CANCELMESSAGE(CANCELMESSAGERequest request)
            {
                return base.Channel.CANCELMESSAGE(request);
            }

            public string CANCELMESSAGE(string data)
            {
                CANCELMESSAGERequest inValue = new CANCELMESSAGERequest();
                inValue.Body = new CANCELMESSAGERequestBody();
                inValue.Body.data = data;
                CANCELMESSAGEResponse retVal = ((ServiceSoap)(this)).CANCELMESSAGE(inValue);
                return retVal.Body.CANCELMESSAGEResult;
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            System.Threading.Tasks.Task<CANCELMESSAGEResponse> ServiceSoap.CANCELMESSAGEAsync(CANCELMESSAGERequest request)
            {
                return base.Channel.CANCELMESSAGEAsync(request);
            }

            public System.Threading.Tasks.Task<CANCELMESSAGEResponse> CANCELMESSAGEAsync(string data)
            {
                CANCELMESSAGERequest inValue = new CANCELMESSAGERequest();
                inValue.Body = new CANCELMESSAGERequestBody();
                inValue.Body.data = data;
                return ((ServiceSoap)(this)).CANCELMESSAGEAsync(inValue);
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            GETREPORTResponse ServiceSoap.GETREPORT(GETREPORTRequest request)
            {
                return base.Channel.GETREPORT(request);
            }

            public string GETREPORT(string data)
            {
                GETREPORTRequest inValue = new GETREPORTRequest();
                inValue.Body = new GETREPORTRequestBody();
                inValue.Body.data = data;
                GETREPORTResponse retVal = ((ServiceSoap)(this)).GETREPORT(inValue);
                return retVal.Body.GETREPORTResult;
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            System.Threading.Tasks.Task<GETREPORTResponse> ServiceSoap.GETREPORTAsync(GETREPORTRequest request)
            {
                return base.Channel.GETREPORTAsync(request);
            }

            public System.Threading.Tasks.Task<GETREPORTResponse> GETREPORTAsync(string data)
            {
                GETREPORTRequest inValue = new GETREPORTRequest();
                inValue.Body = new GETREPORTRequestBody();
                inValue.Body.data = data;
                return ((ServiceSoap)(this)).GETREPORTAsync(inValue);
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            SENDEMLResponse ServiceSoap.SENDEML(SENDEMLRequest request)
            {
                return base.Channel.SENDEML(request);
            }

            public string SENDEML(string data)
            {
                SENDEMLRequest inValue = new SENDEMLRequest();
                inValue.Body = new SENDEMLRequestBody();
                inValue.Body.data = data;
                SENDEMLResponse retVal = ((ServiceSoap)(this)).SENDEML(inValue);
                return retVal.Body.SENDEMLResult;
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            System.Threading.Tasks.Task<SENDEMLResponse> ServiceSoap.SENDEMLAsync(SENDEMLRequest request)
            {
                return base.Channel.SENDEMLAsync(request);
            }

            public System.Threading.Tasks.Task<SENDEMLResponse> SENDEMLAsync(string data)
            {
                SENDEMLRequest inValue = new SENDEMLRequest();
                inValue.Body = new SENDEMLRequestBody();
                inValue.Body.data = data;
                return ((ServiceSoap)(this)).SENDEMLAsync(inValue);
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            SENDSMSResponse ServiceSoap.SENDSMS(SENDSMSRequest request)
            {
                return base.Channel.SENDSMS(request);
            }

            public string SENDSMS(string data)
            {
                SENDSMSRequest inValue = new SENDSMSRequest();
                inValue.Body = new SENDSMSRequestBody();
                inValue.Body.data = data;
                SENDSMSResponse retVal = ((ServiceSoap)(this)).SENDSMS(inValue);
                return retVal.Body.SENDSMSResult;
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            System.Threading.Tasks.Task<SENDSMSResponse> ServiceSoap.SENDSMSAsync(SENDSMSRequest request)
            {
                return base.Channel.SENDSMSAsync(request);
            }

            public System.Threading.Tasks.Task<SENDSMSResponse> SENDSMSAsync(string data)
            {
                SENDSMSRequest inValue = new SENDSMSRequest();
                inValue.Body = new SENDSMSRequestBody();
                inValue.Body.data = data;
                return ((ServiceSoap)(this)).SENDSMSAsync(inValue);
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            SENDTCKNSMSResponse ServiceSoap.SENDTCKNSMS(SENDTCKNSMSRequest request)
            {
                return base.Channel.SENDTCKNSMS(request);
            }

            public string SENDTCKNSMS(string data)
            {
                SENDTCKNSMSRequest inValue = new SENDTCKNSMSRequest();
                inValue.Body = new SENDTCKNSMSRequestBody();
                inValue.Body.data = data;
                SENDTCKNSMSResponse retVal = ((ServiceSoap)(this)).SENDTCKNSMS(inValue);
                return retVal.Body.SENDTCKNSMSResult;
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            System.Threading.Tasks.Task<SENDTCKNSMSResponse> ServiceSoap.SENDTCKNSMSAsync(SENDTCKNSMSRequest request)
            {
                return base.Channel.SENDTCKNSMSAsync(request);
            }

            public System.Threading.Tasks.Task<SENDTCKNSMSResponse> SENDTCKNSMSAsync(string data)
            {
                SENDTCKNSMSRequest inValue = new SENDTCKNSMSRequest();
                inValue.Body = new SENDTCKNSMSRequestBody();
                inValue.Body.data = data;
                return ((ServiceSoap)(this)).SENDTCKNSMSAsync(inValue);
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            SENDWAPResponse ServiceSoap.SENDWAP(SENDWAPRequest request)
            {
                return base.Channel.SENDWAP(request);
            }

            public string SENDWAP(string data)
            {
                SENDWAPRequest inValue = new SENDWAPRequest();
                inValue.Body = new SENDWAPRequestBody();
                inValue.Body.data = data;
                SENDWAPResponse retVal = ((ServiceSoap)(this)).SENDWAP(inValue);
                return retVal.Body.SENDWAPResult;
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            System.Threading.Tasks.Task<SENDWAPResponse> ServiceSoap.SENDWAPAsync(SENDWAPRequest request)
            {
                return base.Channel.SENDWAPAsync(request);
            }

            public System.Threading.Tasks.Task<SENDWAPResponse> SENDWAPAsync(string data)
            {
                SENDWAPRequest inValue = new SENDWAPRequest();
                inValue.Body = new SENDWAPRequestBody();
                inValue.Body.data = data;
                return ((ServiceSoap)(this)).SENDWAPAsync(inValue);
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            SENDMMSResponse ServiceSoap.SENDMMS(SENDMMSRequest request)
            {
                return base.Channel.SENDMMS(request);
            }

            public string SENDMMS(string data)
            {
                SENDMMSRequest inValue = new SENDMMSRequest();
                inValue.Body = new SENDMMSRequestBody();
                inValue.Body.data = data;
                SENDMMSResponse retVal = ((ServiceSoap)(this)).SENDMMS(inValue);
                return retVal.Body.SENDMMSResult;
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            System.Threading.Tasks.Task<SENDMMSResponse> ServiceSoap.SENDMMSAsync(SENDMMSRequest request)
            {
                return base.Channel.SENDMMSAsync(request);
            }

            public System.Threading.Tasks.Task<SENDMMSResponse> SENDMMSAsync(string data)
            {
                SENDMMSRequest inValue = new SENDMMSRequest();
                inValue.Body = new SENDMMSRequestBody();
                inValue.Body.data = data;
                return ((ServiceSoap)(this)).SENDMMSAsync(inValue);
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            CREATEBLACKLISTResponse ServiceSoap.CREATEBLACKLIST(CREATEBLACKLISTRequest request)
            {
                return base.Channel.CREATEBLACKLIST(request);
            }

            public string CREATEBLACKLIST(string data)
            {
                CREATEBLACKLISTRequest inValue = new CREATEBLACKLISTRequest();
                inValue.Body = new CREATEBLACKLISTRequestBody();
                inValue.Body.data = data;
                CREATEBLACKLISTResponse retVal = ((ServiceSoap)(this)).CREATEBLACKLIST(inValue);
                return retVal.Body.CREATEBLACKLISTResult;
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            System.Threading.Tasks.Task<CREATEBLACKLISTResponse> ServiceSoap.CREATEBLACKLISTAsync(CREATEBLACKLISTRequest request)
            {
                return base.Channel.CREATEBLACKLISTAsync(request);
            }

            public System.Threading.Tasks.Task<CREATEBLACKLISTResponse> CREATEBLACKLISTAsync(string data)
            {
                CREATEBLACKLISTRequest inValue = new CREATEBLACKLISTRequest();
                inValue.Body = new CREATEBLACKLISTRequestBody();
                inValue.Body.data = data;
                return ((ServiceSoap)(this)).CREATEBLACKLISTAsync(inValue);
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            GETBLACKLISTResponse ServiceSoap.GETBLACKLIST(GETBLACKLISTRequest request)
            {
                return base.Channel.GETBLACKLIST(request);
            }

            public string GETBLACKLIST(string data)
            {
                GETBLACKLISTRequest inValue = new GETBLACKLISTRequest();
                inValue.Body = new GETBLACKLISTRequestBody();
                inValue.Body.data = data;
                GETBLACKLISTResponse retVal = ((ServiceSoap)(this)).GETBLACKLIST(inValue);
                return retVal.Body.GETBLACKLISTResult;
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            System.Threading.Tasks.Task<GETBLACKLISTResponse> ServiceSoap.GETBLACKLISTAsync(GETBLACKLISTRequest request)
            {
                return base.Channel.GETBLACKLISTAsync(request);
            }

            public System.Threading.Tasks.Task<GETBLACKLISTResponse> GETBLACKLISTAsync(string data)
            {
                GETBLACKLISTRequest inValue = new GETBLACKLISTRequest();
                inValue.Body = new GETBLACKLISTRequestBody();
                inValue.Body.data = data;
                return ((ServiceSoap)(this)).GETBLACKLISTAsync(inValue);
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            REMOVEBLACKLISTResponse ServiceSoap.REMOVEBLACKLIST(REMOVEBLACKLISTRequest request)
            {
                return base.Channel.REMOVEBLACKLIST(request);
            }

            public string REMOVEBLACKLIST(string data)
            {
                REMOVEBLACKLISTRequest inValue = new REMOVEBLACKLISTRequest();
                inValue.Body = new REMOVEBLACKLISTRequestBody();
                inValue.Body.data = data;
                REMOVEBLACKLISTResponse retVal = ((ServiceSoap)(this)).REMOVEBLACKLIST(inValue);
                return retVal.Body.REMOVEBLACKLISTResult;
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            System.Threading.Tasks.Task<REMOVEBLACKLISTResponse> ServiceSoap.REMOVEBLACKLISTAsync(REMOVEBLACKLISTRequest request)
            {
                return base.Channel.REMOVEBLACKLISTAsync(request);
            }

            public System.Threading.Tasks.Task<REMOVEBLACKLISTResponse> REMOVEBLACKLISTAsync(string data)
            {
                REMOVEBLACKLISTRequest inValue = new REMOVEBLACKLISTRequest();
                inValue.Body = new REMOVEBLACKLISTRequestBody();
                inValue.Body.data = data;
                return ((ServiceSoap)(this)).REMOVEBLACKLISTAsync(inValue);
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            CREATEMEMBERResponse ServiceSoap.CREATEMEMBER(CREATEMEMBERRequest request)
            {
                return base.Channel.CREATEMEMBER(request);
            }

            public string CREATEMEMBER(string data)
            {
                CREATEMEMBERRequest inValue = new CREATEMEMBERRequest();
                inValue.Body = new CREATEMEMBERRequestBody();
                inValue.Body.data = data;
                CREATEMEMBERResponse retVal = ((ServiceSoap)(this)).CREATEMEMBER(inValue);
                return retVal.Body.CREATEMEMBERResult;
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            System.Threading.Tasks.Task<CREATEMEMBERResponse> ServiceSoap.CREATEMEMBERAsync(CREATEMEMBERRequest request)
            {
                return base.Channel.CREATEMEMBERAsync(request);
            }

            public System.Threading.Tasks.Task<CREATEMEMBERResponse> CREATEMEMBERAsync(string data)
            {
                CREATEMEMBERRequest inValue = new CREATEMEMBERRequest();
                inValue.Body = new CREATEMEMBERRequestBody();
                inValue.Body.data = data;
                return ((ServiceSoap)(this)).CREATEMEMBERAsync(inValue);
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            GETMEMBERFIELDResponse ServiceSoap.GETMEMBERFIELD(GETMEMBERFIELDRequest request)
            {
                return base.Channel.GETMEMBERFIELD(request);
            }

            public string GETMEMBERFIELD(string data)
            {
                GETMEMBERFIELDRequest inValue = new GETMEMBERFIELDRequest();
                inValue.Body = new GETMEMBERFIELDRequestBody();
                inValue.Body.data = data;
                GETMEMBERFIELDResponse retVal = ((ServiceSoap)(this)).GETMEMBERFIELD(inValue);
                return retVal.Body.GETMEMBERFIELDResult;
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            System.Threading.Tasks.Task<GETMEMBERFIELDResponse> ServiceSoap.GETMEMBERFIELDAsync(GETMEMBERFIELDRequest request)
            {
                return base.Channel.GETMEMBERFIELDAsync(request);
            }

            public System.Threading.Tasks.Task<GETMEMBERFIELDResponse> GETMEMBERFIELDAsync(string data)
            {
                GETMEMBERFIELDRequest inValue = new GETMEMBERFIELDRequest();
                inValue.Body = new GETMEMBERFIELDRequestBody();
                inValue.Body.data = data;
                return ((ServiceSoap)(this)).GETMEMBERFIELDAsync(inValue);
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            UPDATEMEMBERResponse ServiceSoap.UPDATEMEMBER(UPDATEMEMBERRequest request)
            {
                return base.Channel.UPDATEMEMBER(request);
            }

            public string UPDATEMEMBER(string data)
            {
                UPDATEMEMBERRequest inValue = new UPDATEMEMBERRequest();
                inValue.Body = new UPDATEMEMBERRequestBody();
                inValue.Body.data = data;
                UPDATEMEMBERResponse retVal = ((ServiceSoap)(this)).UPDATEMEMBER(inValue);
                return retVal.Body.UPDATEMEMBERResult;
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            System.Threading.Tasks.Task<UPDATEMEMBERResponse> ServiceSoap.UPDATEMEMBERAsync(UPDATEMEMBERRequest request)
            {
                return base.Channel.UPDATEMEMBERAsync(request);
            }

            public System.Threading.Tasks.Task<UPDATEMEMBERResponse> UPDATEMEMBERAsync(string data)
            {
                UPDATEMEMBERRequest inValue = new UPDATEMEMBERRequest();
                inValue.Body = new UPDATEMEMBERRequestBody();
                inValue.Body.data = data;
                return ((ServiceSoap)(this)).UPDATEMEMBERAsync(inValue);
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            CREATECAMPAIGNResponse ServiceSoap.CREATECAMPAIGN(CREATECAMPAIGNRequest request)
            {
                return base.Channel.CREATECAMPAIGN(request);
            }

            public string CREATECAMPAIGN(string data)
            {
                CREATECAMPAIGNRequest inValue = new CREATECAMPAIGNRequest();
                inValue.Body = new CREATECAMPAIGNRequestBody();
                inValue.Body.data = data;
                CREATECAMPAIGNResponse retVal = ((ServiceSoap)(this)).CREATECAMPAIGN(inValue);
                return retVal.Body.CREATECAMPAIGNResult;
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            System.Threading.Tasks.Task<CREATECAMPAIGNResponse> ServiceSoap.CREATECAMPAIGNAsync(CREATECAMPAIGNRequest request)
            {
                return base.Channel.CREATECAMPAIGNAsync(request);
            }

            public System.Threading.Tasks.Task<CREATECAMPAIGNResponse> CREATECAMPAIGNAsync(string data)
            {
                CREATECAMPAIGNRequest inValue = new CREATECAMPAIGNRequest();
                inValue.Body = new CREATECAMPAIGNRequestBody();
                inValue.Body.data = data;
                return ((ServiceSoap)(this)).CREATECAMPAIGNAsync(inValue);
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            GETUNSUBSCRIBEResponse ServiceSoap.GETUNSUBSCRIBE(GETUNSUBSCRIBERequest request)
            {
                return base.Channel.GETUNSUBSCRIBE(request);
            }

            public string GETUNSUBSCRIBE(string data)
            {
                GETUNSUBSCRIBERequest inValue = new GETUNSUBSCRIBERequest();
                inValue.Body = new GETUNSUBSCRIBERequestBody();
                inValue.Body.data = data;
                GETUNSUBSCRIBEResponse retVal = ((ServiceSoap)(this)).GETUNSUBSCRIBE(inValue);
                return retVal.Body.GETUNSUBSCRIBEResult;
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            System.Threading.Tasks.Task<GETUNSUBSCRIBEResponse> ServiceSoap.GETUNSUBSCRIBEAsync(GETUNSUBSCRIBERequest request)
            {
                return base.Channel.GETUNSUBSCRIBEAsync(request);
            }

            public System.Threading.Tasks.Task<GETUNSUBSCRIBEResponse> GETUNSUBSCRIBEAsync(string data)
            {
                GETUNSUBSCRIBERequest inValue = new GETUNSUBSCRIBERequest();
                inValue.Body = new GETUNSUBSCRIBERequestBody();
                inValue.Body.data = data;
                return ((ServiceSoap)(this)).GETUNSUBSCRIBEAsync(inValue);
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            GETLINKCLICKHISTORYResponse ServiceSoap.GETLINKCLICKHISTORY(GETLINKCLICKHISTORYRequest request)
            {
                return base.Channel.GETLINKCLICKHISTORY(request);
            }

            public string GETLINKCLICKHISTORY(string data)
            {
                GETLINKCLICKHISTORYRequest inValue = new GETLINKCLICKHISTORYRequest();
                inValue.Body = new GETLINKCLICKHISTORYRequestBody();
                inValue.Body.data = data;
                GETLINKCLICKHISTORYResponse retVal = ((ServiceSoap)(this)).GETLINKCLICKHISTORY(inValue);
                return retVal.Body.GETLINKCLICKHISTORYResult;
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            System.Threading.Tasks.Task<GETLINKCLICKHISTORYResponse> ServiceSoap.GETLINKCLICKHISTORYAsync(GETLINKCLICKHISTORYRequest request)
            {
                return base.Channel.GETLINKCLICKHISTORYAsync(request);
            }

            public System.Threading.Tasks.Task<GETLINKCLICKHISTORYResponse> GETLINKCLICKHISTORYAsync(string data)
            {
                GETLINKCLICKHISTORYRequest inValue = new GETLINKCLICKHISTORYRequest();
                inValue.Body = new GETLINKCLICKHISTORYRequestBody();
                inValue.Body.data = data;
                return ((ServiceSoap)(this)).GETLINKCLICKHISTORYAsync(inValue);
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            GETMESSAGEHISTORYResponse ServiceSoap.GETMESSAGEHISTORY(GETMESSAGEHISTORYRequest request)
            {
                return base.Channel.GETMESSAGEHISTORY(request);
            }

            public string GETMESSAGEHISTORY(string data)
            {
                GETMESSAGEHISTORYRequest inValue = new GETMESSAGEHISTORYRequest();
                inValue.Body = new GETMESSAGEHISTORYRequestBody();
                inValue.Body.data = data;
                GETMESSAGEHISTORYResponse retVal = ((ServiceSoap)(this)).GETMESSAGEHISTORY(inValue);
                return retVal.Body.GETMESSAGEHISTORYResult;
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            System.Threading.Tasks.Task<GETMESSAGEHISTORYResponse> ServiceSoap.GETMESSAGEHISTORYAsync(GETMESSAGEHISTORYRequest request)
            {
                return base.Channel.GETMESSAGEHISTORYAsync(request);
            }

            public System.Threading.Tasks.Task<GETMESSAGEHISTORYResponse> GETMESSAGEHISTORYAsync(string data)
            {
                GETMESSAGEHISTORYRequest inValue = new GETMESSAGEHISTORYRequest();
                inValue.Body = new GETMESSAGEHISTORYRequestBody();
                inValue.Body.data = data;
                return ((ServiceSoap)(this)).GETMESSAGEHISTORYAsync(inValue);
            }
        }


        #endregion



    }
}
