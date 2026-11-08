using System;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.UI.Shared.Enumerator.ValueCombiners
{
    public class EntityNavigator
    {
        public async Task NavigateToEntity(string entityVariable, IInterviewEntity sourceEntity)
        {
            if (sourceEntity.NavigationState == null) return;
            if (string.IsNullOrEmpty(entityVariable)) return;

            entityVariable = entityVariable.ToLower();

            if (entityVariable == "cover")
                await sourceEntity.NavigationState.NavigateTo(NavigationIdentity.CreateForCoverScreen());
            else if (entityVariable == "complete")
                await sourceEntity.NavigationState.NavigateTo(NavigationIdentity.CreateForCompleteScreen());
            else if (entityVariable == "overview")
                await sourceEntity.NavigationState.NavigateTo(NavigationIdentity.CreateForOverviewScreen());
            else
            {
                var interview = ServiceLocator.Current.GetInstance<IStatefulInterviewRepository>().Get(sourceEntity.InterviewId);
                if (interview == null) return;

                var questionnaire = ServiceLocator.Current.GetInstance<IQuestionnaireStorage>()
                    .GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
                if (questionnaire == null) return;

                var attachmentId = questionnaire.GetAttachmentIdByName(entityVariable);
                if (attachmentId.HasValue)
                    await NavigateToAttachmentAsync(sourceEntity, attachmentId, questionnaire);
                else
                    await NavigateToQuestionOrRosterOrSection(entityVariable, sourceEntity, questionnaire, interview);
            }
        }

        private static async Task NavigateToQuestionOrRosterOrSection(string entityVariable, IInterviewEntity sourceEntity,
            IQuestionnaire questionnaire, IStatefulInterview interview)
        {
            var questionId = questionnaire.GetQuestionIdByVariable(entityVariable);
            Guid? rosterOrGroupId = null;

            if (questionId == null)
                rosterOrGroupId = questionnaire.GetRosterIdByVariableName(entityVariable, true)
                                  ?? questionnaire.GetSectionIdByVariable(entityVariable);

            if (!questionId.HasValue && !rosterOrGroupId.HasValue) return;

            Identity nearestInterviewEntity = null;
            var interviewEntities = interview.GetAllIdentitiesForEntityId(questionId ?? rosterOrGroupId.Value)
                .Where(interview.IsEnabled).ToArray();

            if (interviewEntities.Length == 1)
                nearestInterviewEntity = interviewEntities[0];
            else
            {
                var entitiesInTheSameOrDeeperRoster = interviewEntities.Where(x =>
                    x.RosterVector.Identical(sourceEntity.Identity.RosterVector,
                        sourceEntity.Identity.RosterVector.Length)).ToArray();

                if (entitiesInTheSameOrDeeperRoster.Any())
                    nearestInterviewEntity = entitiesInTheSameOrDeeperRoster.FirstOrDefault();
                else
                {
                    var sourceEntityParentRosterVectors =
                        interview.GetParentGroups(sourceEntity.Identity).Select(x => x.RosterVector).ToArray();

                    nearestInterviewEntity = interviewEntities.FirstOrDefault(x =>
                                                 x.Id == (questionId ?? rosterOrGroupId.Value) &&
                                                 sourceEntityParentRosterVectors.Contains(x.RosterVector)) ??
                                             interviewEntities.FirstOrDefault();
                }
            }

            if (nearestInterviewEntity == null) return;

            if (questionId.HasValue)
            {
                await sourceEntity.NavigationState.NavigateTo(new NavigationIdentity
                {
                    TargetScreen = questionnaire.IsIdentifying(questionId.Value)
                        ? ScreenType.Cover
                        : ScreenType.Group,
                    TargetGroup = interview.GetParentGroup(nearestInterviewEntity),
                    AnchoredElementIdentity = nearestInterviewEntity
                });
            }
            else if (rosterOrGroupId.HasValue)
            {
                await sourceEntity.NavigationState.NavigateTo(new NavigationIdentity
                {
                    TargetScreen = ScreenType.Group,
                    TargetGroup = interview.GetParentGroup(nearestInterviewEntity) ?? nearestInterviewEntity, //for section(chapter) it would be opened
                    AnchoredElementIdentity = nearestInterviewEntity
                });
            }
        }

        private static async Task NavigateToAttachmentAsync(IInterviewEntity sourceEntity, Guid? attachmentId, IQuestionnaire questionnaire)
        {
            var attachmentContentStorage = ServiceLocator.Current.GetInstance<IAttachmentContentStorage>();
            var attachment = questionnaire.GetAttachmentById(attachmentId.Value);

            var attachmentContentMetadata = attachmentContentStorage.GetMetadata(attachment.ContentId);
            if (!attachmentContentMetadata.ContentType.StartsWith("application/pdf", StringComparison.OrdinalIgnoreCase)) return;

            var pdfService = ServiceLocator.Current.GetInstance<IInterviewPdfService>();
            await pdfService.OpenAttachmentAsync(sourceEntity.InterviewId, attachmentId.Value);
        }
    }
}
