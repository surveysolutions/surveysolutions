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
        string Store(Stream preloadedDataFile, string fileName);
        PreloadedContentMetaData GetPreloadedDataMetaInformation(string id);
        PreloadedDataByFile[] GetPreloadedData(string id);
    }
}
