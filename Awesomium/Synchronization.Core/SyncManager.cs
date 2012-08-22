using System;
using System.IO;
using System.Net;
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

        private List<ISynchronizer> synchronizerChain;

        private AutoResetEvent syncIsAvailable = new AutoResetEvent(true);

        #endregion

        #region C-tors

        protected SyncManager(ISyncProgressObserver progressStatus, ISettingsProvider settingsProvider)
            : this(progressStatus, settingsProvider, new List<ISynchronizer>())
        {
        }

        private SyncManager(ISyncProgressObserver progressObserver, ISettingsProvider settingsProvider,
                            List<ISynchronizer> subStructure)
        {
            this.synchronizerChain = subStructure;
            SyncProgressChanged += (s, e) => progressObserver.SetProgress(e.Status);
            BgnOfSync += (s, e) => progressObserver.SetBeginning(e.Status);
            EndOfSync += (s, e) => progressObserver.SetCompleted(e.Status);

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

        protected abstract void CheckPushPrerequisites();

        protected abstract void CheckPullPrerequisites();

        protected virtual void CheckPrerequisites(SyncType type)
        {
            if (type == SyncType.Push)
            {
                CheckPushPrerequisites();
                return;
            }
            CheckPullPrerequisites();
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
                catch (CancelledSynchronizationException)
                {
                    throw; // cancel all in the chain at once
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

            SynchronizationException error = null;
            string log = null;

            try
            {
                BgnOfSync(this, new SynchronizationEvent(new SyncStatus(syncType, direction, 0, null)));
                CheckPrerequisites(syncType);
                log = OnDoSynchronizationAction(syncType, direction);
            }
            catch (CancelledSynchronizationException ex)
            {
                error = ex;
                log = error.Message;
            }
            catch (CheckPrerequisitesException ex)
            {
                error = ex;
                log = error.Message;
            }
            catch (Exception ex)
            {
                error = new SynchronizationException("Synchronization process failed", ex);
                log = ex.Message;
            }
            finally
            {
                this.syncIsAvailable.Set();

                EndOfSync(this, new SynchronizationCompletedEvent(new SyncStatus(syncType, direction, 100, error), log));
            }
        }

        #endregion

        #region Implementation of ISyncManager

        public event EventHandler<SynchronizationEvent> SyncProgressChanged;
        public event EventHandler<SynchronizationEvent> BgnOfSync;
        public event EventHandler<SynchronizationCompletedEvent> EndOfSync;

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
            else
                throw new SynchronizationException(result.ToString());

            return result.ToString();
        }

        #endregion
    }
}
