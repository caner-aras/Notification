using System;
using System.Collections.Generic;
using NHibernate;
using Zirve.NotificationEngine.Core.Domain.Models;
using PayFlex.Collection.Infrastructure;

namespace Zirve.NotificationEngine.Core.Domain.Repositories
{
    public class NotificationQueueRepository : RepositoryBase<NotificationQueue>
    {
        public NotificationQueueRepository(IRepository<NotificationQueue> repository)
            : base(repository) { }


        public NotificationQueue GetSingle(long id)
        {
            ISession session = base.Repository.CurrentSession() as ISession;

            NotificationQueueDetail notificationQueueDetailAlias = null;

            return session.QueryOver<NotificationQueue>()
                // Serkan: aşağıyı kapattım. Çünkü body sadece ihtiyaç olduğunda alınsın diye ayrılmış ama her Queue fetch'inde body'yi de getiriyoruz.
                //.JoinAlias(x => x.NotificationQueueDetail, () => notificationQueueDetailAlias)
                .Where(x => x.Id == id)
                .SingleOrDefault();
        }

        public ICollection<long> GetPendingNotificationQueue(
            int itemCount,
            int maxRetryCount)
        {
            ISession session = base.Repository.CurrentSession() as ISession;
            DateTime processExpireDateTime = DateTime.Now.AddMinutes(-15);

            string sqlQuery = @";WITH NTE AS
	                            (
		                            SELECT TOP(:itemcount) * 
		                            FROM [dbo].[NotificationQueue] WITH (READPAST,UPDLOCK)
		                            WHERE [RetryCount] < :MaxRetryCount AND ( ([IsProcessing] = 0 )
                                           OR ( [IsProcessing] = 1 AND LastTryDateTime < :processExpireDateTime ))
		                            ORDER BY LastTryDateTime Desc
	                            )
	
	                            UPDATE NTE 
	                            SET LastTryDateTime =GETDATE() , IsProcessing = 1
	                            OUTPUT	INSERTED.[Id]
			                            ";

            var notifications = session.CreateSQLQuery(sqlQuery)
                             .SetParameter("itemcount", itemCount)
                               .SetParameter("MaxRetryCount", maxRetryCount)
                             .SetParameter("processExpireDateTime", processExpireDateTime)
                         .List<long>();

            //IList<NotificationQueue> notifications = session.QueryOver<NotificationQueue>()
            //    .Where(noticationQueue => noticationQueue.RetryCount < maxRetryCount && noticationQueue.IsProcessing == false
            //        || (noticationQueue.IsProcessing == true && noticationQueue.LastTryDateTime < DateTime.Now.AddMinutes(-15)))
            //    .OrderBy(noticationQueue => noticationQueue.LastTryDateTime).Desc
            //    .Lock().Upgrade
            //    .Take(itemCount)
            //    .List();

            //notifications
            //    .ForEach(notification =>
            //    {
            //        notification.DoIsProcessing();
            //        base.Update(notification);
            //    });

            return notifications;
        }
    }
}
