using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Comments
{
    [PlainStorage]
    public class CommentInstanceMap : ClassMapping<CommentInstance>
    {
        public CommentInstanceMap()
        {
            Id(x => x.Id, idMapper => idMapper.Generator(Generators.Assigned));
            Property(x => x.QuestionnaireId, ptp => ptp.NotNullable(true));
            Property(x => x.EntityId, ptp => ptp.NotNullable(true));
            Property(x => x.Date, ptp => ptp.NotNullable(true));
            Property(x => x.Comment, ptp => ptp.NotNullable(true));
            Property(x => x.UserName, ptp => ptp.NotNullable(true));
            Property(x => x.UserEmail, ptp => ptp.NotNullable(true));
            Property(x => x.ResolveDate, ptp => ptp.NotNullable(false));
        }
    }
}
