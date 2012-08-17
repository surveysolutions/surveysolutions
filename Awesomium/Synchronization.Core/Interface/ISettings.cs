using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Synchronization.Core.Interface
{
    public interface ISettings
    {
        Guid ClientId { get; set; }
        Guid ParentId { get; set; }
    }
}
