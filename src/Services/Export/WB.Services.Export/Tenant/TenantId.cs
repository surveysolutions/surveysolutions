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
    }
}
