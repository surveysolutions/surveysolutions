#region Using
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
#if USING_MONO
using Awesomium.Mono;
using Awesomium.Mono.Forms;
#else
using Awesomium.Core;
using Awesomium.Windows.Forms;
using Browsing.CAPI;
using Browsing.CAPI.Properties;
using Browsing.CAPI.ClientSettings;
using Synchronization.Core.Events;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using Synchronization.Core.Interface;
using Synchronization.Core.SynchronizationFlow;
using Browsing.CAPI.Synchronization;
using Common;

#endif
#endregion

namespace Browsing.CAPI.Forms
{
    public partial class WebForm : Form
    {
        private Containers.CAPIBrowser capiBrowser;
        private Containers.CAPISynchronization capiSycn;
        private Containers.CAPIMain capiMain;
        private Awesomium.Windows.Forms.WebControl webView;
        private ISettingsProvider clientSettings;
        #region C-tor

        public WebForm()
        {
            // Notice that Control.DoubleBuffered has been set to true
            // in the designer, to prevent flickering.
            InitializeComponent();

            WebCore.Initialize(new WebCoreConfig()
                                   {
                                       EnablePlugins = true,
                                       SaveCacheAndCookies = true
                                   }, true);

            this.webView=new WebControl();
            this.clientSettings=new ClientSettingsProvider();
            AddMain();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
          {
              if (this.webView != null)
              {
                  //this.webView.IsDirtyChanged -= OnIsDirtyChanged;
                  //this.webView.SelectLocalFiles -= OnSelectLocalFiles;                this.webView.Close();
              }

              base.OnFormClosed(e);

#if USING_MONO
    // TODO: Mac OS X: Sends a SIGSEGV to Mono.
            if ( !PlatformDetection.IsMac )
#endif
              WebCore.Shutdown();
          }

        protected void AddBrowser()
      {
          this.capiBrowser = new Browsing.CAPI.Containers.CAPIBrowser(this.webView);
          // 
          // capiBrowser1
          // 
          this.capiBrowser.AutoSize = true;
          this.capiBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
          this.capiBrowser.Location = new System.Drawing.Point(0, 0);
          this.capiBrowser.Name = "capiBrowser1";
          this.capiBrowser.HomeButtonClick += new EventHandler<EventArgs>(capiBrowser_HomeButtonClick);
       //   this.capiBrowser.Size = new System.Drawing.Size(1596, 808);
       //   this.capiBrowser.TabIndex = 2;
          this.Controls.Add(this.capiBrowser);
      }
      protected void AddSynchronizer()
      {
          this.capiSycn = new Browsing.CAPI.Containers.CAPISynchronization(this.clientSettings);
          // 
          // capiBrowser1
          // 
          this.capiSycn.AutoSize = true;
          this.capiSycn.Name = "capiSync";
          this.capiSycn.Size = new System.Drawing.Size(this.ClientSize.Width, this.ClientSize.Height);
          this.capiSycn.Left = 0;
          this.capiSycn.Top = 0;
          this.capiSycn.BackClick += new EventHandler<EventArgs>(capiSycn_BackClick);
          this.Controls.Add(this.capiSycn);
      }

      void capiSycn_BackClick(object sender, EventArgs e)
      {
          this.Controls.Clear();
          AddMain();
      }
      void capiBrowser_HomeButtonClick(object sender, EventArgs e)
      {
          this.Controls.Clear();
          AddMain();
      }
      protected void AddMain()
      {
          this.capiMain = new Browsing.CAPI.Containers.CAPIMain(this.clientSettings);
          // 
          // capiBrowser1
          // 
        //  this.capiMain.AutoSize = true;
      //    this.capiMain.Dock = System.Windows.Forms.DockStyle.Left;
      //    this.capiMain.Location = new System.Drawing.Point(0, 0);
          //this.capiMain.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right  | System.Windows.Forms.AnchorStyles.Top;
         
          this.capiMain.Name = "capiMain";
          this.capiMain.Size = new System.Drawing.Size(920, 200);
          this.capiMain.Left = (this.ClientSize.Width - this.capiMain.Width) / 2;
          this.capiMain.Top = (this.ClientSize.Height - this.capiMain.Height) / 2;
        //  this.capiMain.TabIndex = 2;
          this.Controls.Add(this.capiMain);
          this.capiMain.DashboardClick += new EventHandler<EventArgs>(capiMain_DashboardClick);
          this.capiMain.SynchronizationClick += new EventHandler<EventArgs>(capiMain_SynchronizationClick);
          this.capiMain.LoginClick += new EventHandler<EventArgs>(capiMain_LoginClick);
       //   this.ClientSizeChanged += new EventHandler(capiMain_ClientSizeChanged);
      }

      void capiMain_LoginClick(object sender, EventArgs e)
      {
          this.Controls.Clear();
          AddBrowser();
      }

      void capiMain_SynchronizationClick(object sender, EventArgs e)
      {
          this.Controls.Clear();
          AddSynchronizer();
      }

    /*  void capiMain_ClientSizeChanged(object sender, EventArgs e)
      {
          this.capiMain.Left = (this.ClientSize.Width - this.capiMain.Width) / 2;
          this.capiMain.Top = (this.ClientSize.Height - this.capiMain.Height) / 2;
      }*/

      void capiMain_DashboardClick(object sender, EventArgs e)
      {
          this.Controls.Clear();
          AddBrowser();
      }
       

        #endregion

    }
}