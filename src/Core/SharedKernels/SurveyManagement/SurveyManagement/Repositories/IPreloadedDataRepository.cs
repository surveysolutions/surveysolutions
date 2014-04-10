using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;

namespace WB.Core.SharedKernels.SurveyManagement.Repositories
{
    public interface IPreloadedDataRepository
    {
        Guid Store(Stream preloadedDataFile, string fileName);
        PreloadedContentMetaData GetPreloadedDataMetaInformation(Guid id);
        PreloadedDataByFile[] GetPreloadedData(Guid id);
    }
}
