// -----------------------------------------------------------------------
// <copyright file="AuthorizationPacket.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Main.Core.WCF
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class AuthorizationPacket : IAuthorizationPacket
    {
        public Entities.RegisterData Data { get; set; }

        public bool IsAuthorized { get; set; }
    }
}
