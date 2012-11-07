using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Browsing.Common.Controls;

namespace Browsing.Common.Containers
{
    public partial class UsbStatusPanel : UserControl
    {
        private ManualResetEvent inactiveStatus = new ManualResetEvent(true);
        private DriveInfo chosenUSB = null;

        public EventHandler UsbPressed;

        #region C-tor

        public UsbStatusPanel()
        {
            InitializeComponent();

            InitLook();
        }

        #endregion

        #region Helpers

        private void ChangeAvailableUsb(string status, bool error = false)
        {
            SetLabel(this.labelAvlUsb, status, error);
        }

        private void AddStripItem(ToolStripItem item)
        {
            this.usbStrip.Items.Add(item);
        }

        private void ClearStrip()
        {
            this.usbStrip.Items.Clear();
        }

        /// <summary>
        /// Assign text content to status label
        /// </summary>
        /// <param name="status"></param>
        private void SetLabel(Label label, string status, bool error = false)
        {
            if (label.InvokeRequired)
            {
                label.Invoke(new MethodInvoker(() =>
                {
                    label.Visible = !string.IsNullOrEmpty(status);
                    label.Text = status;
                    label.ForeColor = error ? System.Drawing.Color.Red : System.Drawing.Color.Black;
                }));
            }
            else
            {
                label.Visible = !string.IsNullOrEmpty(status);
                label.Text = status;
                label.ForeColor = error ? System.Drawing.Color.Red : System.Drawing.Color.Black;
            }
        }

        private void ChangeStripVisible(bool value)
        {
            this.usbStrip.Visible = value;
        }

        #endregion

        #region Properties

        public DriveInfo ChosenUsb
        {
            get { return this.chosenUSB; }
        }

        public ManualResetEvent InactiveStatus
        {
            get { return this.inactiveStatus; }
        }

        #endregion

        #region Methods

        public void SetStatus(string status, bool error = false)
        {
            SetLabel(this.statusLabel, status, error);
        }

        public void SetResult(string result, bool error = false)
        {
            SetLabel(this.resultLabel, result, error);
        }

        /// <summary>
        /// Helper method to dissapear in diffrent thread if wait operation expected before hiding the form
        /// </summary>
        /// <param name="immediately"></param>
        public void Deactivate(bool immediately)
        {
            if (immediately)
                MakeInvisibleStatus(0);
            else
                new Thread(MakeInvisibleStatus).Start(10000);
        }

        public List<DriveInfo> ReviewDriversList()
        {
            List<DriveInfo> drivers = new List<DriveInfo>();
            DriveInfo[] listDrives = DriveInfo.GetDrives();

            foreach (var drive in listDrives)
            {
                if (drive.DriveType == DriveType.Removable)
                    drivers.Add(drive);
            }

            return drivers;
        }

        #endregion

        private void UsbChoosen(object sender, EventArgs args)
        {
            this.chosenUSB = null;

            var stripItems = GetStripItems();
            foreach (var usbItem in stripItems)
            {
                var control = (usbItem as ToolStripControlHost).Control;
                var button = control as FlatButton;
                if (button == null)
                    continue;

                if (button == sender)
                {
                    this.chosenUSB = button.Tag as DriveInfo;
                    button.Image = GetImages(1);
                }
                else
                    button.Image = GetImages(0);
            }

            if (UsbPressed != null)
                UsbPressed(sender, args);
        }

        private void MakeInvisibleStatus(object waitTime)
        {
            Thread.Sleep((int)waitTime);

            SetResult(null);
            SetStatus(null);

            this.inactiveStatus.Set();
        }
        
        private void InitLook()
        {
            MakeInvisibleStatus(0);
            ChangeAvailableUsb(null);
            
            UpdateUsbList();
        }
        
        public void UpdateUsbList()
        {
            ClearStrip();

            this.chosenUSB = null;

            var drives = ReviewDriversList();

            foreach (var drive in drives)
            {
                int imageIndex = 0;
                bool theDriverIsChoozen = this.chosenUSB != null && string.Compare(drive.Name, this.chosenUSB.Name, true) == 0;
                if (theDriverIsChoozen)
                {
                    this.chosenUSB = drive;
                    imageIndex = 1;
                }

                FlatButton item = new FlatButton()
                {
                    Text = drive.Name.Trim(new char[] { '/', '\\' }),
                    Tag = drive,

                    Image = GetImages(imageIndex),
                    Font = new Font(this.tableLayoutPanel2.Font.FontFamily, this.tableLayoutPanel2.Font.Size * 0.8f, FontStyle.Italic | FontStyle.Bold | FontStyle.Italic),
                    ImageAlign = ContentAlignment.TopCenter,
                    TextAlign = ContentAlignment.BottomCenter,
                };

                item.Click += UsbChoosen;

                AddStripItem(new ToolStripControlHost(item));
            }

            if (GetStripItems().Count == 0)
            {
                ChangeStripVisible(false);
                ChangeAvailableUsb(null);
            }
            else
            {
                ChangeStripVisible(true);
                ChangeAvailableUsb("Available USB drivers:");
            }
        }

        public ToolStripItemCollection GetStripItems()
        {
            return this.usbStrip.Items;
        }

        public Image GetImages(int index)
        {
            return imageList1.Images[index];
        }
    }
}
