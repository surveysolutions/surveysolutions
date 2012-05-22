using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SynchronizationMessages.Synchronization
{
    public interface ICustomSerializable
    {
        void WriteTo(Stream stream);
        void InitializeFrom(Stream stream);
    }
}
