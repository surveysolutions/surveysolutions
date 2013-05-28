using System;
using System.Threading.Tasks;

namespace WB.Core.Infrastructure.Implementation
{
    internal class ReadLayerService : IReadLayerStatusService, IReadLayerAdministrationService
    {
        private static readonly object LockObject = new object();

        private static string statusMessage = "No administration operations were performed so far.";
        private static bool areViewsBeingRebuiltNow = false;

        #region IReadLayerStatusService implementation

        public bool AreViewsBeingRebuiltNow()
        {
            return areViewsBeingRebuiltNow;
        }

        #endregion // IReadLayerStatusService implementation

        #region IReadLayerAdministrationService implementation

        public string GetReadableStatus()
        {
            return string.Format("{0}{1}Are views being rebuilt now: {2}",
                statusMessage, Environment.NewLine, areViewsBeingRebuiltNow ? "Yes" : "No");
        }

        public void RebuildAllViewsAsync()
        {
            new Task(this.RebuildAllViews).Start();
        }

        #endregion // IReadLayerAdministrationService implementation

        private void RebuildAllViews()
        {
            if (!areViewsBeingRebuiltNow)
            {
                lock (LockObject)
                {
                    if (!areViewsBeingRebuiltNow)
                    {
                        this.RebuildAllViewsImpl();
                    }
                }
            }
        }

        private void RebuildAllViewsImpl()
        {
            try
            {
                areViewsBeingRebuiltNow = true;

                throw new NotImplementedException();
            }
            finally
            {
                areViewsBeingRebuiltNow = false;
            }
        }
    }
}