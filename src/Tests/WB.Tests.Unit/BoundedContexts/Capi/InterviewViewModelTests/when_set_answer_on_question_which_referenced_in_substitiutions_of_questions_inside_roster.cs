using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Tests.Unit.BoundedContexts.Capi.InterviewViewModelTests
{
    internal class when_set_answer_on_question_which_referenced_in_substitiutions_of_questions_inside_roster : InterviewViewModelTestContext
    {
        Establish context = () =>
        {
            rosterId = Guid.Parse("10000000000000000000000000000000");

            var rosterSizeQuestionId = Guid.Parse("33333333333333333333333333333333");
            nestedGroupId = Guid.Parse("22222222222222222222222222222222");
            questionInNesedGroupWithSubstitutionId = Guid.Parse("20000000000000000000000000000000");
            questionSourceOfSubstitutionId = Guid.Parse("30000000000000000000000000000000");
            nestedGroupTitle = "nested Group";
            questionnarie = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion()
                {
                    PublicKey = rosterSizeQuestionId,
                    QuestionType = QuestionType.Numeric
                },
                new TextQuestion()
                {
                    PublicKey = questionSourceOfSubstitutionId,
                    StataExportCaption = "sub_sorce",
                    QuestionType = QuestionType.Text
                },
                new Group()
                {
                    PublicKey = rosterId,
                    IsRoster = true,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>
                    {
                        new Group(nestedGroupTitle)
                        {
                            PublicKey = nestedGroupId,
                            Children =
                                new List<IComposite>
                                {
                                    new NumericQuestion()
                                    {
                                        PublicKey = questionInNesedGroupWithSubstitutionId,
                                        StataExportCaption = "sub",
                                        QuestionText = "%sub_sorce% example",
                                        QuestionType = QuestionType.Numeric
                                    }
                                }
                        }
                    }
                });

            rosterStructure = CreateQuestionnaireRosterStructure(questionnarie);

            interviewSynchronizationDto = CreateInterviewSynchronizationDto(
               answers: new AnsweredQuestionSynchronizationDto[0],
               propagatedGroupInstanceCounts: new Dictionary<InterviewItemId, RosterSynchronizationDto[]>());

            interviewViewModel = CreateInterviewViewModel(questionnarie, rosterStructure,
             interviewSynchronizationDto);
            PropagateScreen(interviewViewModel, rosterId, 0);
            PropagateScreen(interviewViewModel, rosterId, 1);
        };

        Because of = () =>
            interviewViewModel.SetAnswer(ConversionHelper.ConvertIdAndRosterVectorToString(questionSourceOfSubstitutionId, new decimal[0]), "answer");

        It should_title_of_question_with_substitution_in_first_row_be_substituted_with_answer_on_set_question = () =>
            interviewViewModel.FindQuestion(q => q.PublicKey == Create.InterviewItemId(questionInNesedGroupWithSubstitutionId, new decimal[] { 0 })).FirstOrDefault().Text.ShouldEqual("answer example");

        It should_title_of_question_with_substitution_in_second_row_be_substituted_with_answer_on_set_question = () =>
         interviewViewModel.FindQuestion(q => q.PublicKey == Create.InterviewItemId(questionInNesedGroupWithSubstitutionId, new decimal[] { 1 })).FirstOrDefault().Text.ShouldEqual("answer example");

        private static InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure rosterStructure;
        private static InterviewSynchronizationDto interviewSynchronizationDto;

        private static Guid rosterId;
        private static Guid nestedGroupId;
        private static Guid questionInNesedGroupWithSubstitutionId;
        private static Guid questionSourceOfSubstitutionId;
        private static string nestedGroupTitle;
    }
}
