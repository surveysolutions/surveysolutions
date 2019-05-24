using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

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

            var entitiesToRefresh = new List<(string section, Identity id)>();

            foreach (var identity in questions)
            {
                if (this.IsQuestionPrefield(identity, interview))
                {
                    entitiesToRefresh.Add((WebInterview.GetConnectedClientPrefilledSectionKey(interview.Id), identity));
                }
                
                var currentEntity = identity;
                var questionnaire = questionnaireStorage.GetQuestionnaire(interview.QuestionnaireIdentity, null);

                while (currentEntity != null)
                {
                    var parent = this.GetParentIdentity(currentEntity, interview);
                    if (questionnaire.IsTableRoster(currentEntity.Id))
                    {
                        var tableClientRosterIdentity = new Identity(currentEntity.Id, currentEntity.RosterVector.Shrink());
                        entitiesToRefresh.Add((WebInterview.GetConnectedClientSectionKey(parent, interview.Id), tableClientRosterIdentity));
                    }

                    if (parent != null)
                    {
                        entitiesToRefresh.Add((WebInterview.GetConnectedClientSectionKey(parent, interview.Id), currentEntity));
                    }
                    currentEntity = parent;
                }
            }

            foreach (var questionsGroupedByParent in entitiesToRefresh.GroupBy(x => x.section))
            {
                if (questionsGroupedByParent.Key == null)
                    continue;

                var ids = questionsGroupedByParent.Select(p => p.id.ToString()).Distinct().ToArray();
                this.webInterviewInvoker.RefreshEntities(questionsGroupedByParent.Key, ids);
            }

            this.webInterviewInvoker.RefreshSectionState(interviewId); 
        }

        public void ReloadInterview(Guid interviewId) => this.webInterviewInvoker.ReloadInterview(interviewId);
        public void FinishInterview(Guid interviewId) => this.webInterviewInvoker.FinishInterview(interviewId);

        public void MarkAnswerAsNotSaved(string interviewId, string questionId, string errorMessage)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId);

            if (interview == null)
            {
                return;
            }
            
            var questionIdentity = Identity.Parse(questionId);

            var clientGroupIdentity = this.GetClientGroupIdentity(questionIdentity, interview);

            if (clientGroupIdentity != null)
                this.webInterviewInvoker.MarkAnswerAsNotSaved(clientGroupIdentity, questionId, errorMessage);
                
        }

        public virtual void RefreshRemovedEntities(Guid interviewId, params Identity[] entities)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId.FormatGuid());

            if (interview == null)
            {
                return;
            }

            var questionnaire = this.questionnaireStorage.GetQuestionnaireDocument(interview.QuestionnaireIdentity);
            if (questionnaire != null)
            {
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

                this.webInterviewInvoker.RefreshSectionState(interviewId);
            }
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
                ?? (IInterviewTreeNode) interview.GetGroup(identity))?.Parent?.Identity;
        }

        private bool IsQuestionPrefield(Identity identity, IStatefulInterview interview)
        {
            return interview.GetQuestion(identity)?.IsPrefilled ?? false;
        }

        private bool IsSupportFilterOptionCondition(IComposite documentEntity)
            => !string.IsNullOrWhiteSpace((documentEntity as IQuestion)?.Properties.OptionsFilterExpression);

        public virtual void RefreshEntitiesWithFilteredOptions(Guid interviewId)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId.FormatGuid());

            if (interview == null)
            {
                return;
            }

            var document = this.questionnaireStorage.GetQuestionnaireDocument(interview.QuestionnaireIdentity);

            var entityIds = document.Find<IComposite>(this.IsSupportFilterOptionCondition)
                .Select(e => e.PublicKey).ToHashSet();

            foreach (var entityId in entityIds)
            {
                var identities = interview.GetAllIdentitiesForEntityId(entityId).ToArray();
                this.RefreshEntities(interviewId, identities);
            }
        }

        public virtual void RefreshLinkedToListQuestions(Guid interviewId, Identity[] identities)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId.FormatGuid());

            if (interview == null)
            {
                return;
            }

            var questionnaire = this.questionnaireStorage.GetQuestionnaire(interview.QuestionnaireIdentity,
                interview.Language);

            foreach (var questionIdentity in identities)
            {
                if (interview.GetTextListQuestion(questionIdentity) == null) continue;

                var listQuestionIds = questionnaire.GetLinkedToSourceEntity(questionIdentity.Id).ToArray();
                if (!listQuestionIds.Any())
                    return;

                foreach (var listQuestionId in listQuestionIds)
                {
                    var questionsToRefresh = interview.FindQuestionsFromSameOrDeeperLevel(listQuestionId,
                        questionIdentity);
                    this.RefreshEntities(interviewId, questionsToRefresh.ToArray());
                }
            }
        }

        public virtual void RefreshLinkedToRosterQuestions(Guid interviewId, Identity[] rosterIdentities)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId.FormatGuid());

            if (interview == null)
            {
                return;
            }

            var questionnaire = this.questionnaireStorage.GetQuestionnaire(interview.QuestionnaireIdentity,
                interview.Language);

            var rosterIds = rosterIdentities.Select(x => x.Id).Distinct();

            var linkedToRosterQuestionIds = rosterIds.SelectMany(x => questionnaire.GetLinkedToSourceEntity(x));

            foreach (var linkedToRosterQuestionId in linkedToRosterQuestionIds)
            {
                var identitiesToRefresh = interview.GetAllIdentitiesForEntityId(linkedToRosterQuestionId).ToArray();
                this.RefreshEntities(interviewId, identitiesToRefresh);
            }
        }

        public void ReloadInterviewByQuestionnaire(QuestionnaireIdentity questionnaireIdentity)
            => this.webInterviewInvoker.ReloadInterviews(questionnaireIdentity);

        public void ShutDownInterview(Guid interviewId) => this.webInterviewInvoker.ShutDown(interviewId);
    }
}
