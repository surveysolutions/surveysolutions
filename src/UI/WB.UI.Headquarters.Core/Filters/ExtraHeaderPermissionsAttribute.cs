using System;

namespace WB.UI.Headquarters.Filters
{
    public class ExtraHeaderPermissionsAttribute : Attribute
    {
        public HeaderPermissionType[] PermissionTypes { get;}

        public ExtraHeaderPermissionsAttribute(params HeaderPermissionType[] permissionTypes)
        { 
            PermissionTypes = permissionTypes;
        }
    }
}
