using System;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.QuestionHeaderViewModelTests
{
    internal class when_initializing : QuestionHeaderViewModelTestsContext
    {
        Establish context = () =>
        {
            substitutedQuesiton = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            substitutionTargetId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
          
            var plainQuestionnaire = new PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.NumericIntegerQuestion(id: substitutionTargetId, questionText: "title with %subst%"),
                Create.Entity.TextQuestion(substitutedQuesiton, variable: "subst")
            }), 0);

            var questionnaireRepository = Mock.Of<IQuestionnaireStorage>(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>()) == plainQuestionnaire);

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: plainQuestionnaire);
            interview.AnswerTextQuestion(Guid.NewGuid(), substitutedQuesiton, RosterVector.Empty, DateTime.Now, "answer");

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(Moq.It.IsAny<string>()) == interview);

            viewModel = CreateViewModel(questionnaireRepository, interviewRepository);
        };

        Because of = () => 
            viewModel.Init("interview", new Identity(substitutionTargetId, Empty.RosterVector));

        It should_substitute_question_titles = () => 
            viewModel.Title.HtmlText.ShouldEqual("title with answer");

        static QuestionHeaderViewModel viewModel;
        private static Guid substitutionTargetId;
        private static Guid substitutedQuesiton;
    }
}

