using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.SharedKernels.SurveyManagement.Services.Sql
{
    internal interface ISqlServiceFactory
    {
        ISqlService CreateSqlService(string dbPath);
    }
}
