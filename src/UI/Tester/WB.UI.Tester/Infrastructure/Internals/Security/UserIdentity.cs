using System;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.UI.Tester.Infrastructure.Internals.Security
{
    internal class UserIdentity : IUserIdentity
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public Guid UserId { get; set; }
    }
}