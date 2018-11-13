using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Designer.Classifications;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Mappings
{
    [PlainStorage]
    public class ClassificationEntityMap : ClassMapping<ClassificationEntity>
    {
        public ClassificationEntityMap()
        {
            Table("classificationentities");
            Id(x => x.Id, idMap =>
            {
                idMap.Generator(Generators.Assigned);
                idMap.Column("Id");
            });
            DynamicUpdate(true);

            Property(x => x.Title);
            Property(x => x.Parent);
            Property(x => x.Type);
            Property(x => x.Value);
            Property(x => x.Index);

        }
    }
}
