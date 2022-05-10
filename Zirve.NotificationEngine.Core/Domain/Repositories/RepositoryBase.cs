using PayFlex.Collection.Infrastructure;

namespace Zirve.NotificationEngine.Core.Domain.Repositories
{
    public abstract class RepositoryBase<TEntity> where TEntity : class
    {
        protected IRepository<TEntity> Repository { get; private set; }

        protected RepositoryBase() { }

        public RepositoryBase(IRepository<TEntity> repository)
        {
            this.Repository = repository;
        }

        public TEntity GetSingle(object id)
        {
            return this.Repository.GetSingle(id);
        }

        public TEntity Add(TEntity instance)
        {
            return this.Repository.Add(instance);
        }

        public TEntity AddOrUpdate(TEntity instance)
        {
            return this.Repository.AddOrUpdate(instance);
        }

        public void Update(TEntity instance)
        {
            this.Repository.Update(instance);
        }

        public void Delete(TEntity instance)
        {
            this.Repository.Delete(instance);
        }
    }
}
