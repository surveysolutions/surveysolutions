using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Mapping.ByCode;
using WB.Core.SharedKernels.DataCollection.Events.Assignment;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;

namespace WB.Infrastructure.Native.Storage
{
    [Obsolete]
    public class OldToNewAssemblyRedirectSerializationBinder : MainCoreAssemblyRedirectSerializationBaseBinder
    {
        protected static readonly Dictionary<string, Type> nameToType = new Dictionary<string, Type>();

        static OldToNewAssemblyRedirectSerializationBinder()
        {
            var interviewAnswerType = typeof(InterviewAnswer);
            var assembly = interviewAnswerType.Assembly;
            typesMap[interviewAnswerType.Name] = interviewAnswerType.FullName;
            typeToName[interviewAnswerType] = interviewAnswerType.Name;
            nameToType[interviewAnswerType.Name] = interviewAnswerType;

            foreach (var type in assembly.GetTypes().Where(t => t.IsPublic && t.GetBaseTypes().Any(type => type == typeof(AbstractAnswer))))
            {
                if (typesMap.ContainsKey(type.Name))
                    throw new InvalidOperationException($"Assembly contains more then one type with same name. Duplicate type: {type.Name}");

                typesMap[type.Name] = type.FullName;
                typeToName[type] = type.Name;
                nameToType[type.Name] = type;
            }
        }
        public override Type BindToType(string assemblyName, string typeName)
        {
            var typeNameWithoutNamespace = typeName.Split('.').Last();
            if (string.IsNullOrEmpty(assemblyName) && nameToType.TryGetValue(typeNameWithoutNamespace, out Type type))
            {
                typeName = type.FullName;
                assemblyName = type.Assembly.FullName;
            }
            else if (string.Equals(assemblyName, oldAssemblyNameToRedirect, StringComparison.Ordinal) ||
                string.Equals(assemblyName, targetAssemblyName, StringComparison.Ordinal) ||
                string.IsNullOrEmpty(assemblyName))
            {
                assemblyName = targetAssemblyName;

                if (typesMap.TryGetValue(typeNameWithoutNamespace, out string fullTypeName))
                    typeName = fullTypeName;
            }
            else
            {
                //generic replace
                typeName = typeName.Replace(oldAssemblyGenericReplacePattern, newAssemblyGenericReplacePattern);
            }

            return base.BindToType(assemblyName, typeName);
        }

        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            if (typeToName.TryGetValue(serializedType, out string name))
            {
                assemblyName = null;
                typeName = name;
            }
            else
            {
                assemblyName = serializedType.Assembly.FullName;
                typeName = serializedType.FullName;
            }
        }
    }
}
