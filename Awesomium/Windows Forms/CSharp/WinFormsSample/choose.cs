using System;
using System.Collections.Generic;

using System.Windows.Forms;
using System.IO;

using System.Diagnostics;

using WinFormsSample.adept_part;
using WinFormsSample.Properties;
using System.Net;

namespace WinFormsSample
{
    public partial class choose : Form
    {
         List<String> Drives;
        CopyJobSize copyJobSize;

        ListBox drivesLister;

        public choose()
        {
            //copyJobSize = GetJobSize(Configuration.ProgramDirectory);

            InitializeComponent();

            drivesLister = listBox1;

            progressBar1.Visible = false;
            PopulateDrivesUSB();

            
        }




        private void PopulateDrivesUSB()
        {
            CopyBtn.Enabled = false;
            //http://stackoverflow.com/questions/1124463/how-to-get-the-list-of-removable-disk-in-c
            Drives = new List<string>();
            drivesLister.Items.Clear();
            DriveInfo[] ListDrives = DriveInfo.GetDrives();

            foreach (DriveInfo Drive in ListDrives)
            {
                if (Drive.DriveType == DriveType.Removable)
                {
                    Drives.Add(Drive.ToString());
                    drivesLister.Items.Add(DriveLine(Drive));
                }
            }

            if (drivesLister.Items.Count > 0)
            {
                drivesLister.Enabled = true;
                drivesLister.SelectedIndex = 0;
                CopyBtn.Enabled = true;
            }
            else
            {
                drivesLister.Items.Add("No compatible USB devices detected");
                drivesLister.SelectedIndex = 0;
                drivesLister.Enabled = false;
            }
        }
        private string DriveLine(DriveInfo D)
        {
            try
            {
                long freeSpace = D.TotalFreeSpace;
                string freeSpaceStr = UFormat.NiceSizeString(freeSpace);

                string result =
                    D.ToString() + " '"
                    + D.VolumeLabel + "' ["
                    + D.DriveFormat + "] " +
                    freeSpaceStr + " available";

                if (freeSpace < copyJobSize.nBytes)
                {
                    result += " (insufficient space)";
                }

                bool wp = DriveWriteProtected(D.ToString());
                if (wp == true)
                {
                    result += " (write-protected)";
                }

                string adeptFolder = D.Name + "ADEPT";

                if (Directory.Exists(adeptFolder) == true)
                {
                    result += ", contains ADePT";

                    string adeptFile =
                        adeptFolder + Path.DirectorySeparatorChar + "adept.exe";

                    if (File.Exists(adeptFile) == true)
                    {
                        FileVersionInfo adeptVersionInfo;
                        adeptVersionInfo = FileVersionInfo.GetVersionInfo(adeptFile);
                        string adeptVersion = adeptVersionInfo.ProductVersion;
                        result += " v" + adeptVersion;
                    }
                }

                return result;
            }
            catch
            {
                return String.Format("{0} - no information", D.ToString());
            }
        }

        public void AddLine(string line)
        {
            textBox1.Text += (line + "\r\n");
            textBox1.SelectionStart = textBox1.Text.Length;
            textBox1.ScrollToCaret();
            textBox1.Refresh();
        }

        public static void CreatePortable()
        {
            choose F = new choose();
            F.ShowDialog();
        }

        private CopyJobSize GetJobSize(string srcFolder)
        {
            CopyJobSize Result = new CopyJobSize();
            foreach (string oneFile in Directory.GetFiles(
                            srcFolder, "*.*", SearchOption.AllDirectories))
            {
                Result.nFiles++;
                Result.nBytes += (new FileInfo(oneFile)).Length;
            }

            return Result;
        }




        private void CopyBtn_Click(object sender, EventArgs e)
        {
            Cursor currentCursor = Cursor.Current;
            this.Cursor = Cursors.WaitCursor;
            try
            {
                int driveIndex = drivesLister.SelectedIndex;
                string driveLetter = Drives[driveIndex];
                string destination = driveLetter;
                ShootFiles(destination);
                
            }
            catch
            {
                AddLine("Error");
            }
            finally
            {
                AddLine("Export complite");
            }


            this.Cursor = currentCursor;
            //temp;
        }

        

        private void refreshBtn_Click(object sender, EventArgs e)
        {
            PopulateDrivesUSB();
        }

        const int WM_DEVICECHANGE = 0x219;

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_DEVICECHANGE:
                    // The WParam value identifies what is occurring.
                    
                    int n = (int)m.WParam;
                    if (n == 0x8000 || n == 0x8004)
                    {
                        //Thread.Sleep(1000);
                        PopulateDrivesUSB();
                    }
                    
                    //PopulateDrivesUSB();
                    break;
            }
            base.WndProc(ref m);
        }

        private bool DriveWriteProtected(string driveLetter)
        {
            return false;
        }


        private void ShootFiles(string destination)
        {
            

            string exportURL = Settings.Default.DefaultUrl;
            exportURL += "/Synchronizations/Export";
            string filename = string.Format("backup-{0}.zip", DateTime.Now.ToString().Replace("/", "_"));
            filename=filename.Replace(" ", "_");
            filename=filename.Replace(":", "_");
            WebClient myWebClient = new WebClient();
            //byte[] myDataBuffer = myWebClient.DownloadFile(exportURL,de)
            destination += filename;
            myWebClient.DownloadFile(exportURL, destination);

            AddLine("Done! filename: " + filename);
        }

    }


    

    class CopyJobSize
    {
        public int nFiles = 0;
        public long nBytes = 0;
    }
    
}
