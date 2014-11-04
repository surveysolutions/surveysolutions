using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.SurveyManagement.Services.Sql
{
    internal interface ISqlService : IDisposable
    {
        IEnumerable<dynamic> Query(string sql, object param = null);
        IEnumerable<T> Query<T>(string sql, object param = null) where T : class;
        void ExecuteCommand(string sql, object param = null);
    }
}
