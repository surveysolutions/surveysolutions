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

        protected const string oldAssemblyGenericReplacePattern = ", Main.Core";
        protected const string newAssemblyGenericReplacePattern = ", WB.Core.SharedKernels.Questionnaire";

        protected static readonly Dictionary<string, string> typesMap = new Dictionary<string, string>();
        protected static readonly Dictionary<Type, string> typeToName = new Dictionary<Type, string>();

        protected MainCoreAssemblyRedirectSerializationBaseBinder()
        {
        }

        static MainCoreAssemblyRedirectSerializationBaseBinder()
        {
            var assembly = typeof(QuestionnaireDocument).Assembly;

            foreach (var type in assembly.GetTypes().Where(t => t.IsPublic))
            {
                if (typesMap.ContainsKey(type.Name))
                    throw new InvalidOperationException($"Assembly contains more then one type with same name. Duplicate type: {type.Name}");

                typesMap[type.Name] = type.FullName;
                typeToName[type] = type.Name;
            }
        }
    }
}
