using System;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Verifier;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.ValueObjects
{
    public static class QuestionnaireEntityReferenceExtension
    {
        public static QuestionnaireEntityExtendedReference ExtendedReference(
            this QuestionnaireEntityReference reference,
            ReadOnlyQuestionnaireDocument questionnaireDocument)
        {
            if (reference.Type == QuestionnaireVerificationReferenceType.Questionnaire)
            {
                return new QuestionnaireEntityExtendedReference
                (
                    itemId : reference.Id.FormatGuid(),
                    type : QuestionnaireVerificationReferenceType.Questionnaire,
                    variable : questionnaireDocument.VariableName,
                    title : questionnaireDocument.Title
                );
            }

            if (reference.Type == QuestionnaireVerificationReferenceType.Attachment)
            {
                var attachment = questionnaireDocument.Attachments.Single(x => x.AttachmentId == reference.Id);
                return new QuestionnaireEntityExtendedReference
                (
                    itemId : reference.Id.FormatGuid(),
                    type : QuestionnaireVerificationReferenceType.Attachment,
                    variable : attachment.Name,
                    title : attachment.Name
                );
            }

            if (reference.Type == QuestionnaireVerificationReferenceType.Macro)
            {
                var macro = questionnaireDocument.Macros.First(x => x.Key == reference.Id);
                return new QuestionnaireEntityExtendedReference
                (
                    itemId : reference.Id.FormatGuid(),
                    type : QuestionnaireVerificationReferenceType.Macro,
                    variable : macro.Value.Name,
                    title : macro.Value.Content
                );
            }

            if (reference.Type == QuestionnaireVerificationReferenceType.LookupTable)
            {
                var lookupTable = questionnaireDocument.LookupTables.First(x => x.Key == reference.Id);
                return new QuestionnaireEntityExtendedReference
                (
                    itemId : reference.Id.FormatGuid(),
                    type : QuestionnaireVerificationReferenceType.LookupTable,
                    variable : lookupTable.Value.TableName,
                    title : lookupTable.Value.FileName
                );
            }

            if (reference.Type == QuestionnaireVerificationReferenceType.Translation)
            {
                var translation = questionnaireDocument.Translations.First(x => x.Id == reference.Id);
                return new QuestionnaireEntityExtendedReference
                (
                    itemId : reference.Id.FormatGuid(),
                    type : QuestionnaireVerificationReferenceType.Translation,
                    title : translation.Name
                );
            }

            if (reference.Type == QuestionnaireVerificationReferenceType.Categories)
            {
                var categories = questionnaireDocument.Categories.First(x => x.Id == reference.Id);
                return new QuestionnaireEntityExtendedReference
                (
                    itemId : reference.Id.FormatGuid(),
                    type : QuestionnaireVerificationReferenceType.Categories,
                    title : categories.Name
                );
            }

            if (reference.Type == QuestionnaireVerificationReferenceType.CriticalityCondition)
            {
                var criticalityCondition = questionnaireDocument.CriticalityConditions.First(x => x.Id == reference.Id);
                return new QuestionnaireEntityExtendedReference
                (
                    itemId : reference.Id.FormatGuid(),
                    type : QuestionnaireVerificationReferenceType.CriticalityCondition,
                    title : criticalityCondition.Message ?? string.Empty
                );
            }

            var item = questionnaireDocument.Find<IComposite>(reference.Id);
            var parent = item;
            while (parent != null)
            {
                IComposite? grandParent = parent.GetParent();
                if (grandParent?.GetParent() == null)
                {
                    break;
                }

                parent = grandParent;
            }

            if (reference.Type == QuestionnaireVerificationReferenceType.Group
                || reference.Type == QuestionnaireVerificationReferenceType.Roster)
            {
                var group = questionnaireDocument.Find<IGroup>(reference.Id);

                return new QuestionnaireEntityExtendedReference
                (
                    itemId : reference.Id.FormatGuid(),
                    type : @group?.IsRoster == true ? QuestionnaireVerificationReferenceType.Roster : reference.Type,
                    variable : @group?.VariableName,
                    title : @group?.Title ?? "",
                    chapterId : parent?.PublicKey.FormatGuid(),
                    property: reference.Property.ToString()
                );
            }

            if (reference.Type == QuestionnaireVerificationReferenceType.StaticText)
            {
                var staticText = questionnaireDocument.Find<IStaticText>(reference.Id);

                return new QuestionnaireEntityExtendedReference
                (
                    itemId : reference.Id.FormatGuid(),
                    type : reference.Type,
                    title : String.IsNullOrEmpty(staticText?.Text) ? "static text" : staticText.Text,
                    chapterId : parent?.PublicKey.FormatGuid(),
                    indexOfEntityInProperty : reference.IndexOfEntityInProperty,
                    property: reference.Property.ToString()
                );
            }

            if (reference.Type == QuestionnaireVerificationReferenceType.Variable)
            {
                var variable = questionnaireDocument.Find<IVariable>(reference.Id);
                return new QuestionnaireEntityExtendedReference
                (
                    itemId : reference.Id.FormatGuid(),
                    type : reference.Type,
                    title : variable?.Label ?? String.Empty,
                    variable : variable?.Name,
                    chapterId : parent?.PublicKey.FormatGuid(),
                    property: reference.Property.ToString()
                );
            }
            else
            {
                var question = questionnaireDocument.Find<IQuestion>(reference.Id);

                return new QuestionnaireEntityExtendedReference
                (
                    itemId : reference.Id.FormatGuid(),
                    type : reference.Type,
                    variable : question?.StataExportCaption,
                    questionType : "icon-" + question?.QuestionType.ToString().ToLower(),
                    title : question?.QuestionText ?? "",
                    chapterId : parent?.PublicKey.FormatGuid(),
                    indexOfEntityInProperty : reference.IndexOfEntityInProperty,
                    property: reference.Property.ToString()
                );
            }
        }
    }
}
