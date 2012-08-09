using System.Linq;
using Raven.Client;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Views.User
{
    public class UserBrowseViewFactory:  IViewFactory<UserBrowseInputModel, UserBrowseView>
    {
       /* private IDocumentSession documentSession;
        public UserBrowseViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }
*/

        private readonly IDenormalizerStorage<UserDocument> documentItemSession;
        public UserBrowseViewFactory(IDenormalizerStorage<UserDocument> documentItemSession)
        {
            this.documentItemSession = documentItemSession;
        }


        #region Implementation of IViewFactory<UserBrowseInputModel,UserBrowseView>

          public UserBrowseView Load(UserBrowseInputModel input)
          {
              var count = documentItemSession.Query().Count();
              if (count == 0)
                  return new UserBrowseView(input.Page, input.PageSize, count, new UserBrowseItem[0]);
              // Perform the paged query
              var query = documentItemSession.Query().Where(input.Expression).Skip((input.Page - 1) * input.PageSize)
                  .Take(input.PageSize);
            


              // And enact this query
              var items = query
                  .Select(
                      x =>
                      new UserBrowseItem(x.Id, x.UserName, x.Email, x.CreationDate, x.IsLocked, x.Supervisor,
                                         x.Location.Location))
                  .ToArray();

              return new UserBrowseView(
                  input.Page,
                  input.PageSize, count,
                  items.ToArray());

          }

        #endregion
    }
}
