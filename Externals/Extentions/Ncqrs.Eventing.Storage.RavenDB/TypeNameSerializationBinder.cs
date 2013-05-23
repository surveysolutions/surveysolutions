using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Ncqrs.Eventing.Storage.RavenDB
{
    public class TypeNameSerializationBinder : SerializationBinder
    {
        public string TypeFormat { get; private set; }

        public TypeNameSerializationBinder(string typeFormat)
        {
            TypeFormat = typeFormat;
        }

        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.FullName;
        }

        public override Type BindToType(string assemblyName, string typeName)
        {
            string resolvedTypeName = string.Format(TypeFormat, typeName);
            var types = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(resolvedTypeName)).ToList();
            var type = Type.GetType(resolvedTypeName) ?? types.FirstOrDefault(t => t != null);
            if (type == null)
                throw new TypeLoadException(string.Format("Cannot load type {0}", typeName));
            return type;
        }
    }
}
