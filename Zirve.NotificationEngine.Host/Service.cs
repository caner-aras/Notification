namespace Zirve.NotificationEngine.Host
{
    using Zirve.NotificationEngine.Host.Infrascructure;
    using System.ServiceProcess;

    partial class Service : ServiceBase
    {
        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Bootstrapper bootstrapper = new Bootstrapper();
        }

        protected override void OnStop()
        {
        }
    }
}
