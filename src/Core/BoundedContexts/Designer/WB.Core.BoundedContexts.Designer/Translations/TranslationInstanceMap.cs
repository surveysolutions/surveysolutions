using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Translations
{
    [PlainStorage]
    public class TranslationInstanceMap : ClassMapping<TranslationInstance>
    {
        public TranslationInstanceMap()
        {
            Id(x => x.Id, idMapper => idMapper.Generator(Generators.Guid));
            
            Property(x => x.QuestionnaireId, ptp => ptp.NotNullable(true));
            Property(x => x.Type, ptp => ptp.NotNullable(true));
            Property(x => x.QuestionnaireEntityId, ptp => ptp.NotNullable(true));
            Property(x => x.TranslationIndex);
            Property(x => x.Language, ptp => ptp.NotNullable(true));

            Property(x => x.Value, ptp => ptp.NotNullable(true));
        }
    }
}