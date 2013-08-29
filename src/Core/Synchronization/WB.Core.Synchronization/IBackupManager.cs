using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SynchronizationMessages.Export;

namespace WB.Core.Synchronization
{
    public interface IBackupManager
    {
        ZipFileData Backup();
        /// <summary>
        /// restore from zip archive
        /// <b>current implementation of restore DO NOT clean up database. it is only adding new date</b>
        /// </summary>
        /// <param name="zipData"></param>
        
        void Restore(List<string> zipData);
    }
}
