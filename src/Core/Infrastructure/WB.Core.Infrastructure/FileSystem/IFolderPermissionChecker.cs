using System;
using System.Collections.Generic;

namespace WB.Core.Infrastructure.FileSystem
{
    public interface IFolderPermissionChecker
    {
        FolderPermissionCheckResult Check();
    }

    public class FolderPermissionCheckResult
    {
        public string[] AllowedFolders { get; set; }
        public string[] DenidedFolders { get; set; }
        public string ProcessRunedUnder { get; set; }
    }
}