using System.Linq;
using Raven.Client;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.User
{
    public class InterviewersViewFactory : IViewFactory<InterviewersInputModel, InterviewersView>
    {
        private IDenormalizerStorage<UserDocument> users;

        public InterviewersViewFactory(IDenormalizerStorage<UserDocument> users)
        {
            this.users = users;
        }

        #region Implementation of IViewFactory<UserBrowseInputModel,UserBrowseView>

          public InterviewersView Load(InterviewersInputModel input)
          {
              var count = users.Query().Where(u => u.Supervisor!=null).Where(u => u.Supervisor.Id == input.Supervisor.Id).Count();
              if (count == 0)
                  return new InterviewersView(input.Page, input.PageSize, count, new InterviewersItem[0]);
              // Perform the paged query
              var query = users.Query().Where(u => u.Supervisor != null).Where(u => u.Supervisor.Id == input.Supervisor.Id).Skip((input.Page - 1) * input.PageSize).Take(input.PageSize).ToArray();

              // And enact this query
              var items = query.Select(x => new InterviewersItem(x.Id, x.UserName, x.Email, x.CreationDate, x.IsLocked))
                  .ToArray();

              return new InterviewersView(
                  input.Page,
                  input.PageSize, count,
                  items);
          }

        #endregion
    }
}
