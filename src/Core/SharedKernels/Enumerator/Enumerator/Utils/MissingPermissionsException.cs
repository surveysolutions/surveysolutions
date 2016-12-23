using System;
using Plugin.Permissions.Abstractions;

namespace WB.Core.SharedKernels.Enumerator.Utils
{
    public class MissingPermissionsException : Exception
    {
        public Permission Permission { get; }

        public MissingPermissionsException()
        {
        }

        public MissingPermissionsException(string message, Permission permission) : base(message)
        {
            this.Permission = permission;
        }

        public MissingPermissionsException(string message, Permission permission, Exception inner) : base(message, inner)
        {
            this.Permission = permission;
        }
    }
}