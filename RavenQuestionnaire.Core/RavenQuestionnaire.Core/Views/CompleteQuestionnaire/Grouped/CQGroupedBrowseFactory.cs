using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Grouped
{
    public class CQGroupedBrowseFactory : IViewFactory<CQGroupedBrowseInputModel, CQGroupedBrowseView>
    {
        private IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession;
        private IDenormalizerStorage<CQGroupItem> documentGroupSession;

        public CQGroupedBrowseFactory(IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession, IDenormalizerStorage<CQGroupItem> documentGroupSession)
        {
            this.documentItemSession = documentItemSession;
            this.documentGroupSession = documentGroupSession;
        }
        #region Implementation of IViewFactory<CQGroupedBrowseInputModel,CQGroupedBrowseView>

        public CQGroupedBrowseView Load(CQGroupedBrowseInputModel input)
        {
            var questionnaires = this.documentItemSession.Query();
            var templates = this.documentGroupSession.Query();

            var retval = new CQGroupedBrowseView(0, 100, 100, templates.ToList());
            foreach (CQGroupItem cqGroupItem in retval.Groups)
            {
                CQGroupItem item = cqGroupItem;
                List<CompleteQuestionnaireBrowseItem> complete;
                if (input.InterviewerId.HasValue)
                    complete =
                        questionnaires.Where(
                            q =>
                            q.Responsible != null && q.Responsible.Id == input.InterviewerId.Value.ToString() &&
                            q.TemplateId == item.SurveyId).ToList();
                else
                    complete =
                            questionnaires.Where(
                                q =>
                                q.TemplateId == item.SurveyId).ToList();
                cqGroupItem.Items = complete;
                cqGroupItem.TotalCount = complete.Count;
            }
            return retval;

        }
        protected Expression<Func<CompleteQuestionnaireBrowseItem, bool>> BuildPredicate(CQGroupedBrowseInputModel input, string surveyId)
        {
            IList<Expression<Func<CompleteQuestionnaireBrowseItem, bool>>> predicats = new List<Expression<Func<CompleteQuestionnaireBrowseItem, bool>>>();
            predicats.Add((q) => q.TemplateId == surveyId);
            if (input.InterviewerId.HasValue)
            {

                predicats.Add(
                    (q) => q.Responsible != null && q.Responsible.Id == input.InterviewerId.ToString());
            }
            return AndAll(predicats);
        }
        Expression<Func<T, bool>> AndAll<T>(IEnumerable<Expression<Func<T, bool>>> expressions)
        {

            if (expressions == null)
            {
                throw new ArgumentNullException("expressions");
            }
            if (expressions.Count() == 0)
            {
                return t => true;
            }
            Type delegateType = typeof(Func<,>)
                .GetGenericTypeDefinition()
                .MakeGenericType(new[]
                                     {
                                         typeof (T),
                                         typeof (bool)
                                     }
                );
            var combined = expressions
                .Cast<Expression>()
                .Aggregate((e1, e2) => Expression.AndAlso(e1, e2));
            return (Expression<Func<T, bool>>)Expression.Lambda(delegateType, combined);
        }

        #endregion
    }
}
