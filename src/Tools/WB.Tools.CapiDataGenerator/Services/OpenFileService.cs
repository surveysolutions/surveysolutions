using Microsoft.Win32;

namespace CapiDataGenerator
{
    public class OpenFileService : IOpenFileService
    {
        public string OpenFileDialog()
        {
            // Configure open file dialog box
            var dlg = new OpenFileDialog {Filter = "Template file|*.*"};

            // Show open file dialog box
            bool? result = dlg.ShowDialog();

            return result == true ? dlg.FileName : string.Empty;
        }
    }
}
