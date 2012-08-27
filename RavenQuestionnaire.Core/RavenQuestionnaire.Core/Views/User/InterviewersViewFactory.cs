using System.Linq;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;


namespace RavenQuestionnaire.Core.Views.User
{
    public class InterviewersViewFactory : IViewFactory<InterviewersInputModel, InterviewersView>
    {
        private IDenormalizerStorage<UserDocument> users;
        private IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession;

        public InterviewersViewFactory(IDenormalizerStorage<UserDocument> users, IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentSession)
        {
            this.users = users;
            this.documentItemSession = documentSession;
        }

        #region Implementation of IViewFactory<UserBrowseInputModel,UserBrowseView>

          public InterviewersView Load(InterviewersInputModel input)
          {
              var count = users.Query().Where(u => u.Supervisor!=null).Where(u => u.Supervisor.Id == input.Supervisor.Id).Count();
              if (count == 0)
                  return new InterviewersView(input.Page, input.PageSize, count, new InterviewersItem[0], input.Supervisor.Id, input.Supervisor.Name);
              var query = users.Query().Where(u => u.Supervisor != null).Where(u => u.Supervisor.Id == input.Supervisor.Id);
              var questionnaire =
                  documentItemSession.Query().Where(t => t.Responsible != null);
              var items = query.Select(x => new InterviewersItem(x.Id, x.UserName, x.Email, x.CreationDate, x.IsLocked,
                  questionnaire.Where(t => t.Responsible.Id == x.Id).Count(),
                  questionnaire.Where(t => t.Responsible.Id == x.Id).Where(t => t.Status == SurveyStatus.Complete).Count(),
                  questionnaire.Where(t => t.Responsible.Id == x.Id).Where(t => t.Status != SurveyStatus.Complete).Count()));
              if (input.Orders.Count > 0)
              {
                  items = input.Orders[0].Direction == OrderDirection.Asc
                          ? items.OrderBy(input.Orders[0].Field)
                          : items.OrderByDescending(input.Orders[0].Field);
              }
              items = items.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize);
              return new InterviewersView(
                  input.Page,
                  input.PageSize, count,
                  items, input.Supervisor.Id, input.Supervisor.Name);
          }

        #endregion
    }
}
