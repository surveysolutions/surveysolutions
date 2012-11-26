// -----------------------------------------------------------------------
// <copyright file="ISupervisorService.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Synchronization.Core.Interface
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ServiceModel;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [ServiceContract]
    public interface ISupervisorService
    {
        [OperationContract]
        string GetSupervisorPath();
    }
}
