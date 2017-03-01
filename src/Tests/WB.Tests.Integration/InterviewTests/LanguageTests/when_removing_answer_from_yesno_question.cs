using System;
using AppDomainToolkit;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Integration.InterviewTests.LanguageTests
{
    internal class when_removing_answer_from_yesno_question : InterviewTestsContext
    {
        private AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        [SetUp]
        public void BeforeTest() => this.appDomainContext = AppDomainContext.Create();

        [TearDown]
        public void AfterTest() => this.appDomainContext.Dispose();

        [Test]
        public void should_dependent_variable_update_value()
        {
            var variableValue = Execute.InStandaloneAppDomain(this.appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();
                var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var variableId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
                {
                    Create.YesNoQuestion(questionId: questionId, variable: "hhAssets", answers: new[] {11}),
                    Create.Variable(variableId, VariableType.LongInteger, expression: "hhAssets.Missing.Length")
                });

                var interview = SetupStatefullInterview(questionnaireDocument);

                interview.AnswerYesNoQuestion(Create.Command.AnswerYesNoQuestion(
                    questionId: questionId,
                    rosterVector: RosterVector.Empty,
                    answeredOptions: new[] {Create.AnsweredYesNoOption(11m, true)},
                    answerTime: DateTime.UtcNow));

                interview.AnswerYesNoQuestion(Create.Command.AnswerYesNoQuestion(
                    questionId: questionId,
                    rosterVector: RosterVector.Empty,
                    answeredOptions: new AnsweredYesNoOption[] {},
                    answerTime: DateTime.UtcNow));

                return (long) interview.GetVariableValueByOrDeeperRosterLevel(variableId, RosterVector.Empty);
            });

            Assert.That(variableValue, Is.EqualTo(1));
        }
    }
}