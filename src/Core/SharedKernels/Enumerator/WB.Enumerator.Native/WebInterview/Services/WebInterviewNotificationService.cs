using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.WebInterview.LifeCycle;

namespace WB.Enumerator.Native.WebInterview.Services
{
    public class WebInterviewNotificationService : IWebInterviewNotificationService
    {
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IWebInterviewInvoker webInterviewInvoker;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        
        public WebInterviewNotificationService(IStatefulInterviewRepository statefulInterviewRepository,
            IQuestionnaireStorage questionnaireStorage,
            IWebInterviewInvoker webInterviewInvoker)
        {
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.questionnaireStorage = questionnaireStorage;
            this.webInterviewInvoker = webInterviewInvoker;
        }

        public virtual void RefreshEntities(Guid interviewId, params Identity[] questions)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId.FormatGuid());

            if (interview == null)
            {
                return;
            }

            if (questions.Length > InterviewLifecycle.RefreshEntitiesLimit)
            {
                this.webInterviewInvoker.RefreshSection(interviewId);
                return;
            }

            bool doesNeedRefreshSectionList = false;

            var entitiesToRefresh = new List<(string section, Identity id)>();

            var questionnaire = questionnaireStorage.GetQuestionnaire(interview.QuestionnaireIdentity, null);

            if (questionnaire == null) return;

            foreach (var identity in questions)
            {
                if (questionnaire.IsQuestion(identity.Id) && (
                    questionnaire.IsRosterSizeQuestion(identity.Id)
                    || questionnaire.IsRosterTitleQuestion(identity.Id)
                    || questionnaire.GetSubstitutedQuestions(identity.Id).Any()
                    || questionnaire.GetSubstitutedGroups(identity.Id).Any()
                    || questionnaire.GetSubstitutedStaticTexts(identity.Id).Any()
                    || questionnaire.ShowCascadingAsList(identity.Id)
                ))
                {
                    doesNeedRefreshSectionList = true;
                }


                if (questionnaire.IsPrefilled(identity.Id))
                {
                    entitiesToRefresh.Add((WebInterview.GetConnectedClientPrefilledSectionKey(interview.Id), identity));
                }

                var currentEntity = identity;

                while (currentEntity != null)
                {
                    if (questionnaire.ShouldBeHiddenIfDisabled(currentEntity.Id))
                    {
                        doesNeedRefreshSectionList = true;
                        break;
                    }

                    var parent = this.GetParentIdentity(currentEntity, interview);
                   
                    
                    if (parent != null)
                    {
                        if (questionnaire.HasVariable(currentEntity.Id))
                        {
                            if (questionnaire.IsPrefilled(currentEntity.Id))
                            {
                                entitiesToRefresh.Add((WebInterview.GetConnectedClientSectionKey(parent, interview.Id),
                                    currentEntity));
                            }
                            
                            IEnumerable<Guid> affectedStaticTexts =
                                questionnaire.GetStaticTextsThatUseVariableAsAttachment(currentEntity.Id);
                            foreach (var staticTextId in affectedStaticTexts.SelectMany(x => interview.GetAllIdentitiesForEntityId(x)))
                            { 
                                var parentGroup = interview.GetParentGroup(staticTextId);
                                entitiesToRefresh.Add((WebInterview.GetConnectedClientSectionKey(parentGroup, interview.Id), staticTextId));
                            }
                        }
                        else if (questionnaire.IsCustomViewRoster(parent.Id))
                        {
                            var parentGroupIdentity = GetParentIdentity(parent, interview);
                            var connectedClientSectionKey = WebInterview.GetConnectedClientSectionKey(parentGroupIdentity, interview.Id);
                            entitiesToRefresh.Add((connectedClientSectionKey, currentEntity));
                        }
                        else
                        {
                            entitiesToRefresh.Add((WebInterview.GetConnectedClientSectionKey(parent, interview.Id),
                                currentEntity));
                        }
                    }

                    currentEntity = parent;
                }
            }

            if (doesNeedRefreshSectionList)
                this.webInterviewInvoker.RefreshSection(interviewId);
            else
            {
                foreach (var questionsGroupedByParent in entitiesToRefresh.GroupBy(x => x.section))
                {
                    if (questionsGroupedByParent.Key == null)
                        continue;

                    var ids = questionsGroupedByParent.Select(p => p.id?.ToString() ?? string.Empty).Distinct().ToArray();
                    this.webInterviewInvoker.RefreshEntities(questionsGroupedByParent.Key, ids);
                }
                this.webInterviewInvoker.RefreshSectionState(interviewId);
            }
        }

        public void ReloadInterview(Guid interviewId) => this.webInterviewInvoker.ReloadInterview(interviewId);
        public void FinishInterview(Guid interviewId) => this.webInterviewInvoker.FinishInterview(interviewId);

        public void MarkAnswerAsNotSaved(Guid interviewId, Identity questionId, string errorMessage)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId.FormatGuid());

            if (interview == null)
            {
                return;
            }
            
            var clientGroupIdentity = this.GetClientGroupIdentity(questionId, interview);

            if (clientGroupIdentity != null)
                this.webInterviewInvoker.MarkAnswerAsNotSaved(clientGroupIdentity, questionId.ToString(), errorMessage);
        }

        public void MarkAnswerAsNotSaved(Guid interviewId, Identity questionId, Exception exception)
        {
            var errorMessage = WebInterview.GetUiMessageFromException(exception);
            MarkAnswerAsNotSaved(interviewId, questionId, errorMessage);
        }

        public virtual void RefreshRemovedEntities(Guid interviewId, params Identity[] entities)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId.FormatGuid());

            if (interview == null)
            {
                return;
            }

            var questionnaire = this.questionnaireStorage.GetQuestionnaireDocument(interview.QuestionnaireIdentity);
            if (questionnaire == null)
                return;
            
            var entitiesToRefresh = new List<(string section, Identity identity)>();

            foreach (var entity in entities)
            {
                var composite = questionnaire.Find<IComposite>(entity.Id);
                var parent = composite.GetParent();
                var entityRosterVector = entity.RosterVector;
                var childIdentity = entity;

                while (parent != null && parent.PublicKey != interview.QuestionnaireIdentity.QuestionnaireId)
                {
                    entityRosterVector = entityRosterVector.Shrink(entity.RosterVector.Length - 1);
                    var parentIdentity = new Identity(parent.PublicKey, entityRosterVector);

                    if (composite is IGroup group && group.DisplayMode == RosterDisplayMode.Table)
                    {
                        var tableClientRosterIdentity = new Identity(composite.PublicKey, entityRosterVector);
                        entitiesToRefresh.Add((WebInterview.GetConnectedClientSectionKey(parentIdentity, interview.Id), tableClientRosterIdentity));
                    }

                    var parentIdentityAsString = WebInterview.GetConnectedClientSectionKey(parentIdentity, interview.Id);
                    entitiesToRefresh.Add((parentIdentityAsString, childIdentity));

                    childIdentity = parentIdentity;
                    composite = parent;
                    parent = parent.GetParent();
                }
            }

            foreach (var questionsGroupedByParent in entitiesToRefresh.GroupBy(x => x.section))
            {
                if (questionsGroupedByParent.Key == null)
                    continue;

                this.webInterviewInvoker.RefreshEntities(
                    questionsGroupedByParent.Key, 
                    questionsGroupedByParent.Select(p => p.identity.ToString()).Distinct().ToArray());
            }

            this.webInterviewInvoker.RefreshSection(interviewId);
        }

        private string GetClientGroupIdentity(Identity identity, IStatefulInterview interview)
        {
            if(this.IsQuestionPrefield(identity, interview))
            {
                return WebInterview.GetConnectedClientPrefilledSectionKey(interview.Id);
            }

            var parentIdentity = this.GetParentIdentity(identity, interview);

            if (parentIdentity == null) return null;

            return WebInterview.GetConnectedClientSectionKey(parentIdentity, interview.Id);
        }

        private Identity GetParentIdentity(Identity identity, IStatefulInterview interview)
        {
            return (interview.GetQuestion(identity)
                ?? interview.GetStaticText(identity)
                ?? interview.GetRoster(identity)
                ?? interview.GetVariable(identity)
                ?? (IInterviewTreeNode) interview.GetGroup(identity))?.Parent?.Identity;
        }

        private bool IsQuestionPrefield(Identity identity, IStatefulInterview interview)
        {
            return interview.GetQuestion(identity)?.IsPrefilled ?? false;
        }

        private bool IsSupportFilterOptionCondition(IComposite documentEntity)
            => !string.IsNullOrWhiteSpace((documentEntity as IQuestion)?.Properties.OptionsFilterExpression);
    }
}
