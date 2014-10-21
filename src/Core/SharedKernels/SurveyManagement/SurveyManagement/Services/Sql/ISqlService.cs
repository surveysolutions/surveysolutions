using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.SurveyManagement.Services.Sql
{
    internal interface ISqlService : IDisposable
    {
        void ExecuteCommand(string commandText);
        void ExecuteCommands(IEnumerable<string> commands);
        object[][] ExecuteReader(string query);
    }
}
