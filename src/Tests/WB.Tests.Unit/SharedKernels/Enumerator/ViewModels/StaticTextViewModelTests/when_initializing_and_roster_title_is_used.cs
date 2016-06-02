using System;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.StaticTextViewModelTests
{
    internal class when_initializing_and_roster_title_is_used : StaticTextViewModelTestsContext
    {
        Establish context = () =>
        {
            staticTextWithSubstitutionToRosterTitleId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var questionnaireMock = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Entity.StaticText(publicKey: staticTextWithSubstitutionToRosterTitleId, text: "uses %rostertitle%")
            }));  

            var interview = Mock.Of<IStatefulInterview>();

            rosterTitleAnswerValue = "answer";
            var rosterTitleSubstitutionService = new Mock<IRosterTitleSubstitutionService>();
            rosterTitleSubstitutionService.Setup(x => x.Substitute(Moq.It.IsAny<string>(), Moq.It.IsAny<Identity>(), Moq.It.IsAny<string>()))
                .Returns<string, Identity, string>((title, id, interviewId) => title.Replace("%rostertitle%", rosterTitleAnswerValue));

            var questionnaireRepository = new Mock<IPlainQuestionnaireRepository>();
            questionnaireRepository.Setup(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>())).Returns(questionnaireMock);

            var interviewRepository = new Mock<IStatefulInterviewRepository>();
            interviewRepository.SetReturnsDefault(interview);

            viewModel = CreateViewModel(questionnaireRepository.Object, interviewRepository.Object, rosterTitleSubstitutionService: rosterTitleSubstitutionService.Object);
        };

        Because of = () => 
            viewModel.Init("interview", new Identity(staticTextWithSubstitutionToRosterTitleId, Create.Entity.RosterVector(1)), null);

        It should_substitute_roster_title_value = () => 
            viewModel.StaticText.ShouldEqual($"uses {rosterTitleAnswerValue}");

        static StaticTextViewModel viewModel;
        static Guid staticTextWithSubstitutionToRosterTitleId;
        static string rosterTitleAnswerValue;
    }
}