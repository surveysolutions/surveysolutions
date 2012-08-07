using System.Linq;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.User
{
    public class InterviewersViewFactory : IViewFactory<InterviewersInputModel, InterviewersView>
    {
          private IDocumentSession documentSession;

          public InterviewersViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }

        #region Implementation of IViewFactory<UserBrowseInputModel,UserBrowseView>

          public InterviewersView Load(InterviewersInputModel input)
          {
              var supervisorId = IdUtil.CreateUserId(input.Supervisor.Id);
              var count = documentSession.Query<UserDocument>().Where(u => u.Supervisor.Id == supervisorId).Count();
              if (count == 0)
                  return new InterviewersView(input.Page, input.PageSize, count, new InterviewersItem[0]);
              // Perform the paged query
              var query = documentSession.Query<UserDocument>().Where(u => u.Supervisor.Id == supervisorId).Skip((input.Page - 1) * input.PageSize).Take(input.PageSize).ToArray();

              // And enact this query
              var items = query.Select(x => new InterviewersItem(x.Id, x.UserName, x.Email, x.CreationDate, x.IsLocked))
                  .ToArray();

              return new InterviewersView(
                  input.Page,
                  input.PageSize, count,
                  items.ToArray());
          }

        #endregion
    }
}
