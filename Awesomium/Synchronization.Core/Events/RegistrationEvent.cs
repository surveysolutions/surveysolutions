// -----------------------------------------------------------------------
// <copyright file="RegistrationEvent.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Synchronization.Core.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Synchronization.Core.Errors;
    using Synchronization.Core.Interface;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class RegistrationEventArgs : EventArgs
    {
        private string resultMessage = string.Empty;

        public RegistrationEventArgs(IList<IAuthorizationPacket> packets, bool isFirstPhase, RegistrationException error)
        {
            Packets = packets;
            Error = error;
            IsFirstPhase = isFirstPhase;
        }

        public IList<IAuthorizationPacket> Packets { get; private set; }
        public RegistrationException Error { get; private set; }
        public bool IsFirstPhase { get; private set; }

        public bool IsPassed { 
            get { 
                return 
                    Packets != null && 
                    Packets.FirstOrDefault((p) => p.IsAuthorized) != null;
            }
        }

        public string ResultMessage { get { return this.resultMessage; } }

        public string ErrorMessage { 
            get {
                StringBuilder sb = new StringBuilder();

                Exception ex = Error;
                while (ex != null)
                {
                    sb.Append(ex.Message);
                    sb.Append("\n");
                    ex = ex.InnerException;
                }

                return sb.ToString().TrimEnd('\n');
            } 
        }

        public void AppendResultMessage(string message)
        {
            this.resultMessage += "\n" + message;
        }
    }
}
