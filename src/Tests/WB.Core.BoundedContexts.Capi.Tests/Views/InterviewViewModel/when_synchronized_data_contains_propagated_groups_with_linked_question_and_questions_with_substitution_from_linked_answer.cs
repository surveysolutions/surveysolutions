using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Capi.Tests.Views.InterviewViewModel
{
    internal class when_synchronized_data_contains_propagated_groups_with_linked_question_and_questions_with_substitution_from_linked_answer
    {
        private Establish context = () =>
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);

            propagatedGroupId = Guid.Parse("10000000000000000000000000000000");
            sourceForLinkedQuestionId = Guid.Parse("11111111111111111111111111111111");
            linkedQuestionId = Guid.Parse("22222222222222222222222222222222");
            autoPropagatedQuestionId = Guid.Parse("33333333333333333333333333333333");

            questionnarie=new QuestionnaireDocument();
            var chapter = new Group("chapter");
            var autopropagatedQuestion = new AutoPropagateQuestion()
            {
                PublicKey = autoPropagatedQuestionId,
                QuestionType = QuestionType.AutoPropagate,
                Triggers = new List<Guid> { propagatedGroupId }
            };
            chapter.Children.Add(autopropagatedQuestion);

            var roster = new Group() { PublicKey = propagatedGroupId, Propagated = Propagate.AutoPropagated };
            var sourceForLinkedQuestion = new NumericQuestion()
            {
                PublicKey = sourceForLinkedQuestionId
            };
            roster.Children.Add(sourceForLinkedQuestion);
            var linkedQuestion = new SingleQuestion()
            {
                PublicKey = linkedQuestionId,
                LinkedToQuestionId = sourceForLinkedQuestionId,
                StataExportCaption = sourceForLinkedQuestionVariableName
            };
            roster.Children.Add(linkedQuestion);

            var questionWhichSubstitutesLinkedQuestionAnswer =
                new TextQuestion(string.Format("subst %{0}%", sourceForLinkedQuestionVariableName)) { PublicKey = Guid.NewGuid() };
            roster.Children.Add(questionWhichSubstitutesLinkedQuestionAnswer);

            chapter.Children.Add(roster);
            questionnarie.Children.Add(chapter);

            propagationStructure = new QuestionnairePropagationStructure(questionnarie, 1);
            interviewSynchronizationDto = new InterviewSynchronizationDto(id: Guid.NewGuid(), status: InterviewStatus.InterviewerAssigned,
                userId: Guid.NewGuid(), questionnaireId: questionnarie.PublicKey, questionnaireVersion: 1,
                answers: new AnsweredQuestionSynchronizationDto[]
                {
                    new AnsweredQuestionSynchronizationDto(sourceForLinkedQuestionId,new int[]{0},1,string.Empty ),
                    new AnsweredQuestionSynchronizationDto(sourceForLinkedQuestionId,new int[]{1},2,string.Empty )
                },
                disabledGroups: new HashSet<InterviewItemId>(),
                disabledQuestions: new HashSet<InterviewItemId>(), validAnsweredQuestions: new HashSet<InterviewItemId>(),
                invalidAnsweredQuestions: new HashSet<InterviewItemId>(),
                propagatedGroupInstanceCounts: new Dictionary<InterviewItemId, int>()
                {
                    {new InterviewItemId(propagatedGroupId, new int[0]),2 }
                }, 
                wasCompleted: false);
        };

        private Because of = () =>
            interviewViewModel =
                new Capi.Views.InterviewDetails.InterviewViewModel(interviewSynchronizationDto.Id, questionnarie, propagationStructure,
                    interviewSynchronizationDto);

        private It should_chapters_count_equals_1 = () =>
            interviewViewModel.Chapters.Count.ShouldEqual(1);

        private It should_roster_size_equals_2 = () =>
            interviewViewModel.Screens.Values.Count(s => s.ScreenId.Id == propagatedGroupId && !s.ScreenId.IsTopLevel()).ShouldEqual(2);

        private It should_answer_on_first_question_in_first_row_of_roster_equals_to_1 = () =>
            interviewViewModel.FindQuestion(
                question => question.PublicKey == new InterviewItemId(sourceForLinkedQuestionId, new int[] { 0 }))
                .FirstOrDefault()
                .AnswerObject.ShouldEqual(1);

        private It should_answer_on_first_question_in_second_row_of_roster_equals_to_2 = () =>
            interviewViewModel.FindQuestion(
                question => question.PublicKey == new InterviewItemId(sourceForLinkedQuestionId, new int[] { 1 }))
                .FirstOrDefault()
                .AnswerObject.ShouldEqual(2);

        private static WB.Core.BoundedContexts.Capi.Views.InterviewDetails.InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnarie;
        private static QuestionnairePropagationStructure propagationStructure;
        private static InterviewSynchronizationDto interviewSynchronizationDto;
        private static Guid propagatedGroupId;
        private static Guid autoPropagatedQuestionId;
        private static Guid sourceForLinkedQuestionId;
        private const string sourceForLinkedQuestionVariableName = "var";
        private static Guid linkedQuestionId;
        
    }
}
