using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Designer.Models;

namespace WB.UI.Designer.Code
{
    public interface IVerificationErrorsMapper
    {
        VerificationError[] EnrichVerificationErrors(QuestionnaireVerificationError[] verificationErrors, QuestionnaireDocument questionnaireDocument);
    }

    public class VerificationErrorsMapper : IVerificationErrorsMapper
    {
        public VerificationError[] EnrichVerificationErrors(QuestionnaireVerificationError[] verificationErrors, QuestionnaireDocument questionnaireDocument)
        {
            var errors = verificationErrors
                .Where(x => x.References.Count() == 1)
                .GroupBy(x => new { x.Code, x.Message })
                .Select(x => new VerificationError
                {
                    Code = x.Key.Code,
                    Message = x.Key.Message,
                    References = x.SelectMany(g => GetEnrichedReferences(g.References, questionnaireDocument)).ToList(),
                    IsGroupOfErrors = true
                }).ToList();

            errors.AddRange(verificationErrors
                .Where(x => x.References.Count() != 1).Select(x => new VerificationError
                {
                    Code = x.Code,
                    Message = x.Message,
                    References = GetEnrichedReferences(x.References, questionnaireDocument).ToList(),
                    IsGroupOfErrors = false
                }));

            return errors.OrderBy(x => x.Code).ToArray();
        }

        private IEnumerable<VerificationReference> GetEnrichedReferences(
            IEnumerable<QuestionnaireVerificationReference> references,
            QuestionnaireDocument questionnaireDocument)
        {
            foreach (var reference in references)
            {
                var item = questionnaireDocument.Find<IComposite>(reference.Id);
                var parent = item;
                while (parent != null)
                {
                    IComposite grandParent = parent.GetParent();
                    if (grandParent == null || grandParent.GetParent() == null)
                    {
                        break;
                    }
                    else
                    {
                        parent = grandParent;
                    }
                }

                if (reference.Type == QuestionnaireVerificationReferenceType.Group)
                {
                    var group = questionnaireDocument.Find<IGroup>(reference.Id);

                    yield return new VerificationReference
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

                    yield return new VerificationReference
                    {
                        ItemId = reference.Id.FormatGuid(),
                        Type = reference.Type,
                        Title = string.IsNullOrEmpty(staticText.Text) ? "static text" : staticText.Text,
                        ChapterId = Monads.Maybe(() => parent.PublicKey.FormatGuid())
                    };
                }
                else
                {
                    var question = questionnaireDocument.Find<IQuestion>(reference.Id);

                    yield return new VerificationReference
                    {
                        ItemId = reference.Id.FormatGuid(),
                        Type = reference.Type,
                        Variable = question.StataExportCaption,
                        QuestionType = "icon-" + question.QuestionType.ToString().ToLower(),
                        Title = question.QuestionText,
                        ChapterId = Monads.Maybe(() => parent.PublicKey.FormatGuid())
                    };
                }
            }
        }
    }
}