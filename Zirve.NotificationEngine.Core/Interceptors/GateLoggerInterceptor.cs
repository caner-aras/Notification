using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Castle.DynamicProxy;
using Newtonsoft.Json;
using PayFlex.Collection.Infrastructure;
using Zirve.NotificationEngine.Client.DTO;

namespace Zirve.NotificationEngine.Core.Interceptors
{
    public class GateLoggerInterceptor : IInterceptor
    {
        private readonly ILogger logger;

        public GateLoggerInterceptor(ILogger logger)
        {
            this.logger = logger;
        }

        public void Intercept(IInvocation invocation)
        {
            var requestObject = invocation.Arguments[0] as RequestDTOBase;

            this.logger.TrackInfo = new TrackInfo()
            {
                TrackId = requestObject.TrackId.ToString()
            };

            this.AppendLog(invocation, requestObject.TrackId, invocation.Arguments[0], "Request");

            invocation.Proceed();

            this.AppendLog(invocation, requestObject.TrackId, invocation.ReturnValue, "Response");
        }

        private void AppendLog(IInvocation invocation, Guid trackId, object obj, string logDirection)
        {
            List<string> logIgnoreMethodList = new List<string>();
            if (ConfigurationManager.AppSettings["LogIgnoreMethodList"] != null)
            {
                logIgnoreMethodList = ConfigurationManager.AppSettings["LogIgnoreMethodList"]
                    .Split(',')
                    .ToList();
            }

            if (logIgnoreMethodList.Contains(invocation.Method.Name))
            {
                return;
            }

            this.logger.Log("GateLogger", string.Format("{0}|{1}|{2}",
                invocation.Method.Name,
                logDirection,
                JsonConvert.SerializeObject(obj)), LogType.Information);
        }
    }
}