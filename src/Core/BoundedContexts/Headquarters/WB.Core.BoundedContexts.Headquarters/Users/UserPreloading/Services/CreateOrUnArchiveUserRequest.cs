using MediatR;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Dto;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Services
{
    public class CreateOrUnArchiveUserRequest : IRequest<UserToImport>
    {

    }
}