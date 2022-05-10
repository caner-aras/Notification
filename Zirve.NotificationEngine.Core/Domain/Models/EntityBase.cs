using System;

namespace Zirve.NotificationEngine.Core.Domain.Models
{
    public class EntityBase<TId>
    {
        public EntityBase()
        {
            this.CreatedOn = DateTime.Now;
        }

        public override bool Equals(object obj)
        {
            EntityBase<TId> other = obj as EntityBase<TId>;
            if (other == null)
                return false;

            // handle the case of comparing two NEW objects
            bool otherIsTransient = Equals(other.Id, default(TId));
            bool thisIsTransient = Equals(this.Id, default(TId));
            if (otherIsTransient && thisIsTransient)
                return ReferenceEquals(other, this);

            return other.Id.Equals(Id);
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        public virtual TId Id { get; protected set; }
        public virtual DateTime CreatedOn { get; protected set; }
    }
}
