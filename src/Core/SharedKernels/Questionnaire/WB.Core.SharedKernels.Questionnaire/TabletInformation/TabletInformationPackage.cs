using System;

namespace WB.Core.SharedKernel.Structures.TabletInformation
{
    [Obsolete("Since v 5.9.0")]
    public class TabletInformationPackage
    {
        public string Content { get; set; }
        public string AndroidId { get; set; }
    }
}