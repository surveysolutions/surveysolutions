using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.UI.Designer.Models;

namespace WB.UI.Designer.Code
{
    public interface IVerificationErrorsMapper
    {
        VerificationMessage[] EnrichVerificationErrors(QuestionnaireVerificationMessage[] verificationMessages, QuestionnaireDocument questionnaireDocument);
    }

    public class VerificationErrorsMapper : IVerificationErrorsMapper
    {
        public VerificationMessage[] EnrichVerificationErrors(QuestionnaireVerificationMessage[] verificationMessages, QuestionnaireDocument questionnaireDocument)
        {
            var errors = verificationMessages
                .Where(x => x.References.Count() == 1)
                .GroupBy(x => new { x.Code, x.Message })
                .Select(x => new VerificationMessage
                {
                    Code = x.Key.Code,
                    Message = x.Key.Message,
                    IsGroupedMessage = true,
                    Errors = x.Select(g => new VerificationMessageError()
                    {
                        References = GetEnrichedReferences(g.References, questionnaireDocument).ToList(),
                        CompilationErrorMessages = g.CompilationErrorMessages
                    }).ToList()
                }).ToList();

            errors.AddRange(verificationMessages
                .Where(x => x.References.Count() != 1).Select(x => new VerificationMessage
                {
                    Code = x.Code,
                    Message = x.Message,
                    IsGroupedMessage = false,
                    Errors = new List<VerificationMessageError>()
                    {
                        new VerificationMessageError()
                        {
                            CompilationErrorMessages = x.CompilationErrorMessages,
                            References = GetEnrichedReferences(x.References, questionnaireDocument).ToList(),
                        }
                    }
                }));

            return errors.OrderBy(x => x.Code).ToArray();
        }

        private IEnumerable<VerificationReferenceEnriched> GetEnrichedReferences(
            IEnumerable<QuestionnaireNodeReference> references,
            QuestionnaireDocument questionnaireDocument)
        {
            foreach (var reference in references)
            {
                if (reference.Type == QuestionnaireVerificationReferenceType.Attachment)
                {
                    var attachment = questionnaireDocument.Attachments.Single(x => x.AttachmentId == reference.Id);
                    yield return new VerificationReferenceEnriched
                    {
                        ItemId = reference.Id.FormatGuid(),
                        Type = QuestionnaireVerificationReferenceType.Attachment,
                        Variable = attachment.Name,
                        Title = attachment.Name
                    };
                    continue;
                }
                if (reference.Type == QuestionnaireVerificationReferenceType.Macro)
                {
                    var macro = questionnaireDocument.Macros.First(x => x.Key == reference.Id);
                    yield return new VerificationReferenceEnriched
                    {
                        ItemId = reference.Id.FormatGuid(),
                        Type = QuestionnaireVerificationReferenceType.Macro,
                        Variable = macro.Value.Name,
                        Title = macro.Value.Content
                    };
                    continue;
                }
                if (reference.Type == QuestionnaireVerificationReferenceType.LookupTable)
                {
                    var lookupTable = questionnaireDocument.LookupTables.First(x => x.Key == reference.Id);
                    yield return new VerificationReferenceEnriched
                    {
                        ItemId = reference.Id.FormatGuid(),
                        Type = QuestionnaireVerificationReferenceType.LookupTable,
                        Variable = lookupTable.Value.TableName,
                        Title = lookupTable.Value.FileName
                    };
                    continue;
                }
                if (reference.Type == QuestionnaireVerificationReferenceType.Translation)
                {
                    var translation = questionnaireDocument.Translations.First(x => x.Id == reference.Id);
                    yield return new VerificationReferenceEnriched
                    {
                        ItemId = reference.Id.FormatGuid(),
                        Type = QuestionnaireVerificationReferenceType.Translation,
                        Title = translation.Name
                    };
                    continue;
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
                    else
                    {
                        parent = grandParent;
                    }
                }

                if (reference.Type == QuestionnaireVerificationReferenceType.Group
                    || reference.Type == QuestionnaireVerificationReferenceType.Roster)
                {
                    var group = questionnaireDocument.Find<IGroup>(reference.Id);

                    yield return new VerificationReferenceEnriched
                    {
                        ItemId = reference.Id.FormatGuid(),
                        Type = group.IsRoster ? QuestionnaireVerificationReferenceType.Roster : reference.Type,
                        Variable = group.IsRoster ? group.VariableName : null,
                        Title = group.Title,
                        ChapterId = Monads.Maybe(() => parent.PublicKey.FormatGuid())
                    };
                }
                else if (reference.Type == QuestionnaireVerificationReferenceType.StaticText)
                {
                    var staticText = questionnaireDocument.Find<IStaticText>(reference.Id);

                    yield return new VerificationReferenceEnriched
                    {
                        ItemId = reference.Id.FormatGuid(),
                        Type = reference.Type,
                        Title = string.IsNullOrEmpty(staticText.Text) ? "static text" : staticText.Text,
                        ChapterId = Monads.Maybe(() => parent.PublicKey.FormatGuid()),
                        FailedValidationConditionIndex = reference.FailedValidationConditionIndex
                    };
                }
                else if (reference.Type == QuestionnaireVerificationReferenceType.Variable)
                {
                    var variable = questionnaireDocument.Find<IVariable>(reference.Id);
                    yield return new VerificationReferenceEnriched
                    {
                        ItemId = reference.Id.FormatGuid(),
                        Type = reference.Type,
                        Variable = variable.Name,
                        ChapterId = Monads.Maybe(() => parent.PublicKey.FormatGuid())
                    };
                }
                else
                {
                    var question = questionnaireDocument.Find<IQuestion>(reference.Id);

                    yield return new VerificationReferenceEnriched
                    {
                        ItemId = reference.Id.FormatGuid(),
                        Type = reference.Type,
                        Variable = question.StataExportCaption,
                        QuestionType = "icon-" + question.QuestionType.ToString().ToLower(),
                        Title = question.QuestionText,
                        ChapterId = Monads.Maybe(() => parent.PublicKey.FormatGuid()),
                        FailedValidationConditionIndex = reference.FailedValidationConditionIndex
                    };
                }
            }
        }
    }
}