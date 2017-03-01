using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using NSubstitute;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewQuestionViewTests
{
    public class when_creating_view_for_question_with_multiple_failed_validation_conditions : InterviewEntityViewFactoryTestsContext
    {
        Establish context = () =>
        {
            question = Create.Entity.Question();
            firstFailedCondition = new ValidationCondition {Message = "message 1", Expression = "expresssion 1"};
            secondFailedCondition = new ValidationCondition {Message = "message 3", Expression = "expresssion 3"};
            question.ValidationConditions = new List<ValidationCondition>
            {
                firstFailedCondition,
                new ValidationCondition {Message = "message 2", Expression = "expresssion 2"},
                secondFailedCondition
            };


            answeredQuestion = new InterviewQuestion
            {
                FailedValidationConditions = new List<FailedValidationCondition>
                {
                    new FailedValidationCondition(0),
                    new FailedValidationCondition(2)
                }
            };

            interviewEntityViewFactory = CreateInterviewEntityViewFactory();
        };

        Because of = () =>
            result = interviewEntityViewFactory.BuildInterviewQuestionView(question, answeredQuestion, new Dictionary<string, string>(),
                isParentGroupDisabled: false,
                rosterVector: new decimal[0],
                interviewStatus: InterviewStatus.Completed);

        It should_add_list_of_valied_condtions_to_view = () => result.FailedValidationMessages.Count.ShouldEqual(2);

        It should_add_failed_condtions_according_to_failing_indexes = () =>
        {
            result.FailedValidationMessages.First().Expression.ShouldEqual(firstFailedCondition.Expression);
            result.FailedValidationMessages.First().Message.ShouldEqual(firstFailedCondition.Message);
            result.FailedValidationMessages.Second().Expression.ShouldEqual(secondFailedCondition.Expression);
            result.FailedValidationMessages.Second().Message.ShouldEqual(secondFailedCondition.Message);
        };

        private static IInterviewEntityViewFactory interviewEntityViewFactory;
        static InterviewQuestionView result;
        static IQuestion question;
        static InterviewQuestion answeredQuestion;
        static ValidationCondition firstFailedCondition;
        static ValidationCondition secondFailedCondition;
    }
}