using System;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Implementation.Repositories
{
    internal class UserRepository : IPlainAggregateRootRepository<User>
    {
        private readonly IPlainStorageAccessor<User> storage;

        public UserRepository(IPlainStorageAccessor<User> storage)
        {
            this.storage = storage;
        }

        public User Get(Guid aggregateId) => this.storage.GetById(aggregateId.FormatGuid());

        public void Save(User user) => this.storage.Store(user, user.Id.FormatGuid());
    }
}