using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;
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
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IHubContext webInterviewHubContext;
        private readonly IQuestionnaireStorage questionnaireStorage;

        public WebInterviewNotificationService(IStatefulInterviewRepository statefulInterviewRepository,
            IQuestionnaireStorage questionnaireStorage,
            [Ninject.Named("WebInterview")] IHubContext webInterviewHubContext)
        {
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.webInterviewHubContext = webInterviewHubContext;
            this.questionnaireStorage = questionnaireStorage;
        }

        public void RefreshEntities(Guid interviewId, params Identity[] entities)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId.FormatGuid());

            var entitiesToRefresh = new List<Tuple<string, Identity>>();

            foreach (var identity in entities)
            {
                var parent = this.CheckAndGetParentIdentity(identity, interview);
                entitiesToRefresh.Add(Tuple.Create(WebInterview.GetConnectedClientSectionKey(parent?.ToString(), interview.Id.FormatGuid()), identity));

                if (parent != null)
                {
                    while (parent != null)
                    {
                        var newEntity = parent;
                        parent = this.CheckAndGetParentIdentity(newEntity, interview);
                        if (parent != null)
                            entitiesToRefresh.Add(
                                Tuple.Create(WebInterview.GetConnectedClientSectionKey(parent?.ToString(),
                                        interview.Id.FormatGuid()), newEntity));
                    }
                }
            }

            foreach (var questionsGroupedByParent in entitiesToRefresh.GroupBy(x => x.Item1))
            {
                if (questionsGroupedByParent.Key == null)
                {
                    continue;
                }

                var clients = this.webInterviewHubContext.Clients;
                var group = clients.Group(questionsGroupedByParent.Key);

                group.refreshEntities(questionsGroupedByParent.Select(p => p.Item2.ToString()).Distinct().ToArray());
            }

            this.webInterviewHubContext.Clients.Group(interviewId.FormatGuid()).refreshSection();
        }

        public void ReloadInterview(Guid interviewId) => this.webInterviewHubContext.Clients.Group(interviewId.FormatGuid()).reloadInterview();
        public void FinishInterview(Guid interviewId) => this.webInterviewHubContext.Clients.Group(interviewId.FormatGuid()).finishInterview();

        public void MarkAnswerAsNotSaved(string interviewId, string questionId, string errorMessage)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId);
            var questionIdentity = Identity.Parse(questionId);

            var clientGroupIdentity = this.GetClientGroupIdentity(questionIdentity, interview);

            this.webInterviewHubContext.Clients.Group(clientGroupIdentity).markAnswerAsNotSaved(questionId, errorMessage);
        }

        public void RefreshRemovedEntities(Guid interviewId, params Identity[] entities)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId.FormatGuid());
            var questionnarie = this.questionnaireStorage.GetQuestionnaireDocument(interview.QuestionnaireIdentity);
            if (questionnarie != null)
            {
                List<Tuple<string,Identity>> entitiesToRefresh = new List<Tuple<string, Identity>>();

                foreach (var entity in entities)
                {
                    var parent = questionnarie.GetParentById(entity.Id);
                    var parentVector = entity.RosterVector;
                    var childIdentity = entity;

                    while (parent!= null && parent.PublicKey != interview.QuestionnaireIdentity.QuestionnaireId)
                    {
                        parentVector = parentVector.Shrink(entity.RosterVector.Length - 1);
                        var parentIdentity = new Identity(parent.PublicKey, parentVector);

                        string parentIdentityAsString = WebInterview.GetConnectedClientSectionKey(parentIdentity.ToString(), interview.Id.FormatGuid());
                        entitiesToRefresh.Add(new Tuple<string, Identity>(parentIdentityAsString, childIdentity));

                        childIdentity = parentIdentity;
                        parent = questionnarie.GetParentById(parent.PublicKey);
                    }
                }

                foreach (var questionsGroupedByParent in entitiesToRefresh.GroupBy(x => x.Item1))
                {
                    if (questionsGroupedByParent.Key == null)
                    {
                        continue;
                    }

                    var clients = this.webInterviewHubContext.Clients;
                    var group = clients.Group(questionsGroupedByParent.Key);

                    group.refreshEntities(questionsGroupedByParent.Select(p => p.Item2.ToString()).ToArray());
                }

                this.webInterviewHubContext.Clients.Group(interviewId.FormatGuid()).refreshSection();
            }
        }

        private Identity CheckAndGetParentIdentity(Identity identity, IStatefulInterview interview)
        {
            Identity sectionKey;

            if (interview.GetQuestion(identity)?.IsPrefilled ?? false)
            {
                sectionKey = null;
            }
            else
            {
                sectionKey = ((IInterviewTreeNode)interview.GetQuestion(identity)
                    ?? (IInterviewTreeNode)interview.GetStaticText(identity)
                    ?? (IInterviewTreeNode)interview.GetRoster(identity)
                    ?? (IInterviewTreeNode)interview.GetGroup(identity))?.Parent?.Identity;
            }

            return sectionKey;
        }

        private string GetClientGroupIdentity(Identity identity, IStatefulInterview interview)
        {
            var sectionKey = CheckAndGetParentIdentity(identity, interview)?.ToString();
            return WebInterview.GetConnectedClientSectionKey(sectionKey, interview.Id.FormatGuid());
        }
        
    }
}