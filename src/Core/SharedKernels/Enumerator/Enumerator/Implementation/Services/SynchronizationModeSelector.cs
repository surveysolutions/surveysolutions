using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class SynchronizationModeSelector : ISynchronizationMode
    {
        private SynchronizationMode mode;

        public SynchronizationMode GetMode()
        {
            return this.mode;
        }

        public void Set(SynchronizationMode mode)
        {
            this.mode = mode;
        }
    }
}
