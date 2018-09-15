namespace WB.Services.Export.Tenant
{
    public class TenantId
    {
        public TenantId(string id)
        {
            this.Id = id;
        }

        public string Id { get; protected set; }

        public override string ToString()
        {
            return this.Id;
        }

        protected bool Equals(TenantId other)
        {
            return string.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TenantId) obj);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }
    }
}
