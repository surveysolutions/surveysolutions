using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Verifier
{
    public class QuestionnairePreVerifications : AbstractVerifier, IQuestionnairePreVerifier
    {
        private IEnumerable<Func<ReadOnlyQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>>>
            ErrorsVerifiers =>
            new Func<ReadOnlyQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>>[]
            {
                ErrorsByQuestionnaireEntitiesShareSameInternalId_WB0102,
                Critical_EntitiesWithDuplicateVariableName_WB0026
            };

        private static IEnumerable<QuestionnaireVerificationMessage>
            ErrorsByQuestionnaireEntitiesShareSameInternalId_WB0102(ReadOnlyQuestionnaireDocument questionnaire)
        {
            return questionnaire
                .GetAllEntitiesIdAndTypePairsInQuestionnaireFlowOrder()
                .GroupBy(x => x.Id)
                .Where(group => group.Count() > 1)
                .Select(group =>
                    QuestionnaireVerificationMessage.Critical(
                        "WB0102",
                        VerificationMessages.WB0102_QuestionnaireEntitiesShareSameInternalId,
                        group.Select(x =>
                            new QuestionnaireEntityReference(
                                GetReferenceTypeByItemTypeAndId(questionnaire, x.Id, x.Type), x.Id)).ToArray()));
        }

        private static IEnumerable<QuestionnaireVerificationMessage> Critical_EntitiesWithDuplicateVariableName_WB0026(
            ReadOnlyQuestionnaireDocument questionnaire)
        {
            var rosterVariableNameMappedOnRosters = questionnaire
                .Find<IGroup>(g => g.IsRoster && !string.IsNullOrEmpty(g.VariableName))
                .Select(r => new
                {
                    Name = r.VariableName,
                    Reference = QuestionnaireEntityReference.CreateForRoster(r.PublicKey)
                })
                .Union(questionnaire
                    .Find<IGroup>(g => !g.IsRoster && !string.IsNullOrEmpty(g.VariableName))
                    .Select(r => new
                    {
                        Name = r.VariableName,
                        Reference = QuestionnaireEntityReference.CreateForGroup(r.PublicKey)
                    }))
                .Union(questionnaire.Find<IQuestion>(q => true)
                    .Where(x => !string.IsNullOrEmpty(x.StataExportCaption))
                    .Select(r => new
                    {
                        Name = r.StataExportCaption,
                        Reference = CreateReference(r)
                    }))
                .Union(questionnaire.LookupTables.Where(x => !string.IsNullOrEmpty(x.Value.TableName))
                    .Select(r => new
                    {
                        Name = r.Value.TableName,
                        Reference = QuestionnaireEntityReference.CreateForLookupTable(r.Key)
                    }))
                .Union(questionnaire.Find<IVariable>(x => !string.IsNullOrEmpty(x.Name))
                    .Where(x => !string.IsNullOrEmpty(x.Name))
                    .Select(r => new
                    {
                        Name = r.Name,
                        Reference = CreateReference(r)
                    })
                ).Union(questionnaire.Categories
                    .Where(x => !string.IsNullOrEmpty(x.Name))
                    .Select(r => new
                    {
                        Name = r.Name,
                        Reference = QuestionnaireEntityReference.CreateForCategories(r.Id)
                    })
                ).Union(questionnaire.VariableName.ToEnumerable()
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Select(r => new
                    {
                        Name = r,
                        Reference = QuestionnaireEntityReference.CreateForQuestionnaire(questionnaire.PublicKey)
                    })

                ).ToList();


            return rosterVariableNameMappedOnRosters
                .GroupBy(s => s.Name, StringComparer.InvariantCultureIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => QuestionnaireVerificationMessage.Critical(
                    "WB0026",
                    VerificationMessages.WB0026_ItemsWithTheSameNamesFound,
                    group.Select(x => x.Reference).ToArray()));
        }

        private static QuestionnaireVerificationReferenceType GetReferenceTypeByItemTypeAndId(
            ReadOnlyQuestionnaireDocument questionnaire, Guid id, Type entityType)
        {
            if (typeof(IQuestion).IsAssignableFrom(entityType))
                return QuestionnaireVerificationReferenceType.Question;

            if (entityType.IsAssignableFrom(typeof(StaticText)))
                return QuestionnaireVerificationReferenceType.StaticText;

            if (entityType.IsAssignableFrom(typeof(Variable)))
                return QuestionnaireVerificationReferenceType.Variable;

            var group = questionnaire.Find<IGroup>(id);

            return questionnaire.IsRoster(group)
                ? QuestionnaireVerificationReferenceType.Roster
                : QuestionnaireVerificationReferenceType.Group;
        }

        public IEnumerable<QuestionnaireVerificationMessage> Verify(ReadOnlyQuestionnaireDocument questionnaireDocument)
        {
            var verificationMessagesByQuestionnaire = new List<QuestionnaireVerificationMessage>();
            foreach (var verifier in ErrorsVerifiers)
            {
                verificationMessagesByQuestionnaire.AddRange(verifier.Invoke(questionnaireDocument));
            }

            return verificationMessagesByQuestionnaire;
        }
    }
}
