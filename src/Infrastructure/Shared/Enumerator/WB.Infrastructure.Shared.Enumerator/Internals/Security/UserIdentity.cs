using System;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Infrastructure.Shared.Enumerator.Internals.Security
{
    internal class UserIdentity : IUserIdentity
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public Guid UserId { get; set; }
    }
}