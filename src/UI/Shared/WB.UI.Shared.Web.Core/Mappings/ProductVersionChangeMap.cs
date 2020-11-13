using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Versions;

namespace WB.UI.Shared.Web.Mappings
{
    public class ProductVersionChangeMap : ClassMapping<ProductVersionChange>
    {
        public ProductVersionChangeMap()
        {
            this.Table("ProductVersionHistory");

            this.Id(_ => _.UpdateTimeUtc, mapper => mapper.Generator(Generators.Assigned));

            this.Property(_ => _.ProductVersion);
        }
    }
}
