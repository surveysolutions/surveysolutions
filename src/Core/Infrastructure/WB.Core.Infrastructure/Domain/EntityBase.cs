using System.Runtime.CompilerServices;

namespace WB.Core.Infrastructure.Domain
{
    public abstract class EntityBase<TId>
    {
        public virtual TId Id { get; set; }

        protected bool Equals(EntityBase<TId> other)
        {
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;

            if (Id.Equals(default))
            {
                return SignatureEquals(obj);
            }

            return Equals((EntityBase<TId>)obj);
        }

        public override int GetHashCode()
        {
            if (Id.Equals(default(int)))
            {
                return GetSignatureHashCode();
            }

            return Id.GetHashCode();
        }

        protected abstract int GetSignatureHashCode();
        protected abstract bool SignatureEquals(object obj);

        protected int BaseGetHashCode() => RuntimeHelpers.GetHashCode(this);
    }
}
