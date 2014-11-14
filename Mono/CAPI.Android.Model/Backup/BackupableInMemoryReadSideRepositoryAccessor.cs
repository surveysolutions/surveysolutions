using Main.DenormalizerStorage;
using WB.Core.Infrastructure.Backup;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace CAPI.Android.Core.Model.Backup
{
    public class BackupableInMemoryReadSideRepositoryAccessor<TView> : InMemoryReadSideRepositoryAccessor<TView>,IBackupable where TView : class, IView
    {
        public string GetPathToBackupFile()
        {
            return null;
        }

        public void RestoreFromBackupFolder(string path)
        {
           this.Clear();
        }
    }
}