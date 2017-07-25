using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.SignalR;
using Ninject;
using WB.Core.BoundedContexts.Headquarters.Services.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.UI.Headquarters.API.WebInterview.Services
{
    public class WebInterviewNotificationService : IWebInterviewNotificationService
    {
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IHubContext webInterviewHubContext;

        public WebInterviewNotificationService(IStatefulInterviewRepository statefulInterviewRepository,
            IQuestionnaireStorage questionnaireStorage,
            [Named("WebInterview")] IHubContext webInterviewHubContext)
        {
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.webInterviewHubContext = webInterviewHubContext;
            this.questionnaireStorage = questionnaireStorage;
        }

        public virtual void RefreshEntities(Guid interviewId, params Identity[] questions)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId.FormatGuid());

            if (interview == null)
            {
                return;
            }

            var entitiesToRefresh = new List<Tuple<string, Identity>>();

            foreach (var identity in questions)
            {
                if (this.IsQuestionPrefield(identity, interview))
                {
                    entitiesToRefresh.Add(Tuple.Create(WebInterview.GetConnectedClientPrefilledSectionKey(interview.Id.FormatGuid()), identity));
                }
                else
                {
                    var curreentEntity = identity;
                    while (curreentEntity != null)
                    {
                        var parent = this.GetParentIdentity(curreentEntity, interview);
                        if (parent != null)
                        {
                            entitiesToRefresh.Add(Tuple.Create(WebInterview.GetConnectedClientSectionKey(parent.ToString(), interview.Id.FormatGuid()), curreentEntity));
                        }
                        curreentEntity = parent;
                    }
                }
            }

            foreach (var questionsGroupedByParent in entitiesToRefresh.GroupBy(x => x.Item1))
            {
                if (questionsGroupedByParent.Key == null)
                    continue;

                var group = this.webInterviewHubContext.Clients.Group(questionsGroupedByParent.Key);

                group.refreshEntities(questionsGroupedByParent.Select(p => p.Item2.ToString()).Distinct().ToArray());
            }

            this.webInterviewHubContext.Clients.Group(interviewId.FormatGuid()).refreshSection();
        }

        public void ReloadInterview(Guid interviewId)
            => this.webInterviewHubContext.Clients.Group(interviewId.FormatGuid()).reloadInterview();

        public void FinishInterview(Guid interviewId)
            => this.webInterviewHubContext.Clients.Group(interviewId.FormatGuid()).finishInterview();

        public void MarkAnswerAsNotSaved(string interviewId, string questionId, string errorMessage)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId);

            if (interview == null)
            {
                return;
            }
            
            var questionIdentity = Identity.Parse(questionId);

            var clientGroupIdentity = this.GetClientGroupIdentity(questionIdentity, interview);

            this.webInterviewHubContext.Clients.Group(clientGroupIdentity).markAnswerAsNotSaved(questionId, errorMessage);
        }

        public virtual void RefreshRemovedEntities(Guid interviewId, params Identity[] entities)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId.FormatGuid());

            if (interview == null)
            {
                return;
            }

            var questionnarie = this.questionnaireStorage.GetQuestionnaireDocument(interview.QuestionnaireIdentity);
            if (questionnarie != null)
            {
                var entitiesToRefresh = new List<Tuple<string, Identity>>();

                foreach (var entity in entities)
                {
                    var parent = questionnarie.GetParentById(entity.Id);
                    var parentVector = entity.RosterVector;
                    var childIdentity = entity;

                    while (parent != null && parent.PublicKey != interview.QuestionnaireIdentity.QuestionnaireId)
                    {
                        parentVector = parentVector.Shrink(entity.RosterVector.Length - 1);
                        var parentIdentity = new Identity(parent.PublicKey, parentVector);

                        var parentIdentityAsString = WebInterview.GetConnectedClientSectionKey(
                            parentIdentity.ToString(), interview.Id.FormatGuid());
                        entitiesToRefresh.Add(new Tuple<string, Identity>(parentIdentityAsString, childIdentity));

                        childIdentity = parentIdentity;
                        parent = questionnarie.GetParentById(parent.PublicKey);
                    }
                }

                foreach (var questionsGroupedByParent in entitiesToRefresh.GroupBy(x => x.Item1))
                {
                    if (questionsGroupedByParent.Key == null)
                        continue;

                    var clients = this.webInterviewHubContext.Clients;
                    var group = clients.Group(questionsGroupedByParent.Key);

                    group.refreshEntities(questionsGroupedByParent.Select(p => p.Item2.ToString()).ToArray());
                }

                this.webInterviewHubContext.Clients.Group(interviewId.FormatGuid()).refreshSection();
            }
        }

        private string GetClientGroupIdentity(Identity identity, IStatefulInterview interview)
        {
            return this.IsQuestionPrefield(identity, interview)
                ? WebInterview.GetConnectedClientPrefilledSectionKey(interview.Id.FormatGuid())
                : WebInterview.GetConnectedClientSectionKey(this.GetParentIdentity(identity, interview).ToString(),
                    interview.Id.FormatGuid());
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

        public void RefreshComment(Guid interviewId, Identity question)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId.FormatGuid());

            if (interview == null)
            {
                return;
            }

            var clientQuestionIdentity = this.GetClientGroupIdentity(question, interview);

            this.webInterviewHubContext.Clients.Group(clientQuestionIdentity).refreshComment(question.ToString());
        }
    }
}