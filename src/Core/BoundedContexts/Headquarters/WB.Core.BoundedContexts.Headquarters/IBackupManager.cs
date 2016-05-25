using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters
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
