using System;
using System.Configuration;
using System.Web.Http;
using Zirve.NotificationEngine.Client;
using Zirve.NotificationEngine.Client.Enumerations;

namespace Zirve.NotificationEngine.Api.Controllers
{
    public class ApiController : System.Web.Http.ApiController
    {
        public INotificationClient NotificationClient { get; set; }

        public ApiController()
        {
            NotificationClient = new NotificationClient();
            NotificationClient.Init(ConfigurationManager.AppSettings["NotificationQueueServiceHostAddress"], 6000);
        }


        [System.Web.Http.HttpPost]
        [Route("api/SendWithSmtpQueue")]
        public IHttpActionResult  SendWithSmtpQueue([FromBody] Client.DTO.EnqueueRequest EnqueueRequest)
        {
            var response = NotificationClient.Enqueue(new Client.DTO.EnqueueRequest()
            {
                TrackId = Guid.NewGuid(),
                Message = EnqueueRequest.Message,
                MessageSubject = EnqueueRequest.MessageSubject,
                MessageTargetIdentifier = EnqueueRequest.MessageTargetIdentifier,
                NotificationPublishType = Client.Enumerations.NotificationPublishType.Email,
                EmailPublishType = EmailPublishType.Smtp,
                NotificationWorkingType = NotificationWorkingType.StoreAndForward,
            });

            return Json(response);
        }
    }
}
