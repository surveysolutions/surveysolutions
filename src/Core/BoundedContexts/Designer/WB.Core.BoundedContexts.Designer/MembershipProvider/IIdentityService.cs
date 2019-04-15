using System;
using System.Linq;

namespace WB.Core.BoundedContexts.Designer.MembershipProvider
{
    public interface IIdentityService
    {
        IIdentityUser GetById(Guid id);
        IIdentityUser GetByNameOrEmail(string id);
        // todo remove it and replace with GetById
        string GetUserNameByEmail(string email);
    }

    class IdentityService : IIdentityService
    {
        private readonly DesignerDbContext users;

        public IdentityService(DesignerDbContext users)
        {
            this.users = users;
        }

        public IIdentityUser GetById(Guid id)
        {
            return this.users.Users.Find(id);
        }

        public IIdentityUser GetByNameOrEmail(string nameOrEmail)
        {
            if (string.IsNullOrWhiteSpace(nameOrEmail)) return null;
            var normalized = nameOrEmail.Trim().ToUpper();
            var user = users.Users.FirstOrDefault(x => x.NormalizedUserName == normalized)
                       ?? users.Users.FirstOrDefault(x => x.NormalizedEmail == normalized);
            return user;
        }

        public string GetUserNameByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var normalized = email.Trim().ToUpper();
            var user = users.Users.FirstOrDefault(x => x.NormalizedEmail == normalized);
            return user?.UserName;
        }
    }

    public interface IIdentityUser
    {
        string UserName { get; }
        string Email { get; }
        Guid Id { get; }
    }
}
