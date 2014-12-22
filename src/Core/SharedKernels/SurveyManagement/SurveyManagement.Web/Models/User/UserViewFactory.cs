using System;
using System.Linq;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models.User
{
    public class UserViewFactory : IViewFactory<UserViewInputModel, UserView>
    {
        // in this case we use writer here because we want to make sure login is performed on the latest version of data and we understand that indexing may take some time
        private readonly IQueryableReadSideRepositoryReader<UserDocument> users;

        public UserViewFactory(IQueryableReadSideRepositoryReader<UserDocument> users)
        {
            this.users = users;
        }
        
        public UserView Load(UserViewInputModel input)
        {
            UserView result = this.users.Query(queryableUsers =>
                {
                    UserDocument doc = null;
                    if (input.UserId != Guid.Empty)
                    {
                        doc = queryableUsers.FirstOrDefault(u => u.PublicKey == input.UserId);
                    }
                    else if (!string.IsNullOrEmpty(input.UserName) && string.IsNullOrEmpty(input.Password))
                    {
                        doc = queryableUsers.FirstOrDefault(u => u.UserName == input.UserName);
                    }

                    if (!string.IsNullOrEmpty(input.UserName) && !string.IsNullOrEmpty(input.Password))
                    {
                        doc = queryableUsers.FirstOrDefault(u => u.UserName == input.UserName);
                        if (doc != null && doc.Password != input.Password)
                        {
                            return null;
                        }
                    }

                    if (doc == null || doc.IsDeleted)
                    {
                        return null;
                    }

                    return new UserView(doc.PublicKey, doc.UserName, doc.Password, doc.Email, doc.CreationDate, doc.Roles, doc.IsLockedBySupervisor, doc.IsLockedByHQ, doc.Supervisor);
                });
            return result;
        }
    }
}