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
            var user =  this.users.Query().FirstOrDefault(u => u.Id == input.UserId);
            var items = new InterviewerView(user.UserName, user.Id, new List<InterviewerGroupView>());
            var docs = this.documentItemSession.Query().Where(q => q.Responsible != null && q.Responsible.Id == user.Id);
            if (!string.IsNullOrEmpty(input.TemplateId))
            {
                var interviewerGroupView = SelectItems(input.TemplateId, docs, input);
                if (interviewerGroupView.Items.Count>0)
                    items.Items.Add(interviewerGroupView);
            }
            else
            {
                var gr = docs.GroupBy(t=>t.TemplateId);
                foreach (
                    var interviewerGroupView in
                        gr.ToList().Select(template => SelectItems(template.Key, docs, input)).Where(
                            interviewerGroupView => interviewerGroupView.Items.Count > 0))
                    items.Items.Add(interviewerGroupView);
            }
            return items;
        }
        
        #endregion

        #region PrivateMethod

        private InterviewerGroupView SelectItems(string templateId, IQueryable<CompleteQuestionnaireBrowseItem> docs, InterviewerInputModel input)
        {
            int count = docs.Where(t => t.TemplateId == templateId).Count();
            if (count == 0)
                return new InterviewerGroupView(templateId, string.Empty, new List<CompleteQuestionnaireBrowseItem>(), input.Order, input.Page, input.PageSize, count);
            docs = docs.Where(t => t.TemplateId == templateId);
            if (input.Orders.Count > 0)
            {
                var o = docs.Where(t => t.FeaturedQuestions.Count() != 0).SelectMany(t => t.FeaturedQuestions).Select(y => y.PublicKey.ToString()).Distinct().ToList();
                if (o.Contains(input.Orders[0].Field))
                {
                    docs = input.Orders[0].Direction == OrderDirection.Asc
                            ? docs.OrderBy(
                                t =>
                                t.FeaturedQuestions.Where(y => y.PublicKey.ToString() == input.Orders[0].Field).Select(
                                    x => x.AnswerValue).FirstOrDefault())
                            : docs.OrderByDescending(
                                t =>
                                t.FeaturedQuestions.Where(y => y.PublicKey.ToString() == input.Orders[0].Field).Select(
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
            return new InterviewerGroupView(
                    templateId,
                    docs.ToList().Count>0 ? docs.ToList().FirstOrDefault().QuestionnaireTitle : string.Empty,
                    docs.ToList(), input.Order, input.Page,
                    input.PageSize, count);
        }

        #endregion
    }
}