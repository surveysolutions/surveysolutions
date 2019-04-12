using Microsoft.AspNetCore.Http;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Designer1.Extensions;

namespace WB.UI.Designer.Code
{
    public class LoggedInUser : ILoggedInUser
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public LoggedInUser(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public string Id => httpContextAccessor.HttpContext.User.GetId().FormatGuid();
        public string Login => httpContextAccessor.HttpContext.User.GetUserName();
        public bool IsAdmin => httpContextAccessor.HttpContext.User.IsAdmin();
    }
}
