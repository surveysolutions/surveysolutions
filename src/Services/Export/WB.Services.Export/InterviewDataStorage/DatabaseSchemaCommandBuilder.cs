using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            sb.AppendLine(")");
            return sb.ToString();
        }

        public string GenerateCreateSchema(string schemaName)
        {
            return $"CREATE SCHEMA IF NOT EXISTS \"{schemaName}\"";
        }

        public string GenerateDropTable(string tableName)
        {
            return $"DROP TABLE IF EXISTS \"{tableName}\" CASCADE ";
        }

        public string GenerateDropSchema(string schemaName)
        {
            return $@"drop schema if exists ""{schemaName}""";
        }
    }
}
