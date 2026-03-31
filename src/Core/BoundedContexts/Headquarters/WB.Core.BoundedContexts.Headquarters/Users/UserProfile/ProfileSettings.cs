using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserProfile
{
    [AppSetting(FallbackReadFromPrimaryWorkspace = true)]
    public class ProfileSettings
    {
        public bool AllowInterviewerUpdateProfile { get; set; }
    }
}
