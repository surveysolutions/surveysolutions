using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_question_variable_depends_on_that_used_in_validation : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        protected static AppDomainContext appDomainContext;

        public void BecauseOf() => results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
        {
            SetUp.MockedServiceLocator();

            var questionId = Id.g1;
            var variableId = Id.g2;
            var dependentQuestionId = Id.g3;
            var staticTextId = Id.g4;
            

            var interview = SetupInterviewWithExpressionStorage(appDomainContext.AssemblyLoadContext, Create.Entity.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(questionId, "q1"),
                    Create.Entity.Variable(variableId, VariableType.Boolean, variableName:"v1", expression:"1/q1 < 0.1"),
                    Create.Entity.NumericIntegerQuestion(dependentQuestionId, "q2",
                        validationConditions: Create.Entity.ValidationCondition("v1.HasValue", null).ToEnumerable()),
                    Create.Entity.StaticText(staticTextId, "static text",
                        validationConditions: Create.Entity.ValidationCondition("v1.HasValue", null).ToEnumerable().ToList())
                }), new object[]
                {
                    Create.Event.NumericIntegerQuestionAnswered(
                        questionId, null, 1, null, null
                    ),
                    Create.Event.NumericIntegerQuestionAnswered(
                        dependentQuestionId, null, 1, null, null
                    ),
                    Create.Event.NumericIntegerQuestionAnswered(
                        questionId, null, 0, null, null
                    )
                });

            using (var eventContext = new EventContext())
            {
                interview.AnswerNumericIntegerQuestion(Create.Command.AnswerNumericIntegerQuestionCommand(questionId: questionId, answer: 1));
                interview.AnswerNumericIntegerQuestion(Create.Command.AnswerNumericIntegerQuestionCommand(questionId: dependentQuestionId, answer: 1));
                interview.AnswerNumericIntegerQuestion(Create.Command.AnswerNumericIntegerQuestionCommand(questionId: questionId, answer: 0));

                return new InvokeResults
                {
                    WasAnswersDeclaredInvalidEventPublishedForDependentQuestion =
                        eventContext
                            .GetSingleEventOrNull<AnswersDeclaredInvalid>()?
                            .FailedValidationConditions
                            .ContainsKey(Create.Identity(dependentQuestionId))
                        ?? false,
                    WasStaticTextDeclaredInvalidEventPublishedForDependentQuestion=
                        eventContext
                            .GetSingleEventOrNull<StaticTextsDeclaredInvalid>()?
                            //.FailedValidationConditions
                            .GetFailedValidationConditionsDictionary()
                            .ContainsKey(Create.Identity(staticTextId))
                        ?? false
                };
            }
        });

        [NUnit.Framework.Test] public void should_mark_dependent_question_as_invalid () =>
            results.WasAnswersDeclaredInvalidEventPublishedForDependentQuestion.Should().BeTrue();

        [NUnit.Framework.Test] public void should_mark_dependent_static_text_as_invalid () =>
            results.WasStaticTextDeclaredInvalidEventPublishedForDependentQuestion.Should().BeTrue();

        private static InvokeResults results;

        [Serializable]
        internal class InvokeResults
        {
            public bool WasAnswersDeclaredInvalidEventPublishedForDependentQuestion { get; set; }
            public bool WasStaticTextDeclaredInvalidEventPublishedForDependentQuestion { get; set; }
        }
    }
}
