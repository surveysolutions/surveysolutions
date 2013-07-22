using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Raven.Imports.Newtonsoft.Json.Serialization;

namespace Ncqrs.Eventing.Storage.RavenDB
{
    public class IgnoreAssemblyNameForEventsSerializationBinder : DefaultSerializationBinder
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
                return NcqrsEnvironment.GetEventDataType(typeName);
            }
            else
            {
                return base.BindToType(assemblyName, typeName);
            }
        }
    }
}
