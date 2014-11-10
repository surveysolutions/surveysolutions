using System.Linq;
using Main.Core.View;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.Indexes;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories
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
