using System.Collections.Generic;

namespace Client
{
    interface ISaveFileArchive
    {
        void SaveArchive(byte[] data);

        byte[] LoadArchive();

        string getTargetName();

        void FlushTargetList();

        
    }
}
