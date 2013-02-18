using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace RavenQuestionnaire.Web.Utils
{
    public class TypeResolver : SimpleTypeResolver
    {
        public override string ResolveTypeId(Type type)
        {
            return type.Name;
        }
    }
}