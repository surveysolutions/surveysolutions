namespace CapiDataGenerator
{
    public class OpenFileService : IOpenFileService
    {
        public string OpenFileDialog()
        {
            // Show dialog and take result into account
            bool? result = AppSettings.Instance.OpenDialog.Show();
            return result == true ? AppSettings.Instance.OpenDialog.SelectedFilePath : string.Empty;
        }
    }
}
