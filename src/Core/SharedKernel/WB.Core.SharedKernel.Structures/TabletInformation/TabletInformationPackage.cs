using System;

namespace WB.Core.SharedKernel.Structures.TabletInformation
{
    public class TabletInformationPackage
    {
        public string Content { get; set; }
        public string AndroidId { get; set; }
        public Guid ClientRegistrationId { get; set; }
    }
}