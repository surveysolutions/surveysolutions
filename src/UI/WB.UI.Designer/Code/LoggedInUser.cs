using System;
using Microsoft.AspNetCore.Http;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.UI.Designer.Extensions;

namespace WB.UI.Designer.Code
{
    public class LoggedInUser : ILoggedInUser
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public LoggedInUser(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public Guid Id => httpContextAccessor.HttpContext!.User.GetId();
        public string Login => httpContextAccessor.HttpContext!.User.GetUserName();
        public bool IsAdmin => httpContextAccessor.HttpContext!.User.IsAdmin();
    }
}
