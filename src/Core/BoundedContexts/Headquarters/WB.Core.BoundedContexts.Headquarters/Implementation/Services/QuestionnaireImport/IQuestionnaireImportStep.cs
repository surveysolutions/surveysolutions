using System;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    public interface IQuestionnaireImportStep
    {
        bool IsNeedProcessing();
        Task DownloadFromDesignerAsync(IProgress<int> progress);
        void SaveData(IProgress<int> progress);
    }
}
