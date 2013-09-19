using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.AbstractFactories;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace Core.Supervisor.Views.Interview
{
    using Main.Core.Documents;
    using Main.Core.View;
    using WB.Core.BoundedContexts.Supervisor.Views.Interview;
    using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

    public class InterviewDetailsViewFactory : IViewFactory<InterviewDetailsInputModel, InterviewDetailsView>
    {
        private readonly IReadSideRepositoryReader<InterviewData> interviewStore;
        private readonly IReadSideRepositoryReader<UserDocument> userStore;
        private readonly IVersionedReadSideRepositoryReader<QuestionnaireDocumentVersioned> questionnarieStore;
        private readonly IVersionedReadSideRepositoryReader<QuestionnairePropagationStructure> questionnriePropagationStructures;

        public InterviewDetailsViewFactory(IReadSideRepositoryReader<InterviewData> interviewStore,
            IReadSideRepositoryReader<UserDocument> userStore,
            IVersionedReadSideRepositoryReader<QuestionnaireDocumentVersioned> questionnarieStore,
            IVersionedReadSideRepositoryReader<QuestionnairePropagationStructure> questionnriePropagationStructures)
        {
            this.interviewStore = interviewStore;
            this.userStore = userStore;
            this.questionnarieStore = questionnarieStore;
            this.questionnriePropagationStructures = questionnriePropagationStructures;
        }

        public InterviewDetailsView Load(InterviewDetailsInputModel input)
        {
            var interview = this.interviewStore.GetById(input.CompleteQuestionnaireId);
            if (interview == null || interview.IsDeleted)
                return null;

            var questionnarie = this.questionnarieStore.GetById(interview.QuestionnaireId, interview.QuestionnaireVersion);
            if (questionnarie == null)
                throw new ArgumentException(string.Format(
                    "Questionnaire with id {0} and version {1} is missing.", interview.QuestionnaireId, interview.QuestionnaireVersion)) 

            var variablesMap = questionnarie.Questionnaire.GetAllQuestions().Select(x => new
            {
                Id = x.PublicKey,
                Variable = x.StataExportCaption
            }).ToDictionary(x => x.Id, x => x.Variable);


            if (!input.CurrentGroupPublicKey.HasValue)
            {
                input.CurrentGroupPublicKey = interview.InterviewId;
            }

            var user = this.userStore.GetById(interview.ResponsibleId);
            if (user == null)
                throw new ArgumentException(string.Format("User with id {0} is not found.", interview.ResponsibleId));

            var questionnairePropagation = questionnriePropagationStructures.GetById(interview.QuestionnaireId, interview.QuestionnaireVersion);

            var interviewDetails = new InterviewDetailsView()
                {
                    User = input.User,
                    Responsible = new UserLight(interview.ResponsibleId, user.UserName),
                    QuestionnairePublicKey = interview.QuestionnaireId,
                    Title = questionnarie.Questionnaire.Title,
                    Description = questionnarie.Questionnaire.Description,
                    PublicKey = interview.InterviewId,
                    Status = interview.Status
                };

            var groupStack = new Stack<KeyValuePair<IGroup, int>>();

            groupStack.Push(new KeyValuePair<IGroup, int>(questionnarie.Questionnaire, 0));
            while (groupStack.Count > 0)
            {
                var currentGroup = groupStack.Pop();

                if (currentGroup.Key.Propagated == Propagate.AutoPropagated)
                {
                    var propagatedGroups = GetPropagatedLevels(currentGroup.Key.PublicKey, interview, questionnairePropagation).ToList();

                    if (propagatedGroups.Any())
                    {
                        //we have at least one completed propagated group
                        //so for every layer we are creating propagated group
                        foreach (var propagatedGroup in propagatedGroups)
                        {
                            var completedPropGroup = this.GetCompletedGroup(currentGroup.Key, currentGroup.Value, propagatedGroup.Value, variablesMap);

                            interviewDetails.Groups.Add(completedPropGroup);
                        }
                    }
                }
                else
                {
                    var rootLevel = interview.Levels.FirstOrDefault(w => w.Value.ScopeId == interview.InterviewId).Value;


                    interviewDetails.Groups.Add(this.GetCompletedGroup(currentGroup.Key, currentGroup.Value, rootLevel, variablesMap));

                    foreach (var group in currentGroup.Key.Children.OfType<IGroup>().Reverse())
                    {
                        group.SetParent(currentGroup.Key);
                        groupStack.Push(new KeyValuePair<IGroup, int>(group, currentGroup.Value + 1));
                    }
                }
            }

            return interviewDetails;
        }

        private IEnumerable<KeyValuePair<string, InterviewLevel>> GetPropagatedLevels(Guid groupId, InterviewData interviewData, QuestionnairePropagationStructure questionnriePropagation)
        {
            Guid? propagationScope = null;
            //totally not efficient
            foreach (var scopeId in questionnriePropagation.PropagationScopes.Keys)
            {
                foreach (var trigger in questionnriePropagation.PropagationScopes[scopeId])
                {
                    if (trigger == groupId)
                    {
                        propagationScope = scopeId;
                        break;
                    }
                }
            }
            if (!propagationScope.HasValue)
                throw new ArgumentException(string.Format("group {0} is missing in any propagation scope of questionnarie", groupId));


            return interviewData.Levels.Where(w => w.Value.ScopeId == propagationScope);
        }


        private InterviewGroupView GetCompletedGroup(IGroup currentGroup, int depth, InterviewLevel interviewLevel, Dictionary<Guid, string> variablesMap)
        {
            var completedGroup = new InterviewGroupView(currentGroup.PublicKey)
            {
                Depth = depth,
                Title = currentGroup.Title,
                PropagationVector = interviewLevel.PropagationVector,
                ParentId = currentGroup.GetParent() != null ? currentGroup.GetParent().PublicKey : (Guid?) null
            };

            foreach (var question in currentGroup.Children.OfType<IQuestion>())
            {
                InterviewQuestion answeredQuestion = interviewLevel.Questions.FirstOrDefault(q => q.Id == question.PublicKey);

                var interviewQuestion = new InterviewQuestionView(question, answeredQuestion, variablesMap);

                completedGroup.Questions.Add(interviewQuestion);
            }

            return completedGroup;
        }

    }
}
