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
        protected static readonly Dictionary<string, Type> NameToType = new Dictionary<string, Type>();

        static OldToNewAssemblyRedirectSerializationBinder()
        {
            var interviewAnswerType = typeof(InterviewAnswer);
            var assembly = interviewAnswerType.Assembly;
            TypesMap[interviewAnswerType.Name] = interviewAnswerType.FullName;
            TypeToName[interviewAnswerType] = interviewAnswerType.Name;
            NameToType[interviewAnswerType.Name] = interviewAnswerType;

            foreach (var type in assembly.GetTypes().Where(t => t.IsPublic && t.GetBaseTypes().Any(type => type == typeof(AbstractAnswer))))
            {
                if (TypesMap.ContainsKey(type.Name))
                {
                    initTypeError = type;
                    return;
                }
                TypesMap[type.Name] = type.FullName;
                TypeToName[type] = type.Name;
                NameToType[type.Name] = type;
            }
        }
        public override Type BindToType(string assemblyName, string typeName)
        {
            var typeNameWithoutNamespace = typeName.Split('.').Last();
            if (string.IsNullOrEmpty(assemblyName) && NameToType.TryGetValue(typeNameWithoutNamespace, out Type type))
            {
                typeName = type.FullName;
                assemblyName = type.Assembly.FullName;
            }
            else if (string.Equals(assemblyName, oldAssemblyNameToRedirect, StringComparison.Ordinal) ||
                string.Equals(assemblyName, targetAssemblyName, StringComparison.Ordinal) ||
                string.IsNullOrEmpty(assemblyName))
            {
                assemblyName = targetAssemblyName;

                if (TypesMap.TryGetValue(typeNameWithoutNamespace, out string fullTypeName))
                    typeName = fullTypeName;
            }
            else
            {
                //generic replace
                typeName = typeName.Replace(OldAssemblyGenericReplacePattern, NewAssemblyGenericReplacePattern);
            }

            return base.BindToType(assemblyName, typeName);
        }

        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            if (TypeToName.TryGetValue(serializedType, out string name))
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
