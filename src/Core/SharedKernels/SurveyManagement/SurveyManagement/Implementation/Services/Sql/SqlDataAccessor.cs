using System;
using System.Collections.Generic;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Services.Sql;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Sql
{
    internal class SqlDataAccessor : ISqlDataAccessor
    {
        private const string interviewActions = "interview_actions";
        private const string dataFileName = "data.sqlite";

        private const string allDataFolder = "AllData";
        private const string approvedDataFolder = "ApprovedData";
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly List<string> actionFileColumns;

        public SqlDataAccessor(IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            actionFileColumns = new List<string> { this.InterviewIdColumnName, "Action", "Originator", "Role", "Date", "Time" };
        }

        public string GetAllDataFolder(string basePath)
        {
            return this.fileSystemAccessor.CombinePath(basePath, AllDataFolder);
        }

        public string GetApprovedDataFolder(string basePath)
        {
            return this.fileSystemAccessor.CombinePath(basePath, ApprovedDataFolder);
        }
        public string InterviewActionsTableName { get { return interviewActions; } }
        public string DataFileName { get { return dataFileName; } }
        public string AllDataFolder { get { return allDataFolder; } }
        public string ApprovedDataFolder { get { return approvedDataFolder; } }
        public List<string> ActionFileColumns { get { return actionFileColumns; } }

        public string InterviewIdColumnName { get { return "InterviewId"; } }
        public string DataColumnName { get { return "Data"; } }
    }
}
