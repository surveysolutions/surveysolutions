using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.QuestionnaireTests
{
    internal class when_importing_questionannaire_and_question_in_it_has_custom_validation_involving_another_question_with_deeper_propagation_level
        : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            creatorId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            Guid propagatableGroupId = Guid.Parse("22222222222222222222222222222222");

            string propagatedQuestionId = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
            string validatedQuestionId = "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB";
            string validationExpression = "[BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB] > [AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA]";

            var expressionProcessor = Mock.Of<IExpressionProcessor>(processor
                => processor.GetIdentifiersUsedInExpression(validationExpression) == new [] { propagatedQuestionId, validatedQuestionId });

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IExpressionProcessor>())
                .Returns(expressionProcessor);

            questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(new IComposite[]
            {
                new AutoPropagateQuestion
                {
                    Triggers = new List<Guid> {propagatableGroupId},
                },
                new Group
                {
                    PublicKey = propagatableGroupId,
                    Propagated = Propagate.AutoPropagated,
                    Children = new List<IComposite>
                    {
                        new NumericQuestion
                        {
                            PublicKey = Guid.Parse(propagatedQuestionId),
                        }
                    }
                },
                new NumericQuestion
                {
                    PublicKey = Guid.Parse(validatedQuestionId),
                    ValidationExpression = validationExpression,
                },
            });

            questionnaire = CreateQuestionnaire();
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.ImportQuestionnaire(creatorId, questionnaireDocument));

        It should_throw_questionnaire_exception = () =>
            exception.ShouldBeOfType<QuestionnaireException>();

        It should_throw_exception_with_message_containing__custom_validation__ = () =>
            exception.Message.ShouldContain("custom validation");

        It should_throw_exception_with_message_containing__propagation_level__ = () =>
            exception.Message.ShouldContain("propagation level");

        private static Exception exception;
        private static Questionnaire questionnaire;
        private static Guid creatorId;
        private static QuestionnaireDocument questionnaireDocument;
    }
}