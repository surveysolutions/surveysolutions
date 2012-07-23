using System;


using System.Windows.Forms;


namespace WinFormsSample
{
    public partial class PleaseWaitForm : Form
    {
        public PleaseWaitForm()
        {
            //copyJobSize = GetJobSize(Configuration.ProgramDirectory);

            InitializeComponent();



        }




        //private void PopulateDrivesUSB()
        //{
        //    CopyBtn.Enabled = false;
        //    //http://stackoverflow.com/questions/1124463/how-to-get-the-list-of-removable-disk-in-c
        //    Drives = new List<string>();
        //    drivesLister.Items.Clear();
        //    DriveInfo[] ListDrives = DriveInfo.GetDrives();

        //    foreach (DriveInfo Drive in ListDrives)
        //    {
        //        if (Drive.DriveType == DriveType.Removable)
        //        {
        //            Drives.Add(Drive.ToString());
        //        }
        //    }

        //    if (drivesLister.Items.Count > 0)
        //    {
        //        drivesLister.Enabled = true;
        //        drivesLister.SelectedIndex = 0;
        //        CopyBtn.Enabled = true;
        //    }
        //    else
        //    {
        //        drivesLister.Items.Add("No compatible USB devices detected");
        //        drivesLister.SelectedIndex = 0;
        //        drivesLister.Enabled = false;
        //    }
        //}



    }




    class CopyJobSize
    {
        public int nFiles = 0;
        public long nBytes = 0;
    }

}
