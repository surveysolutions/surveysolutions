namespace WB.Services.Export.Tenant
{
    public class TenantInfo
    {
        public TenantInfo(string baseUrl, TenantId id)
        {
            BaseUrl = baseUrl;
            Id = id;
        }

        public TenantInfo(string baseUrl, string tenantId) : this(baseUrl, new TenantId(tenantId))
        {
        }

        public TenantInfo()
        {
            
        }

        public string BaseUrl { get; set; }

        public TenantId Id { get; set; }

        protected bool Equals(TenantInfo other)
        {
            return string.Equals(BaseUrl, other.BaseUrl) && Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(TenantInfo)) return false;
            return Equals((TenantInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((BaseUrl != null ? BaseUrl.GetHashCode() : 0) * 397) ^ (Id != null ? Id.GetHashCode() : 0);
            }
        }
    }
}
