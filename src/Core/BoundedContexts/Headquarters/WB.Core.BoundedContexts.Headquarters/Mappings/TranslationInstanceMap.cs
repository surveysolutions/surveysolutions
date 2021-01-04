using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.Infrastructure.PlainStorage;
using WB.Enumerator.Native.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    public class TranslationInstanceMap : ClassMapping<TranslationInstance>
    {
        public TranslationInstanceMap()
        {
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

            this.Property(x => x.Type, ptp => ptp.NotNullable(true));
            this.Property(x => x.QuestionnaireEntityId, ptp => ptp.NotNullable(true));
            this.Property(x => x.TranslationIndex);
            this.Property(x => x.TranslationId, ptp => ptp.NotNullable(true));
            this.Property(x => x.Value, ptp => ptp.NotNullable(true));
        }
    }
}
