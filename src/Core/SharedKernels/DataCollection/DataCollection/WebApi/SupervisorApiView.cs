using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.WebApi
{
    public class SupervisorApiView
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        
        public List<WorkspaceApiView> Workspaces { get; set; }
    }
}
