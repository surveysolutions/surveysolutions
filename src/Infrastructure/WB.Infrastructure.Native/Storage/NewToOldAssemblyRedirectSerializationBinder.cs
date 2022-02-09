using System;
using System.Collections.Generic;

namespace WB.Infrastructure.Native.Storage
{
    [Obsolete("Resolves old namespaces. Could be dropped after incompatibility shift with the next version.")]
    public class NewToOldAssemblyRedirectSerializationBinder : MainCoreAssemblyRedirectSerializationBaseBinder
    {
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
            if (TypeToName.TryGetValue(serializedType, out var typeShortName))
            {
                assemblyName = oldAssemblyNameToRedirect;
                typeName = oldTypesMap.TryGetValue(typeShortName, out var fullTypeName) ? fullTypeName : serializedType.FullName;
            }
            else
            {
                assemblyName = serializedType.Assembly.FullName;
                typeName = serializedType.FullName?.Replace(NewAssemblyGenericReplacePattern, OldAssemblyGenericReplacePattern); //generic
            }
        }

        private readonly Dictionary<string, string> oldTypesMap = new Dictionary<string, string>()
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
}
