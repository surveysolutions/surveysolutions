// -----------------------------------------------------------------------
// <copyright file="IRegisterData.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Synchronization.Core.Interface
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface IRegisterData
    {
        string Description { get; }
        DateTime RegisterDate { get; }
        Guid RegistrationId { get; }
        Guid Registrator { get; }
        byte[] SecretKey { get; }
    }
}
