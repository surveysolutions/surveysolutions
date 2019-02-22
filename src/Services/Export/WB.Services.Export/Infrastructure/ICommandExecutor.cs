using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WB.Services.Export.Infrastructure
{
    public interface ICommandExecutor
    {
        Task ExecuteNonQueryAsync(DbCommand command, CancellationToken cancellationToken);
    }

    class CommandExecutor : ICommandExecutor
    {
        private readonly TenantDbContext tenantDbContext;

        public CommandExecutor(TenantDbContext tenantDbContext)
        {
            this.tenantDbContext = tenantDbContext;
        }

        public Task ExecuteNonQueryAsync(DbCommand command, CancellationToken cancellationToken)
        {
            var db = tenantDbContext.Database.GetDbConnection();
            command.Connection = db;
            return command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
