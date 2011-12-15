using System;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;

namespace RavenQuestionnaire.Core.Repositories
{
    public class UserRepository : EntityRepository<User, UserDocument>, IUserRepository
    {
        public UserRepository(IDocumentSession documentSession) : base(documentSession) { }

        protected override User Create(UserDocument doc)
        {
            return new User(doc);
        }
        public override void Remove(User entity)
        {
            throw new InvalidOperationException("User can't be deleted");
        }
    }
}
