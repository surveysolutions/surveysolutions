namespace WB.Core.SharedKernel.Structures.TabletInformation
{
    public class TabletInformationPackage
    {
        public TabletInformationPackage() {}

        public TabletInformationPackage(string packageName, byte[] content, string androidId, string registrationId)
        {
            this.PackageName = packageName;
            this.Content = content;
            this.AndroidId = androidId;
            this.RegistrationId = registrationId;
        }

        public string PackageName { get; set; }
        public byte[] Content { get; set; }
        public string AndroidId { get; set; }
        public string RegistrationId { get; set; }
    }
}