using System.Configuration;

namespace Zirve.NotificationEngine.Core.Constants
{
    public static class ConfigParams
    {
        public static readonly string SmartMessageUserName = ConfigurationManager.AppSettings["SmartMessageUserName"];
        public static readonly string SmartMessagePassword = ConfigurationManager.AppSettings["SmartMessagePassword"];
        public static readonly string SmartMessageEndPoint = ConfigurationManager.AppSettings["SmartMessageEndPoint"];
        public static readonly string SmartMessageLogRequest = ConfigurationManager.AppSettings["SmartMessageLogRequest"];

    }
}
