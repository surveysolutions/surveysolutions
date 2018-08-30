using System;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using NSubstitute;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.Variables
{
    public class when_variable_and_question_contains_unicode_control_chars : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp]
        public void context()
        {
            appDomainContext = AppDomainContext.Create();
        }

        [SetUp]
        public void should_cut_special_chars()
        {
            result = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                QuestionnaireDocument questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                    id: QuestionnaireId,
                    children: new IComposite[]
                    {
                        Create.Entity.TextQuestion(n2Id, variable: "t1"),
                        Create.Entity.Variable(Id.g1, VariableType.String, "v1", "$\"{Convert.ToChar(0)}{t1}\""),
                        Create.Entity.StaticText(Id.g3, enablementCondition: "v1.Length > 1")
                    });

                var interview = SetupStatefullInterview(questionnaire);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerTextQuestion(userId, n2Id, new decimal[0], DateTime.Now,
                        $"1{UnicodeCharacter}");

                    var answeredEvent = eventContext.GetSingleEvent<TextQuestionAnswered>();
                    var variableChanged = eventContext.GetSingleEvent<VariablesChanged>().ChangedVariables
                        .Single(x => x.Identity.Id == Id.g1);

                    string newVariableName = variableChanged.NewValue.ToString();

                    var isEnabled = interview.IsEnabled(Create.Identity(Id.g3));

                    return new InvokeResult
                    {
                        TextQuestionContainsUnicode = answeredEvent.Answer.Contains(UnicodeCharacter),
                        VariableContainsUnicode = newVariableName.Contains(UnicodeCharacter),
                        CharsAreRemovedInExpressionStorage = isEnabled == false
                    };

                }
            });
        }

        private class InvokeResult
        {
            public bool TextQuestionContainsUnicode { get; set; }
            public bool VariableContainsUnicode { get; set; }
            public bool CharsAreRemovedInExpressionStorage { get; set; }
        }

        [NUnit.Framework.Test]
        public void should_cut_control_chars_from_text_answer() =>
                result.TextQuestionContainsUnicode.Should().Be(false);

        [NUnit.Framework.Test]
        public void should_cut_control_chars_from_variable_value() =>
            result.VariableContainsUnicode.Should().Be(false);

        [NUnit.Framework.Test]
        public void should_cut_control_chars_inside_expression_storage_and_provide_conditions_with_corrent_value() =>
            result.CharsAreRemovedInExpressionStorage.Should().Be(true);

        private static readonly Guid QuestionnaireId = Guid.Parse("10000000000000000000000000000000");
        private static readonly Guid userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        private static readonly Guid n2Id = Guid.Parse("22222222222222222222222222222222");
        private static string UnicodeCharacter = Convert.ToChar(0).ToString();
        private InvokeResult result;
        private AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        [OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }
    }
}
