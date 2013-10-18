using Main.DenormalizerStorage;
using WB.Core.Infrastructure.Backup;
using WB.Core.Infrastructure.ReadSide;

namespace CAPI.Android.Core.Model.Backup
{
    public class BackupableInMemoryReadSideRepositoryAccessor<TView> : InMemoryReadSideRepositoryAccessor<TView>,IBackupable where TView : class, IView
    {
        public string GetPathToBakupFile()
        {
            return null;
        }

        public void RestoreFromBakupFolder(string path)
        {
           this.Clear();
        }
    }
}