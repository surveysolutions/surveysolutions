using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Views.User
{
    public class UserViewFactory : IViewFactory<UserViewInputModel, UserView>
    {
        private IDocumentSession documentSession;

        public UserViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }

        #region Implementation of IViewFactory<UserViewInputModel,UserView>

        public UserView Load(UserViewInputModel input)
        {
            UserDocument doc = null;
            if (!string.IsNullOrEmpty(input.UserId))
                doc = this.documentSession.Load<UserDocument>(input.UserId);
            else if (!string.IsNullOrEmpty(input.UserName) && string.IsNullOrEmpty(input.Password))
            {
                doc =
                    this.documentSession.Query<UserDocument>().Where(u => u.UserName.Equals(input.UserName)).
                        FirstOrDefault();

            }
            if (!string.IsNullOrEmpty(input.UserName) && !string.IsNullOrEmpty(input.Password))
            {
                doc =
                    this.documentSession.Query<UserDocument>().Where(
                        u => u.UserName.Equals(input.UserName) /*&& u.Password.Equals(input.Password)*/).FirstOrDefault();
                if (doc!=null && doc.Password != input.Password)
                    return null;

            }
            if (doc == null || doc.IsDeleted)
            {
                return null;
            }
            return new UserView(doc.Id, doc.UserName, doc.Password, doc.Email, doc.CreationDate, doc.Roles, doc.IsLocked,
                                doc.Supervisor, doc.Location.Id);
        }


        #endregion
    }
}
