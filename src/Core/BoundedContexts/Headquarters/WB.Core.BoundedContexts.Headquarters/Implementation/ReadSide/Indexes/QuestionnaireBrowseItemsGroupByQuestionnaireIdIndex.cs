using System.Linq;
using Raven.Client.Indexes;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.ReadSide.Indexes
{
    public class QuestionnaireBrowseItemsGroupByQuestionnaireIdIndex : AbstractIndexCreationTask<QuestionnaireBrowseItem, QuestionnaireAndVersionsItem>
    {
        public QuestionnaireBrowseItemsGroupByQuestionnaireIdIndex()
        {
            this.Map = docs => from doc in docs
                select new QuestionnaireAndVersionsItem()
                {
                    QuestionnaireId = doc.QuestionnaireId,
                    Title = doc.Title,
                    Versions = new[] { doc.Version }
                };

            this.Reduce = results => from result in results
                group result by result.QuestionnaireId into g
                select new QuestionnaireAndVersionsItem
                {
                    QuestionnaireId = g.Key,
                    Title = g.Aggregate((q1, q2) => q1.Versions.Max() > q2.Versions.Max() ? q1 : q2).Title,
                    Versions = g.SelectMany(x => x.Versions).ToArray()
                };
        }
    }
}