using System;
using System.Linq;
using HotChocolate;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Questionnaires
{
    public class QuestionsResolver
    {
        public IQueryable<QuestionnaireCompositeItem> Questions(Guid id, long version, string language, [Service] IUnitOfWork unitOfWork)
        {
            unitOfWork.DiscardChanges();
            var qid = new QuestionnaireIdentity(id, version).ToString();

            return unitOfWork.Session.Query<QuestionnaireCompositeItem>()
                .Where(x => x.QuestionnaireIdentity == qid)
                .Where(x => x.EntityType == EntityType.Question);
        }
    }
}
