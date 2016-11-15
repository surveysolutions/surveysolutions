using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.InterviewDetailsViewFactoryTests
{
    [Ignore("KP-8159")]
    internal class when_getting_interview_details_and_questionnaire_has_filtered_categorical_question : InterviewDetailsViewFactoryTestsContext
    {
        Establish context = () =>
        {
            var singleOptionQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var multioptionQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            QuestionnaireDocument questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.SingleOptionQuestion(
                    questionId: singleOptionQuestionId,
                    answerCodes: new decimal[] { 1, 2 },
                    optionsFilterExpression: "a"),
                Create.Entity.MultipleOptionsQuestion(
                    questionId: multioptionQuestionId,
                    answers: new [] { 1, 2 },
                    optionsFilterExpression: "a"));

            var interviewDetailsView = new InterviewDetailsView
            {
                Groups = new List<InterviewGroupView>
                {
                    new InterviewGroupView(singleOptionQuestionId)
                    {
                        Entities = new List<InterviewEntityView>
                        {
                            new InterviewQuestionView
                            {
                                QuestionType = QuestionType.SingleOption,
                                Id = singleOptionQuestionId,
                                RosterVector = RosterVector.Empty,
                                Options = new List<QuestionOptionView>
                                {
                                    new QuestionOptionView { Label = "1", Value = 1},
                                    new QuestionOptionView { Label = "2", Value = 2}
                                },
                                IsFilteredCategorical = true
                            },
                            new InterviewQuestionView
                            {
                                QuestionType = QuestionType.MultyOption,
                                Id = multioptionQuestionId,
                                RosterVector = RosterVector.Empty,
                                Options = new List<QuestionOptionView>
                                {
                                    new QuestionOptionView { Label = "1", Value = 1},
                                    new QuestionOptionView { Label = "2", Value = 2}
                                },
                                IsFilteredCategorical = true
                            }
                        }
                    }
                }
            };

            var expressionState = new Mock<ILatestInterviewExpressionState>();
            expressionState
                .Setup(x => x.FilterOptionsForQuestion(Moq.It.IsAny<Identity>(), Moq.It.IsAny<IEnumerable<CategoricalOption>>()))
                .Returns((Identity identity, IEnumerable<CategoricalOption> options) => options.Where(x => x.Value != 1));

            viewfactory = Setup.InterviewDetailsViewFactory(
                interviewId, interviewDetailsView, questionnaire, expressionState.Object);
        };

        Because of = () => view = viewfactory.GetInterviewDetails(interviewId, null, Empty.RosterVector, InterviewDetailsFilter.All);

        It should_filter_out_options_for_categorical_questions = () =>
        {
            var interviewGroupView = view.FilteredGroups.First();
            var questions = interviewGroupView.Entities.Cast<InterviewQuestionView>().ToList();

            questions[0].Options.Single().Value.ShouldEqual(2);
            questions[1].Options.Single().Value.ShouldEqual(2);
        };

        static InterviewDetailsViewFactory viewfactory;
        static DetailsViewModel view;
        static Guid interviewId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}