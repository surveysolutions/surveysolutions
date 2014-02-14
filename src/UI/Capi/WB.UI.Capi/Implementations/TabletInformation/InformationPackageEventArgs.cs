using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace WB.UI.Capi.Implementations.TabletInformation
{
    public class InformationPackageEventArgs : EventArgs
    {
        public InformationPackageEventArgs(string filePath, long fileSize)
        {
            this.FilePath = filePath;
            this.FileSize = fileSize;
        }

        public string FilePath { get; private set; }
        public long FileSize { get; private set; }
    }
}