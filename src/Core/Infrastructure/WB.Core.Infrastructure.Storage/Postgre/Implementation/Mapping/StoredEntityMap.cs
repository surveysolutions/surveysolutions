using NHibernate.Mapping.ByCode.Conformist;

namespace WB.Core.Infrastructure.Storage.Postgre.Implementation.Mapping
{
    public class StoredEntityMap : ClassMapping<StoredEntity>
    {
        public StoredEntityMap()
        {
            Id(x => x.Id);
            Property(x => x.Value);
        }
    }
}