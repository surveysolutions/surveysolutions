using System.Net;

namespace WB.Core.BoundedContexts.Designer.Views.AllowedAddresses
{
    public class AllowedAddress
    {
        public virtual int Id { get; set; }
        public virtual string Description { get; set; }
        public virtual IPAddress Address { get; set; }
    }
}