using CsvHelper.Configuration;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Native.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    [PlainStorage]
    public class ReusableCategoricalOptionsMap : ClassMapping<ReusableCategoricalOptions>
    {
        public ReusableCategoricalOptionsMap()
        {
            Table("reusablecategoricaloptions");

            this.Id(x => x.Id, IdMapper => IdMapper.Generator(Generators.HighLow));
            this.Component(x => x.QuestionnaireId, cmp =>
            {
                cmp.Property(x => x.Id, m =>
                {
                    m.Update(false);
                    m.Insert(false);
                    m.Access(Accessor.None);
                });

                cmp.Property(x => x.QuestionnaireId);
                cmp.Property(x => x.Version, ptp => ptp.Column("QuestionnaireVersion"));
            });

            this.Property(x => x.CategoriesId, ptp => ptp.NotNullable(true));
            this.Property(x => x.Order, ptp => ptp.NotNullable(true));

            this.Property(x => x.ParentValue);
            this.Property(x => x.Text, ptp => ptp.NotNullable(true));
            this.Property(x => x.Value, ptp => ptp.NotNullable(true));
        }
    }
}
