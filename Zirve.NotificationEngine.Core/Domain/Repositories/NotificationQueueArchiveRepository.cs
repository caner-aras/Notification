using PayFlex.Collection.Infrastructure;
using Zirve.NotificationEngine.Core.Domain.Models;

namespace Zirve.NotificationEngine.Core.Domain.Repositories
{
    public class NotificationQueueArchiveRepository : RepositoryBase<NotificationQueueArchive>
    {
        public NotificationQueueArchiveRepository(IRepository<NotificationQueueArchive> repository)
            : base(repository) { }
    }
}
