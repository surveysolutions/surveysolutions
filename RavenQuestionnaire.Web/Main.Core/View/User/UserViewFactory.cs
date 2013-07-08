using System;
using System.Linq;
using Main.Core.Documents;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace Main.Core.View.User
{
    public class UserViewFactory : IViewFactory<UserViewInputModel, UserView>
    {
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

                    return new UserView(doc.PublicKey, doc.UserName, doc.Password, doc.Email, doc.CreationDate, doc.Roles, doc.IsLocked, doc.Supervisor, doc.Location.Id);
                });
            return result;
        }
    }
}