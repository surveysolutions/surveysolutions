using System;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AppSettingAttribute : Attribute
    {
        public AppSettingAttribute(bool fallbackReadFromPrimaryWorkspace = false)
        {
            FallbackReadFromPrimaryWorkspace = fallbackReadFromPrimaryWorkspace;
        }

        public bool FallbackReadFromPrimaryWorkspace { get; set; }
    }
}
