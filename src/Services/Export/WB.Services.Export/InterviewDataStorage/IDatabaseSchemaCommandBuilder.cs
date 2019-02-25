using System.Collections.Generic;

namespace WB.Services.Export.InterviewDataStorage
{
    public interface IDatabaseSchemaCommandBuilder
    {
        string GenerateCreateTableScript(string tableName, List<CreateTableColumnInfo> columns);
        string GenerateCreateSchema(string schemaName);
        string GenerateDropTable(string tableName);
        string GenerateDropSchema(string schemaName);
    }
}
