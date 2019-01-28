using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace WB.Persistence.Headquarters.Migrations
{
    internal static class Helpers
    {
        public static bool IsTableExistsInSchema(this IDbConnection db, string schema, string table)
        {
            var res = db.QuerySingle<int>(@"SELECT EXISTS (
                       SELECT 1
                       FROM   information_schema.tables 
                       WHERE  table_schema = @schema
                       AND    table_name = @table
                    );", new { table, schema });

            return res > 0;
        }

    }
}
