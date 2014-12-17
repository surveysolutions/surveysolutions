using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport
{
    internal class FileBasedDataExportRepositorySettings
    {
        public FileBasedDataExportRepositorySettings(int maxCountOfCachedEntities)
        {
            this.MaxCountOfCachedEntities = maxCountOfCachedEntities;
        }

        public int MaxCountOfCachedEntities { get; private set; }
    }
}
