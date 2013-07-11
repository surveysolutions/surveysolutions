using System;

namespace WB.Core.Infrastructure
{
    public class MaintenanceException : Exception
    {
        public MaintenanceException(string message)
            : base(message) {}
    }
}