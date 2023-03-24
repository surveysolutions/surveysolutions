#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Newtonsoft.Json.Serialization;

namespace WB.Infrastructure.Native.Storage
{
    [Obsolete("Resolves old namespaces. Could be dropped after incompatibility shift with the next version.")]
    public class MainCoreAssemblyRedirectSerializationBaseBinder : DefaultSerializationBinder
    {
        protected string oldAssemblyNameToRedirect = "Main.Core";
        protected string targetAssemblyName = "WB.Core.SharedKernels.Questionnaire";

        protected const string OldAssemblyGenericReplacePattern = ", Main.Core";
        protected const string NewAssemblyGenericReplacePattern = ", WB.Core.SharedKernels.Questionnaire";

        protected static readonly Dictionary<string, string> TypesMap = new Dictionary<string, string>();
        protected static readonly Dictionary<Type, string> TypeToName = new Dictionary<Type, string>();

        protected MainCoreAssemblyRedirectSerializationBaseBinder()
        {
            if(initTypeError != null)
                throw new InvalidOperationException($"Assembly contains more then one type with the same name. Or with null FullName. Duplicate type: {initTypeError.Name}");
        }

        protected static Type? initTypeError = null;

        static MainCoreAssemblyRedirectSerializationBaseBinder()
        {
            var assembly = typeof(QuestionnaireDocument).Assembly;

            foreach (var type in assembly.GetTypes().Where(t => t.IsPublic))
            {
                if (TypesMap.ContainsKey(type.Name) || type.FullName == null)
                {
                    initTypeError = type;
                    return;
                }

                TypesMap[type.Name] = type.FullName;
                TypeToName[type] = type.Name;
            }
        }
    }
}
