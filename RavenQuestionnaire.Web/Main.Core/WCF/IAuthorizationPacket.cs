namespace Main.Core.WCF
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Main.Core.Entities;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface IAuthorizationPacket
    {
        RegisterData Data { get; set; }
        bool IsAuthorized { get; set; }
    }
}