using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Events.Questionnaire;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.QuestionnaireTests
{
    internal class when_questionnaire_has_question_has_enablement_condition : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(new IComposite[]
            {
                new NumericQuestion()
                {
                    PublicKey = referencedInConditionQuestionId,
                    QuestionType = QuestionType.Numeric,
                    StataExportCaption = referencedInConditionQuestionsVariableName
                },
                new NumericQuestion()
                {
                    PublicKey = questionWithConditionQuestionId,
                    QuestionType = QuestionType.Numeric,
                    ConditionExpression = "some expression"
                }
            });

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
        {
            CreateQuestionnaire(questionnaireDocument.PublicKey, questionnaireDocument);
            questionnaireDocumentFromEvent = eventContext.GetEvents<TemplateImported>().First().Source;
        };

        It should_QuestionsInvolvedInCustomEnablementConditionOfQuestion_contain_referenced_in_conditions_question = () =>
            questionnaireDocumentFromEvent.FirstOrDefault<IQuestion>(q => q.PublicKey == questionWithConditionQuestionId)
                .QuestionIdsInvolvedInCustomEnablementConditionOfQuestion[0].ShouldEqual(
                    referencedInConditionQuestionId);

        It should_raise_1_TemplateImported_event = () =>
            eventContext.GetEvents<TemplateImported>().Count(@event => @event.Source.PublicKey == questionnaireDocument.PublicKey).ShouldEqual(1);

        private static Guid questionWithConditionQuestionId = new Guid("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid referencedInConditionQuestionId = new Guid("22222222222222222222222222222222");
        private static string referencedInConditionQuestionsVariableName = "var";
        private static QuestionnaireDocument questionnaireDocument;
        private static QuestionnaireDocument questionnaireDocumentFromEvent;
        private static EventContext eventContext;
    }
}
