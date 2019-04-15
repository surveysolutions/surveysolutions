using System;
using WB.Core.BoundedContexts.Designer.MembershipProvider;

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
    }
}
