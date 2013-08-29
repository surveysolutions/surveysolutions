using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
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