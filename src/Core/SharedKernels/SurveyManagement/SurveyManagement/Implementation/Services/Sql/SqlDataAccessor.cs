using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Services.Sql;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Sql
{
    internal class SqlDataAccessor : ISqlDataAccessor
    {
        protected const string parentId = "ParentId";
        protected const string interviewActions = "interview_actions";
        protected const string dataFile = "data.sdf";

        private const string allDataFolder = "AllData";
        private const string approvedDataFolder = "ApprovedData";
        protected readonly IFileSystemAccessor fileSystemAccessor;

        public SqlDataAccessor(IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public IEnumerable<string> GetListofTables(ISqlService sqlService)
        {
            return sqlService.Query<string>("select table_name from information_schema.tables where TABLE_TYPE = 'TABLE'");
        }

        public IEnumerable<string> GetListOfColumns(ISqlService sqlService, string tableName)
        {
            return sqlService.Query<string>("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @tableName",
                new { tableName });
        }

        public string GetAllDataFolder(string basePath)
        {
            return this.fileSystemAccessor.CombinePath(basePath, AllDataFolder);
        }

        public string GetApprovedDataFolder(string basePath)
        {
            return this.fileSystemAccessor.CombinePath(basePath, ApprovedDataFolder);
        }

        public string ParentId { get { return parentId; }}
        public string InterviewActions { get { return interviewActions; } }
        public string DataFile { get { return dataFile; }}
        public string AllDataFolder { get { return allDataFolder; } }
        public string ApprovedDataFolder { get { return approvedDataFolder; } }
    }
}
