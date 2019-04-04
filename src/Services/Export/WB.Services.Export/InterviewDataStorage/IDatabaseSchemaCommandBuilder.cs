using System.Collections.Generic;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.InterviewDataStorage
{
    public interface IDatabaseSchemaCommandBuilder
    {
        string GenerateCreateTableScript(string tableName, List<CreateTableColumnInfo> columns);
        string GenerateCreateSchema(TenantInfo schemaName);
        string GenerateDropTable(string tableName);
        string GenerateDropSchema(string schemaName);
    }
}
