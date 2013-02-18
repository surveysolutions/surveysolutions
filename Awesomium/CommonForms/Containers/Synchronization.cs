using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Browsing.Common.Controls;
using Browsing.Common.Interfaces;
using Common.Utils;
using Synchronization.Core.Errors;
using Synchronization.Core.Events;
using Synchronization.Core.Interface;
using Synchronization.Core.SynchronizationFlow;

namespace Browsing.Common.Containers
{
    public abstract partial class Synchronization : Screen, IUsbWatcher
    {
        IUrlUtils utils;

        private bool isPushPossible = false;
        private bool isPullPossible = false;

        #region C-tor

        public Synchronization(ISettingsProvider clientSettings, IRequesProcessor requestProcessor, IUrlUtils utils, ScreenHolder holder)
            : base(holder, true)
        {
            InitializeComponent();

            this.utils = utils;

            this.syncPanel.Parent = ContentPanel;

            this.syncPanel.PullPressed += btnPull_Click;
            this.syncPanel.PushPressed += btnPush_Click;
            this.syncPanel.CancelPressed += btnCancel_Click;
            this.syncPanel.usbStatusPanel.UsbPressed += usb_Click;

            this.SyncManager = DoInstantiateSyncManager(this.syncPanel, clientSettings, requestProcessor, utils, this.syncPanel);

            System.Diagnostics.Debug.Assert(this.SyncManager != null);

            this.SyncManager.EndOfSync += new EventHandler<SynchronizationCompletedEvent>(sync_EndOfSync);
            this.SyncManager.BgnOfSync += new EventHandler<SynchronizationEvent>(sync_BgnOfSync);
        }


        #endregion

        #region Virtual and Abstract

        protected abstract ISyncManager DoInstantiateSyncManager(ISyncProgressObserver progressObserver, ISettingsProvider clientSettings, IRequesProcessor requestProcessor, IUrlUtils utils, IUsbProvider usbProvider);
        protected abstract void OnPushClicked();
        protected abstract void OnPullClicked();

        protected virtual void OnEnablePush(bool enable)
        {
            this.syncPanel.EnablePush(this.isPushPossible && enable);
        }

        protected virtual void OnEnablePull(bool enable)
        {
            this.syncPanel.EnablePull(this.isPullPossible && enable);
        }

        #endregion

        #region Helpers

        private void CheckSyncPossibilities(bool background = true)
        {
            if (background)
                new System.Threading.Thread(CheckSync).Start();
            else
                CheckSync();
        }

        private void CheckSync()
        {
            string status = string.Empty;

            try
            {
                this.syncPanel.ShowError("Looking for available data points ...");

                // assume sync possibility by default
                this.isPullPossible = true;
                this.isPushPossible = true;

                //return;

                IList<SynchronizationException> issues = this.SyncManager.CheckSyncIssues(SyncType.Push, SyncDirection.Up);
                if (issues == null || issues.Count == 0)
                    return;

                SynchronizationException ex = issues.FirstOrDefault<SynchronizationException>(x => x is LocalHosUnreachableException);
                if (ex != null)
                {
                    this.isPullPossible = false;
                    this.isPushPossible = false;

                    status = ex.Message;

                    return; // fatal
                }

                ex = issues.FirstOrDefault<SynchronizationException>(x => x is NetUnreachableException || x is InactiveNetSynchronizerException);
                if (ex != null)
                {
                    status = ex.Message;

                    ex = issues.FirstOrDefault<SynchronizationException>(x => x is UsbUnacceptableException);
                    if (ex != null)
                    {
                        this.isPullPossible = false;
                        this.isPushPossible = false;

                        status += "\n" + ex.Message;

                        return; // fatal
                    }
                }

                ex = issues.FirstOrDefault<SynchronizationException>(x => x is CheckPrerequisitesException);
                if (ex != null)
                {
                    this.isPullPossible = true;
                    this.isPushPossible = false;

                    status += "\n" + ex.Message;
                }
            }
            finally
            {
                this.syncPanel.ShowError(status);

                EnablePull(this.isPullPossible);
                EnablePush(this.isPushPossible);
            }
        }

        #endregion

        #region Event Handlers

        private void sync_BgnOfSync(object sender, SynchronizationEvent e)
        {
            if (this.InvokeRequired)
                this.Invoke(new MethodInvoker(() =>
                {
                    this.syncPanel.State =
                        e.Status.ActionType == SyncType.Push ? SyncState.Push : SyncState.Pull;

                    EnablePush(false);
                    EnablePull(false);
                }));
        }

        private void sync_EndOfSync(object sender, SynchronizationCompletedEvent e)
        {
            if (this.InvokeRequired)
                this.Invoke(new MethodInvoker(() =>
                {
                    this.syncPanel.ShowResult(e.Log);
                    this.syncPanel.State = SyncState.Idle;

                    EnablePush(true);
                    EnablePull(true);
                }));
        }

        private void btnPush_Click(object sender, EventArgs e)
        {
            try
            {
                OnPushClicked();
            }
            catch
            {
            }
        }

        private void btnPull_Click(object sender, EventArgs e)
        {
            try
            {
                OnPullClicked();
            }
            catch
            {
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                this.SyncManager.Stop();
            }
            catch
            {
            }
        }

        private void usb_Click(object sender, EventArgs e)
        {
            try
            {
                CheckSyncPossibilities();
            }
            catch
            {
            }
        }

        #endregion

        #region Fields

        protected internal ISyncManager SyncManager { get; private set; }

        #endregion

        #region Overloading

        protected override void OnUpdateConfigDependencies()
        {
            base.OnUpdateConfigDependencies();

            this.SyncManager.UpdateStatuses();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.syncPanel.SetIdleState();
            
            EnablePush(false);
            EnablePull(false);
            
            this.syncPanel.ShowError("Looking for available data points ...");

        }

        protected override void OnValidateContent()
        {
            base.OnValidateContent();

            CheckSyncPossibilities();
        }

        #endregion

        #region Operations

        public void UpdateUsbList()
        {
            this.syncPanel.UpdateUsbStatus();

            CheckSyncPossibilities();
        }

        public void EnablePush(bool enable)
        {
            OnEnablePush(enable);
        }

        public void EnablePull(bool enable)
        {
            OnEnablePull(enable);
        }

        #endregion

    }
}
