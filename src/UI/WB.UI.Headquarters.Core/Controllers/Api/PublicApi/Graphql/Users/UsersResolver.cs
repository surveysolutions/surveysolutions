#nullable enable
using System.Linq;
using HotChocolate;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Users
{
    public class UsersResolver
    {
        public IQueryable<HqUser> GetUsers(
            [Service] IUnitOfWork unitOfWork,
            [Service] IAuthorizedUser authorizedUser)
        {
            unitOfWork.DiscardChanges();
            var query = unitOfWork.Session.Query<HqUser>();

            return query;
        }
    }
}
