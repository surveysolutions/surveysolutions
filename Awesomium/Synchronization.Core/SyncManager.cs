using NLog;
using System;
using System.Linq;
using System.Text;
using Common.Utils;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using Synchronization.Core.Events;
using Synchronization.Core.Errors;
using Synchronization.Core.Interface;
using Synchronization.Core.SynchronizationFlow;


namespace Synchronization.Core
{
    public abstract class SyncManager : ISyncManager
    {
        #region Members

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private List<ISynchronizer> synchronizerChain;
        private ISettingsProvider settingsProvider;
        private AutoResetEvent syncIsAvailable = new AutoResetEvent(true);

        #endregion

        #region C-tors

        protected SyncManager(ISyncProgressObserver progressStatus, ISettingsProvider settingsProvider, IRequesProcessor requestProcessor, IUrlUtils urlUtils, IUsbProvider usbProvider)
            : this(progressStatus, settingsProvider, requestProcessor, urlUtils, usbProvider, new List<ISynchronizer>())
        {
            //LogManager.EnableLogging();
        }

        private SyncManager(ISyncProgressObserver progressObserver, ISettingsProvider settingsProvider, IRequesProcessor requestProcessor, IUrlUtils urlUtils, IUsbProvider usbProvider,
                            List<ISynchronizer> subStructure)
        {

            this.synchronizerChain = subStructure;
            this.RequestProcessor = requestProcessor;
            this.UrlUtils = urlUtils;
            this.UsbProvider = usbProvider;
            this.settingsProvider = settingsProvider;

            SyncProgressChanged += (s, e) => progressObserver.SetProgress(e.Status);
            BgnOfSync += (s, e) => progressObserver.SetBeginning(e.Status);
            EndOfSync += (s, e) => progressObserver.SetCompleted(e.Status);
            StatisticAccepted += (s, e) => progressObserver.SetStatistics(e.Info);

            AddSynchronizers();
        }

        protected IRequesProcessor RequestProcessor { get; private set; }
        protected IUrlUtils UrlUtils { get; private set; }
        protected IUsbProvider UsbProvider { get; private set; }

        #endregion

        protected abstract void CheckPushPrerequisites(SyncDirection direction);
        protected abstract void CheckPullPrerequisites(SyncDirection direction);

        #region Helpers

        private void AddSynchronizers()
        {
            try
            {
                OnAddSynchronizers(this.synchronizerChain, this.settingsProvider);

                Debug.Assert(this.synchronizerChain.Count > 0, "Have you missed adding synchronizers?");

                foreach (var synchronizer in this.synchronizerChain)
                    synchronizer.SyncProgressChanged += this.SyncProgressChanged;

                UpdateStatuses();
            }
            catch
            {
            }
        }

        private void CheckPrerequisites(SyncType typeSync, SyncDirection direction)
        {
            if (typeSync == SyncType.Push)
            {
                CheckPushPrerequisites(direction);
                return;
            }

            CheckPullPrerequisites(direction);
        }

        private ISynchronizer ExecuteAction(Action<ISynchronizer> action, IList<Exception> errorList)
        {
            foreach (var synchronizer in this.synchronizerChain)
            {
                if (!synchronizer.IsActive)
                    continue;

                try
                {
                    action(synchronizer);

                    return synchronizer;
                }
                catch (CancelledServiceException)
                {
                    throw; // cancel all in the chain at once
                }
                catch (Exception e)
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

            ServiceException error = null;
            string log = null;

            try
            {
                BgnOfSync(this, new SynchronizationEventArgs(new SyncStatus(syncType, direction, 0, null)));
                CheckPrerequisites(syncType, direction);
                log = Sync(syncType, direction);
            }
            catch (CancelledServiceException ex)
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

                Logger.Info(log);

                EndOfSync(this, new SynchronizationCompletedEventArgs(new SyncStatus(syncType, direction, 100, error), log));
                if (error == null)
                {
                    var statEvent = OnGetStatisticsAfterSyncronization(syncType);
                    if (statEvent != null)
                        StatisticAccepted(this, statEvent);
                }
            }
        }

        private string Sync(SyncType action, SyncDirection direction)
        {
            IList<Exception> errorList = new List<Exception>();
            var succesSynchronizer = ExecuteAction(
                    s =>
                    {
                        if (action == SyncType.Pull)
                            s.Pull(direction);
                        else
                        {
                            s.Push(direction);
                        }
                    },
                    errorList
                );

            var result = new StringBuilder();
            foreach (Exception synchronizationException in errorList)
                result.AppendLine(synchronizationException.Message);
            if (succesSynchronizer != null)
                result.AppendLine(succesSynchronizer.GetSuccessMessage(action, direction));
            else
                throw new SynchronizationException(result.ToString());

            return result.ToString();
        }


        #endregion

        #region Implementation of ISyncManager

        public event EventHandler<SynchronizationEventArgs> SyncProgressChanged;
        public event EventHandler<SynchronizationEventArgs> BgnOfSync;
        public event EventHandler<SynchronizationCompletedEventArgs> EndOfSync;
        public event EventHandler<SynchronizationStatisticEventArgs> StatisticAccepted;

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
            foreach (var synchronizer in this.synchronizerChain)
                synchronizer.Stop();
        }

        public void UpdateStatuses()
        {
            foreach (var synchronizer in this.synchronizerChain)
                synchronizer.UpdateStatus();
        }

        public IList<ServiceException> CheckSyncIssues(SyncType syncType, SyncDirection direction)
        {
            if (this.RequestProcessor.Process<string>(this.UrlUtils.GetDefaultUrl(), "False") == "False")
                return new List<ServiceException>() { new LocalHosUnreachableException() }; // there is no connection to local host


            IList<ServiceException> errors = new List<ServiceException>();

            try
            {
                CheckPrerequisites(syncType, direction);
            }
            catch (CheckPrerequisitesException e)
            {
                errors.Add(e);
            }

            foreach (var synchronizer in this.synchronizerChain)
            {
                IList<ServiceException> sErrors = synchronizer.CheckSyncIssues(syncType, direction);
                if (sErrors == null || sErrors.Count == 0)
                    continue;

                errors = errors.Union(sErrors).ToList();
            }

            return errors;
        }

        #endregion

        #region Abstract and Virtual

        protected abstract void OnAddSynchronizers(IList<ISynchronizer> syncChain, ISettingsProvider settingsProvider);

        protected abstract SynchronizationStatisticEventArgs OnGetStatisticsAfterSyncronization(SyncType action);

        #endregion
    }
}
