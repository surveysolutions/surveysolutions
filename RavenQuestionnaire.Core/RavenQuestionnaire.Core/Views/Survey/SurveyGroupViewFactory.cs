using System.Linq;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;

namespace RavenQuestionnaire.Core.Views.Survey
{
    public class SurveyGroupViewFactory : IViewFactory<SurveyGroupInputModel, SurveyGroupView>
    {
        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession;

        public SurveyGroupViewFactory(IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession)
        {
            this.documentItemSession = documentItemSession;
        }

        public SurveyGroupView Load(SurveyGroupInputModel input)
        {
            var count = documentItemSession.Query().Where(x => x.TemplateId == input.Id).ToList().Count;
            if (count==0)
                return new SurveyGroupView(input.Page, input.PageSize, input.QuestionnaireId, 0, new CompleteQuestionnaireBrowseItem[0], input.Id);
            IQueryable<CompleteQuestionnaireBrowseItem> query = documentItemSession.Query().Where(v=>v.TemplateId==input.Id);
            if (!string.IsNullOrEmpty(input.QuestionnaireId))
            {
                query = query.Where(t => t.CompleteQuestionnaireId == input.QuestionnaireId);
                if (input.Orders.Count > 0)
                    query = DefineOrderBy(query, input);
            }
            else
            {
                if (input.Orders.Count > 0)
                    query = DefineOrderBy(query, input);
            }
            query = query.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize);
            return new SurveyGroupView(input.Page, input.PageSize, query.FirstOrDefault()!=null ? query.FirstOrDefault().QuestionnaireTitle : input.Id, 0, query, input.Id);
        }



        private IQueryable<CompleteQuestionnaireBrowseItem> DefineOrderBy(IQueryable<CompleteQuestionnaireBrowseItem> query, SurveyGroupInputModel input)
        {
            var o = query.SelectMany(t => t.FeaturedQuestions).Select(y => y.QuestionText).Distinct().ToList();
            if (o.Contains(input.Orders[0].Field))
            {
                query = input.Orders[0].Direction == OrderDirection.Asc
                        ? query.OrderBy(
                            t =>
                            t.FeaturedQuestions.Where(y => y.QuestionText == input.Orders[0].Field).Select(
                                x => x.AnswerValue).FirstOrDefault())
                        : query.OrderByDescending(
                            t =>
                            t.FeaturedQuestions.Where(y => y.QuestionText == input.Orders[0].Field).Select(
                                x => x.AnswerValue).FirstOrDefault());
            }
            else
            {
                query = input.Orders[0].Direction == OrderDirection.Asc
                        ? query.OrderBy(input.Orders[0].Field)
                        : query.OrderByDescending(input.Orders[0].Field);
            }
            return query;
        }
    }
}
