using System;
using Ncqrs;
using Raven.Imports.Newtonsoft.Json.Serialization;

namespace WB.Core.Infrastructure.Storage.Raven.Implementation.WriteSide
{
    internal class IgnoreAssemblyNameForEventsSerializationBinder : DefaultSerializationBinder
    {
        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            if (NcqrsEnvironment.IsEventDataType(serializedType.FullName))
            {
                assemblyName = null;
                typeName = serializedType.FullName;
            }
            else
            {
                base.BindToName(serializedType, out assemblyName, out typeName);
            }
        }

        public override Type BindToType(string assemblyName, string typeName)
        {
            
            if (NcqrsEnvironment.IsEventDataType(typeName))
            {
                return NcqrsEnvironment.GetEventDataTypeByFullName(typeName);
            }

            var replacedTypeName = ReplaceAssemblyInTypeName(typeName);
            if (NcqrsEnvironment.IsEventDataType(replacedTypeName))
            {
                return NcqrsEnvironment.GetEventDataTypeByFullName(replacedTypeName);
            }

            return base.BindToType(assemblyName, typeName);
        }

        [Obsolete]
        private static string ReplaceAssemblyInTypeName(string typeName)
        {
            return typeName.Replace(", Main.Core", ", WB.Core.Infrastructure");
        }
    }
}
