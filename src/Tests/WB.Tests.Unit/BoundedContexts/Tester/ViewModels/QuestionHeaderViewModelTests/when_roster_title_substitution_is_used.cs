using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Moq;
using NSubstitute;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.QuestionHeaderViewModelTests
{
    internal class when_roster_title_substitution_is_used : QuestionHeaderViewModelTestsContext
    {
        Establish context = () =>
        {
            var rosterTitleId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            substitutionTargetQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            QuestionnaireDocument questionnaire = Create.QuestionnaireDocument(children: new List<IComposite>
            {
                Create.Roster(rosterTitleQuestionId: rosterTitleId, 
                            fixedTitles: new string[]{"one", "two"},
                            children: new List<IComposite>
                            {
                                Create.TextQuestion(questionId: rosterTitleId, variable: "test"),
                                Create.TextQuestion(questionId: substitutionTargetQuestionId, text: "uses %rostertitle%", variable:"subst")
                            })
               });
            QuestionnaireModel model = Create.QuestionnaireModelBuilder().BuildQuestionnaireModel(questionnaire);

            var interview = Mock.Of<IStatefulInterview>();

            rosterTitleAnswerValue = "answer";
            var rosterTitleSubstitutionService = new Mock<IRosterTitleSubstitutionService>();
            rosterTitleSubstitutionService.Setup(x => x.Substitute(Moq.It.IsAny<string>(), Moq.It.IsAny<Identity>(), Moq.It.IsAny<string>()))
                .Returns<string, Identity, string>((title, id, interviewId) => title.Replace("%rostertitle%", rosterTitleAnswerValue));

            var questionnaireRepository = new Mock<IPlainKeyValueStorage<QuestionnaireModel>>();
            questionnaireRepository.SetReturnsDefault(model);

            var interviewRepository = new Mock<IStatefulInterviewRepository>();
            interviewRepository.SetReturnsDefault(interview);

            viewModel = CreateViewModel(questionnaireRepository.Object, interviewRepository.Object, rosterTitleSubstitutionService: rosterTitleSubstitutionService.Object);
        };

        Because of = () => viewModel.Init("interview", new Identity(substitutionTargetQuestionId, new decimal[] { 1 }));

        It should_substitute_roster_title_value = () => viewModel.Title.ShouldEqual(string.Format("uses {0}", rosterTitleAnswerValue));

        static QuestionHeaderViewModel viewModel;
        static Guid substitutionTargetQuestionId;
        static string rosterTitleAnswerValue;
    }
}