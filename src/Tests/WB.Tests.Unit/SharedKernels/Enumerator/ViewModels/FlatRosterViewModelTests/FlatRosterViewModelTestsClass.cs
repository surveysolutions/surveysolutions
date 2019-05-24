﻿using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NSubstitute;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.FlatRosterViewModelTests
{
    [TestOf(typeof(FlatRosterViewModel))]
    public class FlatRosterViewModelTestsClass : BaseMvvmCrossTest
    {
        [Test]
        public void should_read_plain_rosters_from_interview()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                chapterId: Id.g10,
                Create.Entity.NumericIntegerQuestion(Id.g1),
                Create.Entity.Roster(Id.gA, 
                    rosterSizeQuestionId: Id.g1,
                    rosterSizeSourceType: RosterSizeSourceType.Question,
                    displayMode: RosterDisplayMode.Flat,
                    children: new IComposite[]
                    {
                        Create.Entity.TextQuestion(Id.g2)
                    }));

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            interview.AnswerNumericIntegerQuestion(Create.Command.AnswerNumericIntegerQuestionCommand(questionId: Id.g1, answer: 1));

            var statefulInterviewRepository = Create.Storage.InterviewRepository(interview);

            var questionnaireStorage = Create.Storage.QuestionnaireStorage(questionnaire);
            var viewModelFactory = Substitute.For<IInterviewViewModelFactory>();
            viewModelFactory.GetNew<FlatRosterTitleViewModel>()
                .ReturnsForAnyArgs(Create.ViewModel.FlatRosterTitleViewModel(statefulInterviewRepository, questionnaireStorage));

            viewModelFactory.GetEntity(null, null, null)
                .ReturnsForAnyArgs(Create.ViewModel.TextQuestionViewModel(interviewRepository: statefulInterviewRepository,
                    questionnaireStorage: questionnaireStorage));

            ICompositeCollectionInflationService inflationService = Substitute.For<ICompositeCollectionInflationService>();
            var compositeCollection = new CompositeCollection<ICompositeEntity>();
            var textQuestionViewModel = Create.ViewModel.TextQuestionViewModel();
            compositeCollection.Add(textQuestionViewModel);

            inflationService.GetInflatedCompositeCollection(null)
                .ReturnsForAnyArgs(compositeCollection);

            var viewModel = Create.ViewModel.FlatRosterViewModel(statefulInterviewRepository, viewModelFactory, compositeCollectionInflationService: inflationService);

            // Act
            viewModel.Init(interview.EventSourceId.FormatGuid(), Create.Identity(Id.gA, 1), 
                Create.Other.NavigationState(statefulInterviewRepository, currentGroup: Create.Identity(Id.g10)));

            // Assert
            Assert.That(viewModel.RosterInstances, Does.Contain(textQuestionViewModel));
        }
    }
}
