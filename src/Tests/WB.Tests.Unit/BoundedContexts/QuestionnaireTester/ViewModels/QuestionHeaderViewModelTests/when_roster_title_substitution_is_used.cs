using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Moq;
using NSubstitute;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.QuestionHeaderViewModelTests
{
    public class when_roster_title_substitution_is_used : QuestionHeaderViewModelTestsContext
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
                                Create.TextQuestion(questionId: rosterTitleId, variableName: "test"),
                                Create.TextQuestion(questionId: substitutionTargetQuestionId, text: "uses %rostertitle%", variableName:"subst")
                            })
               });
            QuestionnaireModel model = null;

            var modelStorage = new Mock<IPlainKeyValueStorage<QuestionnaireModel>>();
            modelStorage.Setup(x => x.Store(Moq.It.IsAny<QuestionnaireModel>(), Moq.It.IsAny<string>()))
                .Callback<QuestionnaireModel, string>((q, id) => { model = q; });

            var importService = Create.QuestionnaireImportService(modelStorage.Object);
            importService.ImportQuestionnaire(questionnaire, null);

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