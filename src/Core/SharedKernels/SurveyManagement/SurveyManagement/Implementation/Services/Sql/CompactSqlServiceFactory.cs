using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Services.Sql;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Sql
{
    internal class CompactSqlServiceFactory : ISqlServiceFactory
    {
        private readonly IFileSystemAccessor fileSystemAccessor;

        public CompactSqlServiceFactory(IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public ISqlService CreateSqlService(string dbPath)
        {
            return new CompactSqlService(dbPath, fileSystemAccessor);
        }
    }
}
