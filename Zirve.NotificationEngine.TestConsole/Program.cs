using Zirve.NotificationEngine.Client.DTO;
using Zirve.NotificationEngine.Client.Enumerations;
using Zirve.NotificationEngine.Client;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace Zirve.NotificationEngine.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                INotificationClient notificationClient = new NotificationClient();
                notificationClient.Init(ConfigurationManager.AppSettings["NotificationQueueServiceHostAddress"], 6000);

                SendWithSmtpQueue(notificationClient);

                //SendWithAzureQueue(notificationClient);

                //SendSingleWithSmartMessage(notificationClient);


                //SendSingleSMSWithSmartMessage(notificationClient);

                //SendWithAwsSmtpQueue(notificationClient);
                //SendBulkWithSmartMessage(notificationClient);

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }

            Console.Read();
        }

        private static void SendWithAwsSmtpQueue(INotificationClient notificationClient)
        {
            var response = notificationClient.Enqueue(new Client.DTO.EnqueueRequest()
            {
                TrackId = Guid.NewGuid(),
                Message = "Amazon Test Mail",
                MessageSubject = "Dikkat Bu Bir AWS Entegrasyonudur !  Sevgiler.",
                MessageTargetIdentifier = "burak.basaran@zirveyazilim.net",
                NotificationPublishType = Client.Enumerations.NotificationPublishType.Email,
                EmailPublishType = EmailPublishType.AWSSmtp,
                NotificationWorkingType = NotificationWorkingType.StoreAndForward,
            });

            Console.WriteLine(response.ResponseCode);
        }

        private static void SendWithSmtpQueue(INotificationClient notificationClient)
        {
            var response = notificationClient.Enqueue(new Client.DTO.EnqueueRequest()
            {
                TrackId = Guid.NewGuid(),
                Message = "test",
                MessageSubject = "test subject",
                MessageTargetIdentifier = "caneraras@yahoo.com",
                //CCRecipients = "****@innova.com.tr",
                //BCCRecipients = "****@innova.com.tr",
                NotificationPublishType = Client.Enumerations.NotificationPublishType.Email,
                EmailPublishType = EmailPublishType.Smtp,
                NotificationWorkingType = NotificationWorkingType.StoreAndForward,
            });

            Console.WriteLine(response.ResponseCode);
        }

        private static void SendWithAzureQueue(INotificationClient notificationClient)
        {
            var response = notificationClient.Enqueue(new Client.DTO.EnqueueRequest()
            {
                TrackId = Guid.NewGuid(),
                Message = "test",
                MessageSubject = "test subject",
                MessageTargetIdentifier = "****@innova.com.tr",
                NotificationPublishType = Client.Enumerations.NotificationPublishType.Email,
                EmailPublishType = EmailPublishType.Azure,
                NotificationWorkingType = NotificationWorkingType.StoreAndForward,
            });

            Console.WriteLine(response.ResponseCode);
        }

        private static void SendSingleWithSmartMessage(INotificationClient notificationClient)
        {
            var request = new Client.DTO.EnqueueRequest()
            {
                TrackId = Guid.NewGuid(),
                Message = "test",
                MessageSubject = "test subject",
                MessageTargetIdentifier = "oaktemur@innova.com.tr",
                NotificationPublishType = Client.Enumerations.NotificationPublishType.Email,
                EmailPublishType = Client.Enumerations.EmailPublishType.SmartMessage,
                NotificationWorkingType = NotificationWorkingType.Forward,
                ExternalId = "654654646946",
            };
            request.Parameters = new List<Client.DTO.Parameter>();
            request.Parameters.Add(new Client.DTO.Parameter { Name = "JobID", Value = "a6dff75d-60ab-4a9f-9302-a4c800fe0fc7" });
            request.Parameters.Add(new Client.DTO.Parameter { Name = "FromName", Value = "innova" });
            request.Parameters.Add(new Client.DTO.Parameter { Name = "FromEmail", Value = "fatura@tantitoni.com.tr" });
            request.Parameters.Add(new Client.DTO.Parameter { Name = "InstitutionName", Value = "innova" });

            var response = notificationClient.Enqueue(request);

            Console.WriteLine(response.ResponseCode);

            System.Threading.Thread.Sleep(500);

            var inquiryRequest = new Client.DTO.NotificationInquiryRequest()
            {
                ExternalId = "654654646946",
                EmailPublishType = EmailPublishType.SmartMessage
            };
            inquiryRequest.Parameters = new List<Client.DTO.Parameter>();
            inquiryRequest.Parameters.Add(new Client.DTO.Parameter { Name = "JobID", Value = "9d0cf5f6-a945-463a-8134-a4c1010da8b0" });
            var reportResponse = notificationClient.NotificationInquiry(inquiryRequest);
        }

        private static void SendBulkWithSmartMessage(INotificationClient notificationClient)
        {
            var messageVariables1 = new List<MessageVariable>();
            messageVariables1.Add(new MessageVariable
            {
                Name = "Balance",
                Value = "200"
            });
            messageVariables1.Add(new MessageVariable
            {
                Name = "Segment",
                Value = "Gold"
            });
            messageVariables1.Add(new MessageVariable
            {
                Name = "Name",
                Value = "Serkan Olgun"
            });

            var messageVariables2 = new List<MessageVariable>();
            messageVariables2.Add(new MessageVariable
            {
                Name = "Balance",
                Value = "100"
            });
            messageVariables2.Add(new MessageVariable
            {
                Name = "Segment",
                Value = "Silver"
            });
            messageVariables2.Add(new MessageVariable
            {
                Name = "Name",
                Value = "G sko"
            });

            var recipients = new List<Recipient>();
            recipients.Add(new Recipient
            {
                Name = "Serkan Olgun",
                TargetAddress = "****@innova.com.tr",
                MessageVariables = messageVariables1
            });
            recipients.Add(new Recipient
            {
                Name = "Serkan Olgun 2",
                TargetAddress = "****@****.****",
                MessageVariables = messageVariables2
            });
            var request = new Client.DTO.EnqueueRequest()
            {
                TrackId = Guid.NewGuid(),
                Message = "<!DOCTYPE html PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\" \"http://www.w3.org/TR/html4/loose.dtd\"> <html lang=\"en\">   <head>     <meta content=\"text/html; charset=UTF-8\" http-equiv=\"content-type\">     <meta name=\"viewport\" content=\"initial-scale=1.0\">     <!-- So that mobile webkit will display zoomed in -->     <meta name=\"format-detection\" content=\"telephone=no\">     <!-- disable auto telephone linking in iOS -->     <title>Ooredoo Mail Çalışmaları</title>    <style type=\"text/css\"> 		.ReadMsgBody{ 			width:100%; 			background-color:#ebebeb; 		} 		.ExternalClass{ 			width:100%; 			background-color:#ebebeb; 		} 		.ExternalClass,.ExternalClass p,.ExternalClass span,.ExternalClass font,.ExternalClass td,.ExternalClass div{ 			line-height:100%; 		} 		body{ 			-webkit-text-size-adjust:none; 			-ms-text-size-adjust:none; 			font-family:Arial, Helvetica, sans-serif; 		} 		body{ 			margin:0; 			padding:0; 		} 		table{ 			border-spacing:0; 		} 		table td{ 			border-collapse:collapse; 		} 		.yshortcuts a{ 			border-bottom:none !important; 		} 		div.text-note{ 			font-size:11px; 			color:#404142; 			margin:0px; 		} 		div.text-note a{ 			color:#ed1c24; 		} 		.border-gray{ 			border-bottom:1px solid #dfe0e0; 		} 		h2{ 			font-weight:500; 			font-size:22px; 			color:#414042; 		} 		h3{ 			font-weight:500; 			font-size:18px; 			color:#414042; 		} 		p.paragraph-text{ 			margin:0 0 20px 0; 			padding:0; 			color:#414042; 			font-size:13px; 		} 		.ftr-content{ 			font-size:11px; 			color:#9a9a9a; 		} 		.ftr-content-left{ 			font-size:11px; 			color:#9a9a9a; 		} 		.ftr-content-link{ 			font-size:11px; 			color:#9a9a9a; 			font-weight:bold; 			text-decoration:none; 		} 		img{ 			max-width:100%; 		} 	@media screen and (max-width: 600px){ 		table[class=container]{ 			width:100% !important; 		}  }	@media screen and (max-width: 300px){ 		td[class=container-padding]{ 			padding-left:12px !important; 			padding-right:12px !important; 		}  } </style> 	</head> 	<body rightmargin=\"0\" bottommargin=\"0\" leftmargin=\"0\" topmargin=\"0\" style=\"    background-color: #ebecec; margin:0; padding:0;\"      marginheight=\"0\" marginwidth=\"0\">     <table class=\"container \" style=\"border-bottom:1px solid #dfe0e0;\" bgcolor=\"#FFFFFF\"        border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">       <tbody>         <tr>           <td align=\"center\" height=\"100\" width=\"100%\"><a href=\"http://www.ooredoo.dz/\"><img                  alt=\"\" src=\"http://www.ooredoo.com/en/media/get/20130320_logo.png\"></a><br>               <table class=\"container \" style=\" display:block !important; margin: 0 auto!important;clear: both!important; \"                  bgcolor=\"#ffffff\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"650\">                 <tbody>                   <tr>                   </tr>                   <tr>                   </tr>                 </tbody>               </table>             </td>         </tr>       </tbody>     </table> 	 	<table bgcolor=\"#ebecec\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">       <tbody>         <tr>           <td style=\"background-color: #ebebeb;\" align=\"center\" bgcolor=\"#ebebeb\"              valign=\"top\" width=\"100%\">             <!-- 600px container (white background) -->             <table class=\"container\" bgcolor=\"#ffffff\" border=\"0\" cellpadding=\"0\"                cellspacing=\"0\" width=\"650\">               <tbody>                 <tr>                   <td colspan=\"3\" style=\"background-color:#ebecec;\" align=\"right\"                      bgcolor=\"#ebecec\" height=\"50\"><br>                   </td>                 </tr>                 <tr>                   <td colspan=\"3\" style=\"background-color:#ebecec;\" align=\"right\"                      bgcolor=\"#ebecec\" height=\"20\" valign=\"top\">                     <div class=\"text-note\"> <br>                     </div>                   </td>                 </tr>                 <tr>                   <td colspan=\"3\" style=\" border:1px solid #dfe0e0; border-bottom:none;  background-color: #ffffff; font-family: Helvetica, sans-serif; color: #333;\"                      bgcolor=\"#ffffff\">                     <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">                       <tbody>                         <tr>                           <td class=\"\" style=\"padding: 28px; text-align: center;\">                             <p class=\"MsoNormal\" style=\"text-align: center;\" align=\"center\"><b><span                                    style=\"font-size: 14pt; font-family: &quot;Arial&quot;,&quot;sans-serif&quot;;\">Senin Adın:</b> [{Name}]&nbsp;!<o:p></o:p></span></p>                             <p class=\"MsoNormal\" style=\"text-align: center;\" align=\"center\"><b><span                                    style=\"font-size: 14pt; font-family: &quot;Arial&quot;,&quot;sans-serif&quot;;\">Bakiyen:</b> [{Balance}]&nbsp;!<o:p></o:p></span></p>                             <p class=\"MsoNormal\" style=\"text-align: center;\" align=\"center\"><b><span                                    style=\"font-size: 14pt; font-family: &quot;Arial&quot;,&quot;sans-serif&quot;;\">Segmentin:</b> [{Segment}]&nbsp;!<o:p></o:p></span></p>                             <span style=\"font-size: 11pt; font-family: &quot;Calibri&quot;,&quot;sans-serif&quot;;\"><img                                  alt=\"\" src=\"http://www.ooredoo.com/media/thumbnail/widget-thumb-290x179/e9f7390bb19ab5455d4425bdf645ed11/20140615_Messi-homepage-widget-en-updated.png\"><br>                             </span>                             <p class=\"MsoNormal\" style=\"text-align: center;\" align=\"center\"><b><span                                    style=\"font-size: 14pt; font-family: &quot;Arial&quot;,&quot;sans-serif&quot;;\">Yukarıdaki de Messi!!!<o:p></o:p></span></b></p>                           </td>                         </tr>                       </tbody>                     </table>           </td>         </tr>       </tbody>                     </table>           </td>         </tr>       </tbody>                     </table> 	   </body> </html>",
                MessageSubject = "Ooredoo Kişiye Özel Mail Deneme",
                //MessageTargetIdentifier = "****@innova.com.tr",
                Recipients = recipients,
                NotificationPublishType = Client.Enumerations.NotificationPublishType.Email,
                EmailPublishType = Client.Enumerations.EmailPublishType.SmartMessage,
                NotificationWorkingType = NotificationWorkingType.TryForwardAndStore
            };
            request.Parameters = new List<Client.DTO.Parameter>();
            request.Parameters.Add(new Client.DTO.Parameter { Name = "JobID", Value = "79fa339f-2319-4995-8b12-a3fb00fa8a72" });
            request.Parameters.Add(new Client.DTO.Parameter { Name = "FromName", Value = "innova" });
            request.Parameters.Add(new Client.DTO.Parameter { Name = "FromEmail", Value = "innova@m.smartmessage.com.tr" });
            request.Parameters.Add(new Client.DTO.Parameter { Name = "InstitutionName", Value = "innova" });
            request.Parameters.Add(new Client.DTO.Parameter { Name = "PlannedDate", Value = DateTime.Now.AddMinutes(3).ToString() });
            var response = notificationClient.Enqueue(request);
            Console.WriteLine(response.ResponseCode);
        }


        private static void SendSingleSMSWithSmartMessage(INotificationClient notificationClient)
        {
            var request = new Client.DTO.EnqueueRequest()
            {
                //00905350623632
                TrackId = Guid.NewGuid(),
                Message = "test test test",
                MessageTargetIdentifier = "+905350623632",
                NotificationPublishType = Client.Enumerations.NotificationPublishType.Sms,
                SmsPublishType = Client.Enumerations.SmsPublishType.SmartMessage,
                NotificationWorkingType = NotificationWorkingType.Forward,
                ExternalId = "111222333444555",
            };
            request.Parameters = new List<Client.DTO.Parameter>();
            request.Parameters.Add(new Client.DTO.Parameter { Name = "JobID", Value = "922e4088-e374-4be0-be72-a8e900cec6e1" });


            var response = notificationClient.Enqueue(request);

            Console.WriteLine(response.ResponseCode);

            System.Threading.Thread.Sleep(10500);

            var inquiryRequest = new Client.DTO.NotificationInquiryRequest()
            {
                ExternalId = "111222333444555",
                EmailPublishType = EmailPublishType.SmartMessage
            };
            inquiryRequest.Parameters = new List<Client.DTO.Parameter>();
            inquiryRequest.Parameters.Add(new Client.DTO.Parameter { Name = "JobID", Value = "922e4088-e374-4be0-be72-a8e900cec6e1" });
            var reportResponse = notificationClient.NotificationInquiry(inquiryRequest);
        }
    }
}
