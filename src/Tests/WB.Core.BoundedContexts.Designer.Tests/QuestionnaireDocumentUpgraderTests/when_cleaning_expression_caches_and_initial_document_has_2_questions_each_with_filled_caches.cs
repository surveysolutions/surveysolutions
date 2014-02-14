using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireDocumentUpgraderTests
{
    internal class when_cleaning_expression_caches_and_initial_document_has_2_questions_each_with_filled_caches : QuestionnaireDocumentUpgraderTestsContext
    {
        Establish context = () =>
        {
            initialDocument = CreateQuestionnaireDocument(new IComposite[]
            {
                new TextQuestion
                {
                    ConditionalDependentQuestions = new List<Guid> { Guid.Parse("11111111111111111111111111111111") },
                    ConditionalDependentGroups = new List<Guid> { Guid.Parse("22222222222222222222222222222222") },
                    QuestionsWhichCustomValidationDependsOnQuestion = new List<Guid> { Guid.Parse("33333333333333333333333333333333") },
                    QuestionIdsInvolvedInCustomEnablementConditionOfQuestion = new List<Guid> { Guid.Parse("44444444444444444444444444444444") },
                    QuestionIdsInvolvedInCustomValidationOfQuestion = new List<Guid> { Guid.Parse("55555555555555555555555555555555") },
                },
                new NumericQuestion
                {
                    ConditionalDependentQuestions = new List<Guid> { Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA") },
                    ConditionalDependentGroups = new List<Guid> { Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB") },
                    QuestionsWhichCustomValidationDependsOnQuestion = new List<Guid> { Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC") },
                    QuestionIdsInvolvedInCustomEnablementConditionOfQuestion = new List<Guid> { Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD") },
                    QuestionIdsInvolvedInCustomValidationOfQuestion = new List<Guid> { Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE") },
                },
            });

            upgrader = CreateQuestionnaireDocumentUpgrader();
        };


        Because of = () =>
            resultDocument = upgrader.CleanExpressionCaches(initialDocument);


        It should_return_cloned_copy_of_document = () =>
            resultDocument.ShouldNotBeTheSameAs(initialDocument);


        It should_set_ConditionalDependentQuestions_cache_of_first_question_to_null = () =>
            GetFirstQuestion(resultDocument).ConditionalDependentQuestions.ShouldEqual(null);

        It should_set_ConditionalDependentGroups_cache_of_first_question_to_null = () =>
            GetFirstQuestion(resultDocument).ConditionalDependentGroups.ShouldEqual(null);

        It should_set_QuestionsWhichCustomValidationDependsOnQuestion_cache_of_first_question_to_null = () =>
            GetFirstQuestion(resultDocument).QuestionsWhichCustomValidationDependsOnQuestion.ShouldEqual(null);

        It should_set_QuestionIdsInvolvedInCustomEnablementConditionOfQuestion_cache_of_first_question_to_null = () =>
            GetFirstQuestion(resultDocument).QuestionIdsInvolvedInCustomEnablementConditionOfQuestion.ShouldEqual(null);

        It should_set_QuestionIdsInvolvedInCustomValidationOfQuestion_cache_of_first_question_to_null = () =>
            GetFirstQuestion(resultDocument).QuestionIdsInvolvedInCustomValidationOfQuestion.ShouldEqual(null);


        It should_set_ConditionalDependentQuestions_cache_of_second_question_to_null = () =>
            GetSecondQuestion(resultDocument).ConditionalDependentQuestions.ShouldEqual(null);

        It should_set_ConditionalDependentGroups_cache_of_second_question_to_null = () =>
            GetSecondQuestion(resultDocument).ConditionalDependentGroups.ShouldEqual(null);

        It should_set_QuestionsWhichCustomValidationDependsOnQuestion_cache_of_second_question_to_null = () =>
            GetSecondQuestion(resultDocument).QuestionsWhichCustomValidationDependsOnQuestion.ShouldEqual(null);

        It should_set_QuestionIdsInvolvedInCustomEnablementConditionOfQuestion_cache_of_second_question_to_null = () =>
            GetSecondQuestion(resultDocument).QuestionIdsInvolvedInCustomEnablementConditionOfQuestion.ShouldEqual(null);

        It should_set_QuestionIdsInvolvedInCustomValidationOfQuestion_cache_of_second_question_to_null = () =>
            GetSecondQuestion(resultDocument).QuestionIdsInvolvedInCustomValidationOfQuestion.ShouldEqual(null);


        private static QuestionnaireDocumentUpgrader upgrader;
        private static QuestionnaireDocument initialDocument;
        private static QuestionnaireDocument resultDocument;


        private static IQuestion GetFirstQuestion(QuestionnaireDocument questionnaireDocument)
        {
            return (IQuestion) questionnaireDocument.Children[0];
        }

        private static IQuestion GetSecondQuestion(QuestionnaireDocument questionnaireDocument)
        {
            return (IQuestion) questionnaireDocument.Children[1];
        }
    }
}