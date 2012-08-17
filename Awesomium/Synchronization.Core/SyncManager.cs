using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Synchronization.Core.Interface;
using Synchronization.Core.Events;
using Synchronization.Core.Errors;
using Synchronization.Core.SynchronizationFlow;

namespace Synchronization.Core
{
    public abstract class SyncManager : ISyncManager
    {
        #region Members

        private ISyncProgressObserver progressStatus;
        private List<ISynchronizer> synchronizerChain;
        private AutoResetEvent syncIsAvailable = new AutoResetEvent(true);

        #endregion

        #region C-tors

        protected SyncManager(ISyncProgressObserver progressStatus, ISettingsProvider settingsProvider)
            : this(progressStatus, settingsProvider, new List<ISynchronizer>())
        {
        }

        private SyncManager(ISyncProgressObserver progressStatus, ISettingsProvider settingsProvider, List<ISynchronizer> subStructure)
        {
            this.synchronizerChain = subStructure;
            this.progressStatus = progressStatus;

            SyncProgressChanged += (s, e) => this.progressStatus.SetProgress(e.Status);

            AddSynchronizers(settingsProvider);
        }

        #endregion

        #region Helpers

        private void AddSynchronizers(ISettingsProvider settingsProvider)
        {
            try
            {
                OnAddSynchronizers(this.synchronizerChain, settingsProvider);

                Debug.Assert(this.synchronizerChain.Count > 0, "Have you missed adding synchronizers?");

                foreach (var synchronizer in this.synchronizerChain)
                    synchronizer.SyncProgressChanged += this.SyncProgressChanged;
            }
            catch
            {
            }
        }

        private ISynchronizer ExecuteAction(Action<ISynchronizer> action, IList<Exception> errorList)
        {
            foreach (var synchronizer in synchronizerChain)
            {
                try
                {
                    action(synchronizer);
                    return synchronizer;
                }
                catch (SynchronizationException e)
                {
                    errorList.Add(e);
                }
            }

            return null;
        }

        private void DoSynchronizationAction(SyncType syncType, SyncDirection direction)
        {
            if (!this.syncIsAvailable.WaitOne(0))
                return;

            string error = null;

            try
            {
                this.progressStatus.SetBeginning();

                error = OnDoSynchronizationAction(syncType, direction);
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            finally
            {
                this.syncIsAvailable.Set();

                this.progressStatus.SetCompleted(new SyncStatus(100, !string.IsNullOrEmpty(error), false));
            }
        }

        #endregion

        #region Implementation of ISyncManager

        public event EventHandler<SynchronizationEvent> SyncProgressChanged;

        public void Push(SyncDirection direction)
        {
            DoSynchronizationAction(SyncType.Push, direction);
        }

        public void Pull(SyncDirection direction)
        {
            DoSynchronizationAction(SyncType.Pull, direction);
        }

        public void Stop()
        {
            if (!this.syncIsAvailable.WaitOne(0))
                return;

            foreach (var synchronizer in synchronizerChain)
                synchronizer.Stop();
        }

        #endregion

        #region Abstract and Virtual

        protected abstract void OnAddSynchronizers(IList<ISynchronizer> syncChain, ISettingsProvider settingsProvider);

        protected virtual string OnDoSynchronizationAction(SyncType action, SyncDirection direction)
        {
            IList<Exception> errorList = new List<Exception>();

            var succesSynchronizer = ExecuteAction(
                    s =>
                    {
                        if (action == SyncType.Pull)
                            s.Pull(direction);
                        else
                            s.Push(direction);
                    },
                    errorList
                );

            StringBuilder result = new StringBuilder();
            foreach (SynchronizationException synchronizationException in errorList)
                result.AppendLine(synchronizationException.Message);

            if (succesSynchronizer != null)
                result.AppendLine(succesSynchronizer.BuildSuccessMessage(action, direction));

            return result.ToString();
        }

        #endregion
    }
}
