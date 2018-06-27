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
                {
                    ItemId = reference.Id.FormatGuid(),
                    Type = QuestionnaireVerificationReferenceType.Questionnaire,
                    Variable = questionnaireDocument.VariableName,
                    Title = questionnaireDocument.Title
                };
            }

            if (reference.Type == QuestionnaireVerificationReferenceType.Attachment)
            {
                var attachment = questionnaireDocument.Attachments.Single(x => x.AttachmentId == reference.Id);
                return new QuestionnaireEntityExtendedReference
                {
                    ItemId = reference.Id.FormatGuid(),
                    Type = QuestionnaireVerificationReferenceType.Attachment,
                    Variable = attachment.Name,
                    Title = attachment.Name
                };
            }

            if (reference.Type == QuestionnaireVerificationReferenceType.Macro)
            {
                var macro = questionnaireDocument.Macros.First(x => x.Key == reference.Id);
                return new QuestionnaireEntityExtendedReference
                {
                    ItemId = reference.Id.FormatGuid(),
                    Type = QuestionnaireVerificationReferenceType.Macro,
                    Variable = macro.Value.Name,
                    Title = macro.Value.Content
                };
            }

            if (reference.Type == QuestionnaireVerificationReferenceType.LookupTable)
            {
                var lookupTable = questionnaireDocument.LookupTables.First(x => x.Key == reference.Id);
                return new QuestionnaireEntityExtendedReference
                {
                    ItemId = reference.Id.FormatGuid(),
                    Type = QuestionnaireVerificationReferenceType.LookupTable,
                    Variable = lookupTable.Value.TableName,
                    Title = lookupTable.Value.FileName
                };
            }

            if (reference.Type == QuestionnaireVerificationReferenceType.Translation)
            {
                var translation = questionnaireDocument.Translations.First(x => x.Id == reference.Id);
                return new QuestionnaireEntityExtendedReference
                {
                    ItemId = reference.Id.FormatGuid(),
                    Type = QuestionnaireVerificationReferenceType.Translation,
                    Title = translation.Name
                };
            }

            var item = questionnaireDocument.Find<IComposite>(reference.Id);
            var parent = item;
            while (parent != null)
            {
                IComposite grandParent = parent.GetParent();
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
                {
                    ItemId = reference.Id.FormatGuid(),
                    Type = @group.IsRoster ? QuestionnaireVerificationReferenceType.Roster : reference.Type,
                    Variable = @group.IsRoster ? @group.VariableName : null,
                    Title = @group.Title,
                    ChapterId = parent?.PublicKey.FormatGuid()
                };
            }

            if (reference.Type == QuestionnaireVerificationReferenceType.StaticText)
            {
                var staticText = questionnaireDocument.Find<IStaticText>(reference.Id);

                return new QuestionnaireEntityExtendedReference
                {
                    ItemId = reference.Id.FormatGuid(),
                    Type = reference.Type,
                    Title = String.IsNullOrEmpty(staticText.Text) ? "static text" : staticText.Text,
                    ChapterId = parent?.PublicKey.FormatGuid(),
                    IndexOfEntityInProperty = reference.IndexOfEntityInProperty
                };
            }

            if (reference.Type == QuestionnaireVerificationReferenceType.Variable)
            {
                var variable = questionnaireDocument.Find<IVariable>(reference.Id);
                return new QuestionnaireEntityExtendedReference
                {
                    ItemId = reference.Id.FormatGuid(),
                    Type = reference.Type,
                    Title = variable.Label,
                    Variable = variable.Name,
                    ChapterId = parent?.PublicKey.FormatGuid()
                };
            }
            else
            {
                var question = questionnaireDocument.Find<IQuestion>(reference.Id);

                return new QuestionnaireEntityExtendedReference
                {
                    ItemId = reference.Id.FormatGuid(),
                    Type = reference.Type,
                    Variable = question.StataExportCaption,
                    QuestionType = "icon-" + question.QuestionType.ToString().ToLower(),
                    Title = question.QuestionText,
                    ChapterId = parent?.PublicKey.FormatGuid(),
                    IndexOfEntityInProperty = reference.IndexOfEntityInProperty
                };
            }
        }
    }
}
