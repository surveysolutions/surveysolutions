using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WB.Core.GenericSubdomains.Portable.Services;
using System.Linq;
using Main.Core.Documents;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public class JsonSerializerSettingsFactory : IJsonSerializerSettingsFactory
    {
        private readonly Dictionary<TypeSerializationSettings, JsonSerializerSettings> jsonSerializerSettingsByTypeNameHandling;
        private Dictionary<string, string> assemblyRemappings = new Dictionary<string, string>();
        
        private readonly NewToOldAssemblyRedirectSerializationBinder newToOldBinder = new NewToOldAssemblyRedirectSerializationBinder();
        private readonly OldToNewAssemblyRedirectSerializationBinder oldToNewBinder = new OldToNewAssemblyRedirectSerializationBinder();

        public JsonSerializerSettingsFactory()
        {
            

            jsonSerializerSettingsByTypeNameHandling =
               new Dictionary<TypeSerializationSettings, JsonSerializerSettings>()
                {
                    {
                        TypeSerializationSettings.AllTypes, new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.All,
                            NullValueHandling = NullValueHandling.Ignore,
                            FloatParseHandling = FloatParseHandling.Decimal
                        }
                    },
                    {
                        TypeSerializationSettings.ObjectsOnly, new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.Objects,
                            NullValueHandling = NullValueHandling.Ignore,
                            FloatParseHandling = FloatParseHandling.Decimal,
                            Formatting = Formatting.None
                        }
                    },
                    {
                        TypeSerializationSettings.None, new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.None,
                            NullValueHandling = NullValueHandling.Ignore,
                            FloatParseHandling = FloatParseHandling.Decimal,
                            Formatting = Formatting.None
                        }
                    },

                    {
                        TypeSerializationSettings.Auto, new JsonSerializerSettings()
                        {
                            TypeNameHandling = TypeNameHandling.Auto,
                            DefaultValueHandling = DefaultValueHandling.Ignore,
                            MissingMemberHandling = MissingMemberHandling.Ignore,
                            NullValueHandling = NullValueHandling.Ignore,
                            Formatting = Formatting.None
                        }
                   }
                };
        }

        public JsonSerializerSettings GetJsonSerializerSettings(TypeSerializationSettings typeSerialization)
        {
            return this.GetJsonSerializerSettings(typeSerialization, SerializationBinderSettings.OldToNew);
        }

        public JsonSerializerSettings GetJsonSerializerSettings(TypeSerializationSettings typeSerialization, SerializationBinderSettings binderSettings)
        {
            var settings = jsonSerializerSettingsByTypeNameHandling[typeSerialization];
            switch (binderSettings)
            {
                case SerializationBinderSettings.OldToNew:
                    settings.Binder = this.oldToNewBinder;
                    break;
                case SerializationBinderSettings.NewToOld:
                    settings.Binder = this.newToOldBinder;
                    break;
            }

            return settings;
        }

        private class NewToOldAssemblyRedirectSerializationBinder : DefaultSerializationBinder
        {
            private string oldAssemblyNameToRedirect = "Main.Core";
            private string targetAssemblyName = "WB.Core.SharedKernels.Questionnaire";

            private const string oldAssemblyGenericReplacePattern = ", Main.Core]";
            private const string newAssemblyGenericReplacePattern = ", WB.Core.SharedKernels.Questionnaire]";

            private readonly Dictionary<string, string> typesMap = new Dictionary<string, string>();
            private readonly Dictionary<Type, string> typeToName = new Dictionary<Type, string>();

            public NewToOldAssemblyRedirectSerializationBinder()
            {
                var assembly = typeof(QuestionnaireDocument).Assembly;

                foreach (var type in assembly.GetTypes().Where(t => t.IsPublic))
                {
                    if (typesMap.ContainsKey(type.Name))
                        throw new InvalidOperationException("Assembly contains more then one type with same name.");

                    typesMap[type.Name] = type.FullName;
                    typeToName[type] = type.Name;
                }
            }

            public override Type BindToType(string assemblyName, string typeName)
            {
                if (String.Equals(assemblyName, targetAssemblyName, StringComparison.Ordinal) ||
                    String.IsNullOrEmpty(assemblyName))
                {
                    assemblyName = oldAssemblyNameToRedirect;
                }

                return base.BindToType(assemblyName, typeName);
            }

            public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
            {
                string typeShortName;
                if (typeToName.TryGetValue(serializedType, out typeShortName))
                { 
                    assemblyName = oldAssemblyNameToRedirect;

                    string fullTypeName;
                    if (oldTypesMap.TryGetValue(typeShortName, out fullTypeName))
                        typeName = fullTypeName;
                    else
                    {
                        typeName = serializedType.FullName;
                    }
                }
                else
                {
                    assemblyName = serializedType.Assembly.FullName;
                    typeName = serializedType.FullName.Replace(newAssemblyGenericReplacePattern, oldAssemblyGenericReplacePattern); //generic
                }
            }

            private Dictionary<string, string> oldTypesMap = new Dictionary<string, string>()
        {
            {"ValidationConditionsBackwardCompatibility", "WB.Core.SharedKernels.NonConficltingNamespace.ValidationConditionsBackwardCompatibility"},
            {"ValidationCondition", "WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition"},
            {"IReadSideRepositoryEntity", "WB.Core.SharedKernels.SurveySolutions.IReadSideRepositoryEntity"},
            {"CompositeException", "WB.Core.SharedKernels.SurveySolutions.Documents.CompositeException"},
            {"Constants", "WB.Core.SharedKernels.SurveySolutions.Documents.Constants"},
            {"FixedRosterTitle", "WB.Core.SharedKernels.SurveySolutions.Documents.FixedRosterTitle"},
            {"IView", "WB.Core.SharedKernels.SurveySolutions.Documents.IView"},
            {"LookupTable", "WB.Core.SharedKernels.SurveySolutions.Documents.LookupTable"},
            {"Macro", "WB.Core.SharedKernels.SurveySolutions.Documents.Macro"},
            {"IQuestionnaireDocument", "Main.Core.Documents.IQuestionnaireDocument"},
            {"QuestionnaireDocument", "Main.Core.Documents.QuestionnaireDocument"},
            {"QuestionData", "Main.Core.Entities.QuestionData"},
            {"IComposite", "Main.Core.Entities.Composite.IComposite"},
            {"AbstractQuestion", "Main.Core.Entities.SubEntities.AbstractQuestion"},
            {"Answer", "Main.Core.Entities.SubEntities.Answer"},
            {"GeoPosition", "Main.Core.Entities.SubEntities.GeoPosition"},
            {"Group", "Main.Core.Entities.SubEntities.Group"},
            {"IConditional", "Main.Core.Entities.SubEntities.IConditional"},
            {"IGroup", "Main.Core.Entities.SubEntities.IGroup"},
            {"Image", "Main.Core.Entities.SubEntities.Image"},
            {"IQuestion", "Main.Core.Entities.SubEntities.IQuestion"},
            {"IStaticText", "Main.Core.Entities.SubEntities.IStaticText"},
            {"Option", "Main.Core.Entities.SubEntities.Option"},
            {"Order", "Main.Core.Entities.SubEntities.Order"},
            {"QuestionIdAndVariableName", "Main.Core.Entities.SubEntities.QuestionIdAndVariableName"},
            {"QuestionScope", "Main.Core.Entities.SubEntities.QuestionScope"},
            {"QuestionType", "Main.Core.Entities.SubEntities.QuestionType"},
            {"RosterSizeSourceType", "Main.Core.Entities.SubEntities.RosterSizeSourceType"},
            {"ShareType", "Main.Core.Entities.SubEntities.ShareType"},
            {"StaticText", "Main.Core.Entities.SubEntities.StaticText"},
            {"UserLight", "Main.Core.Entities.SubEntities.UserLight"},
            {"UserRoles", "Main.Core.Entities.SubEntities.UserRoles"},
            {"DateTimeQuestion", "Main.Core.Entities.SubEntities.Question.DateTimeQuestion"},
            {"GpsCoordinateQuestion", "Main.Core.Entities.SubEntities.Question.GpsCoordinateQuestion"},
            {"IMultimediaQuestion", "Main.Core.Entities.SubEntities.Question.IMultimediaQuestion"},
            {"IMultyOptionsQuestion", "Main.Core.Entities.SubEntities.Question.IMultyOptionsQuestion"},
            {"INumericQuestion", "Main.Core.Entities.SubEntities.Question.INumericQuestion"},
            {"IQRBarcodeQuestion", "Main.Core.Entities.SubEntities.Question.IQRBarcodeQuestion"},
            {"ITextListQuestion", "Main.Core.Entities.SubEntities.Question.ITextListQuestion"},
            {"MultimediaQuestion", "Main.Core.Entities.SubEntities.Question.MultimediaQuestion"},
            {"MultyOptionsQuestion", "Main.Core.Entities.SubEntities.Question.MultyOptionsQuestion"},
            {"NumericQuestion", "Main.Core.Entities.SubEntities.Question.NumericQuestion"},
            {"QRBarcodeQuestion", "Main.Core.Entities.SubEntities.Question.QRBarcodeQuestion"},
            {"SingleQuestion", "Main.Core.Entities.SubEntities.Question.SingleQuestion"},
            {"TextListQuestion", "Main.Core.Entities.SubEntities.Question.TextListQuestion"},
            {"TextQuestion", "Main.Core.Entities.SubEntities.Question.TextQuestion"}
        };
        }

        private class OldToNewAssemblyRedirectSerializationBinder : DefaultSerializationBinder
        {
            private const string oldAssemblyNameToRedirect = "Main.Core";
            private readonly string targetAssemblyName = "WB.Core.SharedKernels.Questionnaire";
            private readonly Dictionary<string, string> typesMap = new Dictionary<string, string>();
            private readonly Dictionary<Type, string> typeToName = new Dictionary<Type, string>();

            private const string oldAssemblyGenericReplacePattern = ", Main.Core]";
            private const string newAssemblyGenericReplacePattern = ", WB.Core.SharedKernels.Questionnaire]";

            //namespaces replace
            //generic!

            public OldToNewAssemblyRedirectSerializationBinder()
            {
                typesMap = new Dictionary<string, string>();
                var assembly = typeof(QuestionnaireDocument).Assembly;
                //targetAssemblyName = assembly.FullName;

                foreach (var type in assembly.GetTypes().Where(t => t.IsPublic))
                {
                    if (typesMap.ContainsKey(type.Name))
                        throw new InvalidOperationException("Assembly contains more then one type with same name.");

                    typesMap[type.Name] = type.FullName;
                    typeToName[type] = type.Name;
                }
            }

            public override Type BindToType(string assemblyName, string typeName)
            {
                if (String.Equals(assemblyName, oldAssemblyNameToRedirect, StringComparison.Ordinal) ||
                    String.IsNullOrEmpty(assemblyName))
                {
                    assemblyName = targetAssemblyName;
                    string fullTypeName;

                    if (typesMap.TryGetValue(typeName.Split('.').Last(), out fullTypeName))
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
                //generic types are not changed
                //on load assembly is reroute but not namespace
                string name;
                if (typeToName.TryGetValue(serializedType, out name))
                {
                    assemblyName = null;
                    typeName = serializedType.Name;
                }
                else
                {
                    assemblyName = serializedType.Assembly.FullName;
                    typeName = serializedType.FullName;
                }
            }
        }
    }
}