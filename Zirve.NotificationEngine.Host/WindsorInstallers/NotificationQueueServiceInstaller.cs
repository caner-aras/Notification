using System.Xml;
using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Castle.Facilities.WcfIntegration;
using System;
using System.Configuration;
using System.ServiceModel;
using Zirve.NotificationEngine.Host.Services;
using Zirve.NotificationEngine.Client.ServiceInterfaces;
using Zirve.NotificationEngine.Core.Interceptors;

namespace Zirve.NotificationEngine.Host.WindsorInstallers
{
    public class NotificationQueueServiceInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            string serviceHostUrl = ConfigurationManager.AppSettings["NotificationQueueServiceHostAddress"];
            int timeoutInMinutes = 10;

            if (serviceHostUrl.ToLower().Contains("net.tcp://"))
            {
                container.Register(
                Component.For<INotificationQueueService>()
                    .ImplementedBy<NotificationQueueService>()
                    .Interceptors(InterceptorReference.ForType<GateLoggerInterceptor>()).AtIndex(0)
                    .Interceptors(InterceptorReference.ForType<ExceptionInterceptor>()).AtIndex(1)
                    .LifeStyle.PerWcfOperation()
                    .AsWcfService(new DefaultServiceModel()
                        .AddEndpoints(WcfEndpoint.BoundTo(new NetTcpBinding()
                        {
                            MaxBufferPoolSize = int.MaxValue,
                            MaxBufferSize = int.MaxValue,
                            MaxConnections = int.MaxValue,
                            MaxReceivedMessageSize = int.MaxValue,
                            ReaderQuotas = new XmlDictionaryReaderQuotas()
                            {
                                MaxDepth = int.MaxValue,
                                MaxArrayLength = int.MaxValue,
                                MaxStringContentLength = int.MaxValue,
                                MaxBytesPerRead = int.MaxValue,
                                MaxNameTableCharCount = int.MaxValue
                            },
                            PortSharingEnabled = true,
                            Security = new NetTcpSecurity
                            {
                                Mode = SecurityMode.None,
                                Transport = new TcpTransportSecurity
                                {
                                    ClientCredentialType =
                                        TcpClientCredentialType.None,
                                    ProtectionLevel =
                                        System.Net.Security.ProtectionLevel.None
                                }
                            },
                            ReceiveTimeout = TimeSpan.FromMinutes(timeoutInMinutes),
                            SendTimeout = TimeSpan.FromMinutes(timeoutInMinutes),
                            CloseTimeout = TimeSpan.FromMinutes(timeoutInMinutes),
                            OpenTimeout = TimeSpan.FromMinutes(timeoutInMinutes)
                        }
                    ).At(serviceHostUrl))
                    .PublishMetadata()));
            }
            else if (serviceHostUrl.ToLower().Contains("http"))
            {
                container.Register(
                           Component.For<INotificationQueueService>()
                    .ImplementedBy<NotificationQueueService>()
                    .Interceptors(InterceptorReference.ForType<GateLoggerInterceptor>()).AtIndex(0)
                    .Interceptors(InterceptorReference.ForType<ExceptionInterceptor>()).AtIndex(1)
                    .LifeStyle.PerWcfOperation()
                   .AsWcfService(new DefaultServiceModel()
                        .AddEndpoints(WcfEndpoint.BoundTo(new BasicHttpBinding()
                        {
                            MaxBufferPoolSize = int.MaxValue,
                            MaxBufferSize = int.MaxValue,
                            MaxReceivedMessageSize = int.MaxValue,
                            ReaderQuotas = new XmlDictionaryReaderQuotas()
                            {
                                MaxDepth = int.MaxValue,
                                MaxArrayLength = int.MaxValue,
                                MaxStringContentLength = int.MaxValue,
                                MaxBytesPerRead = int.MaxValue,
                                MaxNameTableCharCount = int.MaxValue
                            },
                            ReceiveTimeout = TimeSpan.FromMinutes(timeoutInMinutes),
                            SendTimeout = TimeSpan.FromMinutes(timeoutInMinutes),
                            CloseTimeout = TimeSpan.FromMinutes(timeoutInMinutes),
                            OpenTimeout = TimeSpan.FromMinutes(timeoutInMinutes),
                            Security = new BasicHttpSecurity()
                            {
                                Mode = serviceHostUrl.ToLower().Contains("https") ?
                                    BasicHttpSecurityMode.Transport :
                                    BasicHttpSecurityMode.None
                            }
                        }
                    ).At(serviceHostUrl))
                    .AddBaseAddresses(serviceHostUrl)
                    .PublishMetadata(o => o.EnableHttpGet())));
            }
        }
    }
}


