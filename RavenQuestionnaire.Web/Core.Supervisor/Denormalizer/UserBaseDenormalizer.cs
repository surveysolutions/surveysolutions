using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace Core.Supervisor.Denormalizer
{
    public class UserBaseDenormalizer
    {
        protected readonly IReadSideRepositoryWriter<UserDocument> users;

        public UserBaseDenormalizer(IReadSideRepositoryWriter<UserDocument> users)
        {
            this.users = users;
        }

        protected UserLight FillResponsiblesName(UserLight responsible)
        {
            var user = this.users.GetById(responsible.Id);
            return new UserLight
                {
                    Id = responsible.Id,
                    Name = string.IsNullOrWhiteSpace(responsible.Name)
                               ? user == null ? "" : user.UserName
                               : responsible.Name
                };
        }
    }
}