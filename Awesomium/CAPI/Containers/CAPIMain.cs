using System;
using System.Linq;
using System.Windows.Forms;
using Awesomium.Core;
using Common.Utils;
using Synchronization.Core.Interface;
using Browsing.Common.Containers;
using Browsing.Common.Controls;

namespace Browsing.CAPI.Containers
{
    public partial class CAPIMain : Main
    {
        public CAPIMain(ISettingsProvider clientSettings, IRequesProcessor requestProcessor, IUrlUtils urlUtils, ScreenHolder holder)
            : base(clientSettings, requestProcessor, urlUtils, holder)
        {
            InitializeComponent();
        }

        protected override void AddRegistrationButton(TableLayoutPanel tableLayoutPanel)
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));

            FlatButton btnRegistration = new Browsing.Common.Controls.FlatButton();
            btnRegistration.Dock = System.Windows.Forms.DockStyle.Fill;
            btnRegistration.FlatAppearance.BorderSize = 0;
            btnRegistration.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnRegistration.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

            btnRegistration.Image = ((System.Drawing.Image)(resources.GetObject("btnDashboard.Image")));
            btnRegistration.Location = new System.Drawing.Point(390, 43);
            btnRegistration.Margin = new System.Windows.Forms.Padding(0);
            btnRegistration.Name = "btnRegistration";
            btnRegistration.Padding = new System.Windows.Forms.Padding(0, 0, 0, 5);
            btnRegistration.Size = new System.Drawing.Size(215, 220);
            btnRegistration.TabIndex = 2;
            btnRegistration.Text = "Registration";
            btnRegistration.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            btnRegistration.UseVisualStyleBackColor = true;

            tableLayoutPanel.Controls.Add(btnRegistration, 5, 1);
        }
    }
}
