using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    internal interface IDataFileService
    {
        Dictionary<ValueVector<Guid>, string> CreateCleanedFileNamesForLevels(IDictionary<ValueVector<Guid>, string> levels);
        string CreateValidFileName(string name);
    }
}
