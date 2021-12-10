using System;
using System.Linq;
using HotChocolate;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Questionnaires
{
    public class QuestionnairesResolver
    {
        public IQueryable<QuestionnaireBrowseItem> Questionnaires(
            Guid? id,
            long? version,
            [Service] IUnitOfWork viewFactory)
        {
            var objects = viewFactory.Session.Query<QuestionnaireBrowseItem>()
                .Where(x => !x.IsDeleted);
            if (id.HasValue)
            {
                objects = objects.Where(x => x.QuestionnaireId == id);
            }

            if (version.HasValue)
            {
                objects = objects.Where(x => x.Version == version);
            }
            
            return objects;
        }
    }
}
