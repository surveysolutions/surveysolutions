using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.Designer
{
    public static class Extensions
    {
        public static void AddUserWithId(this DesignerDbContext dbContext, Guid userId)
        {
            dbContext.Add(new DesignerIdentityUser
            {
                Id = userId
            });
            dbContext.SaveChanges();
        }

        public static void SetupLoggedInUser(this ControllerBase controller, Guid userId)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            }));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }
    }
}
