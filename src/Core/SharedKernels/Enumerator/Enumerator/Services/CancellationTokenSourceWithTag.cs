using System.Threading;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public class CancellationTokenSourceWithTag : CancellationTokenSource
    {
        public string Tag { get; }

        public CancellationTokenSourceWithTag(string tag)
        {
            Tag = tag;
        }
    }    
}
