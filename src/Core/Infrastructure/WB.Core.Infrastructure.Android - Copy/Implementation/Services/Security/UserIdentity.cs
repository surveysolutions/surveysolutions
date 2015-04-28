using System;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;

namespace WB.Core.Infrastructure.Android.Implementation.Services.Security
{
    internal class UserIdentity : IUserIdentity
    {
        public bool IsAuthenticated { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public Guid UserId { get; set; }
    }
}