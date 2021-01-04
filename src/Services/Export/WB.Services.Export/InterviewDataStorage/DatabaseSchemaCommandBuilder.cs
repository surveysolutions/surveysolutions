using System.Collections.Generic;
using System.Linq;
using System.Text;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.InterviewDataStorage
{
    public class DatabaseSchemaCommandBuilder : IDatabaseSchemaCommandBuilder
    {
        public string GenerateCreateTableScript(string tableName, List<CreateTableColumnInfo> columns)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"CREATE TABLE IF NOT EXISTS \"{tableName}\"(");

            foreach (var column in columns)
            {
                sb.Append($"\"{column.Name}\" {column.SqlType} ");
                sb.Append(column.IsNullable ? " NULL " : " NOT NULL ");
                if (column.DefaultValue != null)
                    sb.Append(" DEFAULT " + column.DefaultValue);
                sb.AppendLine(",");
            }

            sb.AppendLine($"PRIMARY KEY ({string.Join(" , ", columns.Where(c => c.IsPrimaryKey).Select(c => $"\"{c.Name}\""))})");
            sb.AppendLine(");");
            return sb.ToString();
        }

        public string GenerateCreateSchema(TenantInfo tenant)
        {
            var schemaName = tenant.SchemaName();
            return $@"CREATE SCHEMA IF NOT EXISTS ""{schemaName}""; COMMENT ON SCHEMA ""{schemaName}"" IS '{tenant.ShortName}';";
        }

        public string GenerateDropTable(string tableName)
        {
            return $"DROP TABLE IF EXISTS \"{tableName}\" CASCADE;";
        }

        public string GenerateDropSchema(string schemaName)
        {
            return $@"drop schema if exists ""{schemaName}"";";
        }
    }
}
