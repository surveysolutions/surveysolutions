using System.Linq;
using Core.Supervisor.Views.Reposts.Views;
using Main.Core.View;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;
using WB.Core.Infrastructure.Raven.Implementation.ReadSide.Indexes;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace Core.Supervisor.Views.Reposts.Factories
{
    public class QuestionnairesAndVersionsFactory : IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireAndVersionsView>
    {
        private readonly IReadSideRepositoryIndexAccessor indexAccessor;

        public QuestionnairesAndVersionsFactory(IReadSideRepositoryIndexAccessor indexAccessor)
        {
            this.indexAccessor = indexAccessor;
        }

        public QuestionnaireAndVersionsView Load(QuestionnaireBrowseInputModel input)
        {
            string indexName = typeof(QuestionnaireBrowseItemsGroupByQuestionnaireIdIndex).Name;

            IQueryable<QuestionnaireAndVersionsItem> items = this.indexAccessor.Query<QuestionnaireAndVersionsItem>(indexName);

            var totalCount = items.Count();

            QuestionnaireAndVersionsItem[] currentPage =
                    items.Skip((input.Page - 1) * input.PageSize)
                    .Take(input.PageSize)
                    .ToArray();
   

            return new QuestionnaireAndVersionsView()
            {
                Items = currentPage,
                TotalCount = totalCount,
            };
        }
    }
}
