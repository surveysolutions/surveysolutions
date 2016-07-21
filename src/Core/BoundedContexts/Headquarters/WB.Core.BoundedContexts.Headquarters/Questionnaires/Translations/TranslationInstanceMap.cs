using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Questionnaires.Translations
{
    [PlainStorage]
    public class TranslationInstanceMap : ClassMapping<TranslationInstance>
    {
        public TranslationInstanceMap()
        {
            this.Id(x => x.Id, IdMapper => IdMapper.Generator(Generators.HighLow));
            this.Component(x => x.QuestionnaireId, cmp =>
            {
                cmp.Property(x => x.QuestionnaireId);
                cmp.Property(x => x.Version, ptp => ptp.Column("QuestionnaireVersion"));
            });

            this.Property(x => x.Type, ptp => ptp.NotNullable(true));
            this.Property(x => x.QuestionnaireEntityId, ptp => ptp.NotNullable(true));
            this.Property(x => x.TranslationIndex);
            this.Property(x => x.TranslationId, ptp => ptp.NotNullable(true));
            this.Property(x => x.Value, ptp => ptp.NotNullable(true));
        }
    }
}