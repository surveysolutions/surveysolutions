using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.DynamicTextViewModelTests
{
    [TestFixture]
    [TestOf(typeof(DynamicTextViewModel))]
    public class DynamicTextViewModel_InitMethodTests
    {
        [Test]
        public void should_not_append_roster_title_when_custom_roster_title_used()
        {
            var rosterId = Id.g1;
            var rosterIdentity = Create.Identity(rosterId, 1);
            const string expectedTitle = "group title without roster title";

            var interview = Mock.Of<IStatefulInterview>(x => x.GetRosterTitle(rosterIdentity) == "rosterTitle" && 
                x.GetBrowserReadyTitleHtml(rosterIdentity) == expectedTitle);
            var questionnaire = Mock.Of<IQuestionnaire>(x => x.IsRosterGroup(rosterId) == true &&
                                                             x.HasCustomRosterTitle(rosterId) == true);

            var questionnaireStorage = SetUp.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);


            var viewModel = Create.ViewModel.DynamicTextViewModel(interviewRepository: Create.Storage.InterviewRepository(interview),
                questionnaireStorage: questionnaireStorage);

            // Act
            viewModel.Init("interview", rosterIdentity);

            // Assert
            Assert.That(viewModel, Has.Property(nameof(viewModel.PlainText)).EqualTo(expectedTitle));
        }

        [Test]
        public void should_append_roster_title_for_roster()
        {
            var rosterId = Id.g1;
            var rosterIdentity = Create.Identity(rosterId, 1);
            const string expectedTitle = "group title without roster title";

            var interview = Mock.Of<IStatefulInterview>(x => x.GetRosterTitle(rosterIdentity) == "rosterTitle" &&
                x.GetBrowserReadyTitleHtml(rosterIdentity) == expectedTitle);
            var questionnaire = Mock.Of<IQuestionnaire>(x => x.IsRosterGroup(rosterId) == true);

            var questionnaireStorage = SetUp.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);


            var viewModel = Create.ViewModel.DynamicTextViewModel(interviewRepository: Create.Storage.InterviewRepository(interview),
                questionnaireStorage: questionnaireStorage);

            // Act
            viewModel.Init("interview", rosterIdentity);

            // Assert
            Assert.That(viewModel, Has.Property(nameof(viewModel.PlainText)).EqualTo(expectedTitle + " - rosterTitle"));
        }
    }
}
