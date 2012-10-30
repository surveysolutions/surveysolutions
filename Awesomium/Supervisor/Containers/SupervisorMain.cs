using System;
using System.Linq;
using System.Windows.Forms;
using Browsing.Common.Containers;
using Browsing.Common.Controls;
using Browsing.Supervisor.Registration;
using Common.Utils;
using Synchronization.Core.Interface;
using Synchronization.Core.Registration;

namespace Browsing.Supervisor.Containers
{
    public partial class SupervisorMain : Main
    {
        private FlatButton btnRegistration;
        private bool registrationFirstStep = true;
        private SupervisorRegistrationManager supervisorRegistrationManager;
        
        #region Constructor

        public SupervisorMain(ISettingsProvider clientSettings, IRequesProcessor requestProcessor, IRSACryptoService rsaCryptoService, IUrlUtils urlUtils, ScreenHolder holder)
            : base(clientSettings, requestProcessor,rsaCryptoService, urlUtils, holder)
        {
            InitializeComponent();
            supervisorRegistrationManager = new SupervisorRegistrationManager();
        }

        #endregion

        protected override void AddRegistrationButton(TableLayoutPanel tableLayoutPanel)
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));

            this.btnRegistration = new Browsing.Common.Controls.FlatButton();
            this.btnRegistration.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnRegistration.FlatAppearance.BorderSize = 0;
            this.btnRegistration.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRegistration.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

            this.btnRegistration.Image = ((System.Drawing.Image)(resources.GetObject("btnDashboard.Image")));
            this.btnRegistration.Location = new System.Drawing.Point(390, 43);
            this.btnRegistration.Margin = new System.Windows.Forms.Padding(0);
            this.btnRegistration.Name = "btnRegistration";
            this.btnRegistration.Padding = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.btnRegistration.Size = new System.Drawing.Size(215, 220);
            this.btnRegistration.TabIndex = 2;
            this.btnRegistration.Text = "Registration";
            this.btnRegistration.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnRegistration.UseVisualStyleBackColor = true;
            this.btnRegistration.Click += new System.EventHandler(this.btnRegistration_Click);
            tableLayoutPanel.Controls.Add(this.btnRegistration, 5, 1);
        }
        private void btnRegistration_Click(object sender, EventArgs e)
        {
            if (this.registrationFirstStep)
            {
                btnRegistration.Text = "Finish Registration";
                var user = this.OnGetCurrentUser();
                supervisorRegistrationManager.RegistrationFirstStep(rsaCryptoService,user);
                registrationFirstStep = false;
            }
            else
            {
                supervisorRegistrationManager.RegistrationSecondStep();
            }
        }
        protected override void OnSynchronizationClicked(object sender, System.EventArgs e)
        {
            this.Holder.Redirect(this.Holder.LoadedScreens.FirstOrDefault(s => s is SyncChoicePage));
        }
    }
}
