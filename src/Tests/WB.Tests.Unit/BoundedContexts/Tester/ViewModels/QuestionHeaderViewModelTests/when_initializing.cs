using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Tester.Implementation.Aggregates;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.Tester.Repositories;
using WB.Core.BoundedContexts.Tester.ViewModels.Questions.State;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.QuestionHeaderViewModelTests
{
    public class when_initializing : QuestionHeaderViewModelTestsContext
    {
        Establish context = () =>
        {
            substitutedQuesiton = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            substitutionTargetId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var maskedTextQuestionModel = new TextQuestionModel
            {
                Title = "title with %subst%",
                Id = substitutionTargetId
            };
            QuestionnaireModel questionnaireModel = new QuestionnaireModel
            {
                Questions = new Dictionary<Guid, BaseQuestionModel>
                {
                    { substitutionTargetId, maskedTextQuestionModel }
                },
                QuestionsByVariableNames = new Dictionary<string, BaseQuestionModel>
                {
                    {
                        "blah", maskedTextQuestionModel
                    },
                    {
                        "subst", new TextQuestionModel
                        {
                            Variable = "subst",
                            Id = substitutedQuesiton
                        }
                    },
                }
            };

            var questionnaireRepository = new Mock<IPlainKeyValueStorage<QuestionnaireModel>>();
            questionnaireRepository.SetReturnsDefault(questionnaireModel);

            var answer = new TextAnswer();
            answer.SetAnswer("answer");
            var interview = Mock.Of<IStatefulInterview>(x => x.FindBaseAnswerByOrDeeperRosterLevel(substitutedQuesiton, Empty.RosterVector) == answer);

            var interviewRepository = new Mock<IStatefulInterviewRepository>();
            interviewRepository.SetReturnsDefault(interview);
            viewModel = CreateViewModel(questionnaireRepository.Object, interviewRepository.Object);
        };

        Because of = () => viewModel.Init("interview", new Identity(substitutionTargetId, Empty.RosterVector));

        It should_substitute_question_titles = () => viewModel.Title.ShouldEqual("title with answer");

        static QuestionHeaderViewModel viewModel;
        private static Guid substitutionTargetId;
        private static Guid substitutedQuesiton;
    }
}

