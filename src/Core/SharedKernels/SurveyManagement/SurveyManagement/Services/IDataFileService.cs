using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    internal interface IDataFileService
    {
        Dictionary<Guid, string> CreateCleanedFileNamesForLevels(IDictionary<Guid, string> levels);
    }
}
