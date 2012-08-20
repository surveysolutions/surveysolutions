using System.Linq;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;

namespace RavenQuestionnaire.Core.Views.User
{
    public class InterviewerViewFactory : IViewFactory<InterviewerInputModel, InterviewerView>
    {
        private IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession;
        private IDenormalizerStorage<UserDocument> users;

        public InterviewerViewFactory(IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentSession, IDenormalizerStorage<UserDocument> users)
        {
            this.documentItemSession = documentSession;
            this.users = users;
        }

        #region Implementation of IViewFactory<UserViewInputModel,UserView>

        public InterviewerView Load(InterviewerInputModel input)
        {
            var count = this.documentItemSession.Query().Where(input.Expression).Count();
            var user =  this.users.Query().FirstOrDefault(u => u.Id == input.UserId);
            if (count == 0)
            {
                return new InterviewerView(input.Page, input.PageSize, count, user.UserName, user.Id, new List<CompleteQuestionnaireBrowseItem>());
            }
            var docs = this.documentItemSession.Query().Where(q =>q.Responsible!=null && q.Responsible.Id == user.Id);
            if (input.Orders.Count>0)
            {
                var o = docs.Where(t=>t.FeaturedQuestions.Count()!=0).SelectMany(t => t.FeaturedQuestions).Select(y => y.QuestionText).Distinct().ToList();
                if (o.Contains(input.Orders[0].Field))
                {
                    docs = input.Orders[0].Direction == OrderDirection.Asc
                            ? docs.OrderBy(
                                t =>
                                t.FeaturedQuestions.Where(y => y.QuestionText == input.Orders[0].Field).Select(
                                    x => x.AnswerValue).FirstOrDefault())
                            : docs.OrderByDescending(
                                t =>
                                t.FeaturedQuestions.Where(y => y.QuestionText == input.Orders[0].Field).Select(
                                    x => x.AnswerValue).FirstOrDefault());
                }
                else
                {
                    docs = input.Orders[0].Direction == OrderDirection.Asc
                            ? docs.OrderBy(input.Orders[0].Field)
                            : docs.OrderByDescending(input.Orders[0].Field);
                }
            }
            docs = docs.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize);
            return new InterviewerView(input.Page, input.PageSize, count, user.UserName, user.Id, docs.ToList());
        }


        #endregion
    }
}
