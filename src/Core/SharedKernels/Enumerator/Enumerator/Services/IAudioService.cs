using System;
using System.IO;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IAudioService : IDisposable
    {
        void Start(int bitRate);
        void Stop();
        Stream GetLastRecord();
        TimeSpan GetDuration();
    }
}
