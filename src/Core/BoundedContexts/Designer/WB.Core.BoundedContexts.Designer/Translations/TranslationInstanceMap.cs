using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Translations
{
    [PlainStorage]
    public class TranslationInstanceMap : ClassMapping<TranslationInstance>
    {
        public TranslationInstanceMap()
        {
            ComposedId(map =>
            {
                map.Property(x => x.QuestionnaireId, ptp => ptp.NotNullable(true));
                map.Property(x => x.Type, ptp => ptp.NotNullable(true));
                map.Property(x => x.QuestionnaireEntityId, ptp => ptp.NotNullable(true));
                map.Property(x => x.TranslationIndex);
                map.Property(x => x.Culture, ptp => ptp.NotNullable(true));
            });

            Property(x => x.Translation, ptp => ptp.NotNullable(true));
        }
    }
}