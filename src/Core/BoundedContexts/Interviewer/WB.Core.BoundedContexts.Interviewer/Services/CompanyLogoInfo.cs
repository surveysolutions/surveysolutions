namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public class CompanyLogoInfo
    {
        public byte[] Logo { get; set; }

        public bool HasCustomLogo { get; set; }

        public bool LogoNeedsToBeUpdated { get; set; }

        public string Etag { get; set; }
    }
}