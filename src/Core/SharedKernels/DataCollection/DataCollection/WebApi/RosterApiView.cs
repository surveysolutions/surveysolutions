using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.WebApi
{
    public class RosterApiView
    {
        public IdentityApiView Identity { get; set; }
        public List<RosterInstanceApiView> Instances { get; set; } 
    }
}