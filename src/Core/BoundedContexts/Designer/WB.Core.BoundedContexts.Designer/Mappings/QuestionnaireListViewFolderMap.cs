using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Mappings
{
    [PlainStorage]
    public class QuestionnaireListViewFolderMap : ClassMapping<QuestionnaireListViewFolder>
    {
        public QuestionnaireListViewFolderMap()
        {
            Id(x => x.Id, idMap =>
            {
                idMap.Generator(Generators.Assigned);
                idMap.Column("Id");
            });
            DynamicUpdate(true);

            Property(x => x.Title);
            Property(x => x.Parent);
            Property(x => x.CreateDate);
            Property(x => x.CreatedBy);
        }
    }
}
