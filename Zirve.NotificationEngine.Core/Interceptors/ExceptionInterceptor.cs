using System;
using System.Linq;
using Castle.DynamicProxy;
using Zirve.NotificationEngine.Core.Constants;
using Zirve.NotificationEngine.Core.Exceptions;
using Zirve.NotificationEngine.Client.DTO;
using PayFlex.Collection.Infrastructure;

namespace Zirve.NotificationEngine.Core.Interceptors
{
    public class ExceptionInterceptor : IInterceptor
    {
        private readonly ILogger logger;

        public ExceptionInterceptor(
            ILogger logger)
        {
            this.logger = logger;
        }

        public void Intercept(IInvocation invocation)
        {
            if (!(invocation.Method.IsPublic) || !invocation.Arguments.Any())
            {
                invocation.Proceed();
                return;
            }

            dynamic request = invocation.Arguments[0];

            try
            {
                invocation.Proceed();

                ResponseDTOBase response = invocation.ReturnValue as ResponseDTOBase;

                if (string.IsNullOrWhiteSpace(response.ResponseCode))
                {
                    response.ResponseCode = ResponseCode.Success;
                }
            }
            catch (BusinessException exception)
            {
                HandleException(invocation, exception.ResponseCode, LogType.Information, exception);
            }
            catch (Exception exception)
            {
                HandleException(invocation, ResponseCode.Error, LogType.Error, exception);
            }
        }


        private void HandleException(
            IInvocation invocation,
            string responseCode,
            LogType logType,
            Exception exception)
        {
            ResponseDTOBase returnValue = (ResponseDTOBase)Activator.CreateInstance(invocation.Method.ReturnType);

            returnValue.ResponseCode = responseCode;

            invocation.ReturnValue = returnValue;

            this.logger.Log(string.Format("{0}|{1}", invocation.Method.Name, returnValue.ResponseCode), logType, exception);
        }
    }
}
