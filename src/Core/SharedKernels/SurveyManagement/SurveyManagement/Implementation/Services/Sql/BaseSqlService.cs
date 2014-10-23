using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            return sqlService.ExecuteReader("select table_name from information_schema.tables where TABLE_TYPE = 'TABLE'").Select(table => table[0].ToString());
        }

        protected IEnumerable<string> GetListOfColumns(ISqlService sqlService, string tableName)
        {
            return sqlService.ExecuteReader("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tableName + "'").Select(table => table[0].ToString());
        } 
    }
}
