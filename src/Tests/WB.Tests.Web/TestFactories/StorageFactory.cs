using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Tests.Web.TestFactories
{
    public class StorageFactory
    {
        public DesignerDbContext InMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<DesignerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString("N"))
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            
            var dbContext = new DesignerDbContext(options);

            return dbContext;
        }
    }
}
