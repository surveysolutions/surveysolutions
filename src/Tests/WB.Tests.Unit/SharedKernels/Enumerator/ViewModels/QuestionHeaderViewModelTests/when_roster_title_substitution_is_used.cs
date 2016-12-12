using System;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.QuestionHeaderViewModelTests
{
    internal class when_roster_title_substitution_is_used : QuestionHeaderViewModelTestsContext
    {
        Establish context = () =>
        {
            substitutionTargetQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var questionnaireMock = Mock.Of<IQuestionnaire>(_
               => _.GetQuestionTitle(substitutionTargetQuestionId) == "uses %rostertitle%"
               && _.GetQuestionInstruction(substitutionTargetQuestionId) == "Instruction"
               );
        
            var plainQuestionnaire = new PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.FixedRoster(children: new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(id: substitutionTargetQuestionId, questionText: "title with %rostertitle%")
                })
            }), 0);

            var questionnaireRepository = Mock.Of<IQuestionnaireStorage>(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>()) == plainQuestionnaire);

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: plainQuestionnaire);

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(Moq.It.IsAny<string>()) == interview);

            viewModel = CreateViewModel(questionnaireRepository, interviewRepository);
        };

        Because of = () => 
            viewModel.Init("interview", new Identity(substitutionTargetQuestionId, Create.Entity.RosterVector(0)));

        It should_substitute_roster_title_value = () => 
            viewModel.Title.HtmlText.ShouldEqual("title with Fixed Roster 1");

        static QuestionHeaderViewModel viewModel;
        static Guid substitutionTargetQuestionId;
    }
}