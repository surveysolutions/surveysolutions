using System.IO;
using MediatR;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Dto;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Services
{
    public class UserImportRequest : IRequest<UserImportVerificationError[]>
    {
        public string Filename { get; set; }
        public Stream FileStream { get; set; }
    }
}