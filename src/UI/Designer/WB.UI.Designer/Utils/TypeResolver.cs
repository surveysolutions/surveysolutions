using System;
using System.Web.Script.Serialization;

namespace WB.UI.Designer.Utils
{
    public class TypeResolver : SimpleTypeResolver
    {
        public override string ResolveTypeId(Type type)
        {
            return type.Name;
        }
    }
}