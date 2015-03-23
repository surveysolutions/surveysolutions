using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Tests.Unit.BoundedContexts.Capi.InterviewViewModelTests
{
    internal class when_creating_interview_with_answered_supervisor_questions_in_fixed_roster : InterviewViewModelTestContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new Group("Fixed Roster")
                {
                    PublicKey = fixedRosterId,
                    IsRoster = true,
                    RosterFixedTitles = new[] { "1", "2", "3" },
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    Children = new List<IComposite>
                    {
                        new TextQuestion
                        {
                            PublicKey = answeredQuestionId,
                            QuestionType = QuestionType.Text,
                            QuestionScope = QuestionScope.Supervisor
                        }
                    }
                },
                new SingleQuestion
                {
                    PublicKey = linkedQuestionId,
                    QuestionType = QuestionType.SingleOption,
                    LinkedToQuestionId = answeredQuestionId
                });

            questionnaire.Title = testTemplate;

            var scopeVector = new ValueVector<Guid>(new[] { fixedRosterId });
            rosterStructure = new QuestionnaireRosterStructure
            {
                QuestionnaireId = questionnaire.PublicKey,
                Version = 1,
                RosterScopes = new Dictionary<ValueVector<Guid>, RosterScopeDescription>
                {
                    { 
                        scopeVector, 
                        new RosterScopeDescription(scopeVector: scopeVector, 
                            scopeName:"", 
                            scopeType: RosterScopeType.Fixed, 
                            rosterIdToRosterTitleQuestionIdMap: new Dictionary<Guid, RosterTitleQuestionDescription>()
                            )
                    }
                }
            };

            answer1 = new AnsweredQuestionSynchronizationDto(answeredQuestionId, new decimal[] { 0 }, "Answer 1", string.Empty);
            answer2 = new AnsweredQuestionSynchronizationDto(answeredQuestionId, new decimal[] { 1 }, "Answer 2", string.Empty);
            answer3 = new AnsweredQuestionSynchronizationDto(answeredQuestionId, new decimal[] { 2 }, "Answer 3", string.Empty);

            interviewSynchronizationDto = CreateInterviewSynchronizationDto(
                answers: new[] { answer1, answer2, answer3 },
                propagatedGroupInstanceCounts: new Dictionary<InterviewItemId, RosterSynchronizationDto[]>());
        };

        Because of = () =>
            interviewViewModel = CreateInterviewViewModel(questionnaire, rosterStructure,
                interviewSynchronizationDto);

        It should_find_one_supervisor_question_in_template = () =>
            interviewViewModel.SuperviorQuestionIds.Count.ShouldEqual(1);

        It should_contains_answeredQuestionId_in_supervisors_questions_keys_collection = () =>
            interviewViewModel.SuperviorQuestionIds.Keys.ShouldContainOnly(answeredQuestionId);

        It should_set_fixedRosterId_as_roster_scope_for_finded_supervisors_question = () =>
           interviewViewModel.SuperviorQuestionIds[answeredQuestionId].ShouldContainOnly(fixedRosterId);

        It should_find_3_supervisor_answers = () =>
            interviewViewModel.SuperviorQuestions.Count.ShouldEqual(3);

    /*    It should_collect_supervisor_answers_with_answeredQuestionId_in_keys = () =>
            interviewViewModel.SuperviorQuestions.Keys.Select(x => x.Id).Distinct().ShouldContainOnly(answeredQuestionId);*/

        It should_set_propagation_vector_for_the_first_item_equals_0 = () =>
            interviewViewModel.SuperviorQuestions.Keys.ElementAt(0).ShouldEqual(answeredQuestionId.FormatGuid()+"[0]");

        It should_set_propagation_vector_for_the_second_item_equals_1 = () =>
            interviewViewModel.SuperviorQuestions.Keys.ElementAt(1).ShouldEqual(answeredQuestionId.FormatGuid()+"[1]");

        It should_set_propagation_vector_for_the_third_item_equals_2 = () =>
           interviewViewModel.SuperviorQuestions.Keys.ElementAt(2).ShouldEqual(answeredQuestionId.FormatGuid() + "[2]");

        It should_set_answe1_for_the_1st_item_in_supervisor_answers_collection = () =>
          interviewViewModel.SuperviorQuestions.Values.ElementAt(0).ShouldEqual(answer1);

        It should_set_answe2_for_the_1st_item_in_supervisor_answers_collection = () =>
          interviewViewModel.SuperviorQuestions.Values.ElementAt(1).ShouldEqual(answer2);

        It should_set_answe3_for_the_1st_item_in_supervisor_answers_collection = () =>
          interviewViewModel.SuperviorQuestions.Values.ElementAt(2).ShouldEqual(answer3);

        It should_linked_question_has_3_options = () =>
            GetLinkedQuestionById(linkedQuestionId).AnswerOptions.Count().ShouldEqual(3);

        It should_set_first_option_of_linked_question_equal_to__Answer_2__ = () =>
            GetLinkedQuestionById(linkedQuestionId).AnswerOptions.ElementAt(0).Title.ShouldEqual("Answer 1");

        It should_set_second_option_of_linked_question_equal_to__Answer_2__ = () =>
            GetLinkedQuestionById(linkedQuestionId).AnswerOptions.ElementAt(1).Title.ShouldEqual("Answer 2");

        It should_set_third_option_of_linked_question_equal_to__Answer_3__ = () =>
            GetLinkedQuestionById(linkedQuestionId).AnswerOptions.ElementAt(2).Title.ShouldEqual("Answer 3");

        private static LinkedQuestionViewModel GetLinkedQuestionById(Guid linkedQuestionId)
        {
            return ((LinkedQuestionViewModel)interviewViewModel.FindQuestion(
                question => question.PublicKey == new InterviewItemId(linkedQuestionId, new decimal[] { }))
                .FirstOrDefault());
        }

        private static InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireRosterStructure rosterStructure;
        private static InterviewSynchronizationDto interviewSynchronizationDto;
        private static Guid answeredQuestionId = Guid.Parse("33333333333333333333333333333333");
        private static Guid fixedRosterId = Guid.Parse("11111111111111111111111111111111");
        private static Guid linkedQuestionId = Guid.Parse("22222222222222222222222222222222");
        private const string testTemplate = "test template";
        private const int answerForNumeric = 15;
        private static AnsweredQuestionSynchronizationDto answer1 = null;
        private static AnsweredQuestionSynchronizationDto answer2 = null;
        private static AnsweredQuestionSynchronizationDto answer3 = null;
    }
}