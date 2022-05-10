using PayFlex.Collection.Infrastructure.ServiceInstaller;

namespace Zirve.NotificationEngine.Host
{
    public class Program
    {
        static void Main(string[] args)
        {
            ServiceStarter.StartApplication<Service>(args);
        }
    }
}
