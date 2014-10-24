using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using WB.Core.SharedKernels.SurveyManagement.Services.Sql;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Sql
{
    internal class BaseSqlService
    {
        protected const string parentId = "ParentId";
        protected const string interviewActions = "interview_actions";
        protected const string dataFile = "data.sdf";

        protected IEnumerable<string> GetListofTables(ISqlService sqlService)
        {
            return sqlService.Query<string>("select table_name from information_schema.tables where TABLE_TYPE = 'TABLE'");
        }

        protected IEnumerable<string> GetListOfColumns(ISqlService sqlService, string tableName)
        {
            return sqlService.Query<string>("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @tableName",
                new { tableName });
        } 
    }
}
