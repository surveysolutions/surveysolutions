using System;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Upgrade
{
    public class AssignmentUpgradeException : Exception
    {
        public AssignmentUpgradeException(string message) : base(message)
        {
        }
    }
}