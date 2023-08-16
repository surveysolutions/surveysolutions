using System;

namespace WB.Core.SharedKernels.Enumerator.Utils
{
    public class MissingPermissionsException : Exception
    {
        public Type PermissionType { get; }

        public MissingPermissionsException()
        {
        }

        public MissingPermissionsException(string message, Exception inner) : base(message, inner)
        {
        }

        public MissingPermissionsException(string message, Type permissionType) : base(message)
        {
            this.PermissionType = permissionType;
        }

        public MissingPermissionsException(string message, Type permissionType, Exception inner) : base(message, inner)
        {
            this.PermissionType = permissionType;
        }
    }
}
