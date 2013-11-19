using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireDenormalizerTests
{
    internal class when_QuestionChangeEvent_with_Autopropagate_question_type_is_come : QuestionChangeEventTestContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument(questionnaireId);

            questionnaire
                .AddChapter(Guid.NewGuid())
                .AddGroup(triggeredGroupId, propagationKind: Propagate.AutoPropagated);

            questionnaire
                .AddChapter(Guid.NewGuid())
                .AddQuestion(autoQuestionId, type: QuestionType.AutoPropagate, triggers: new List<Guid>(), maxValue: 8);

            var questionFactoryMock = Mock.Of<IQuestionFactory>(factory => factory.CreateQuestion(Moq.It.IsAny<QuestionData>()) == new NumericQuestion("Text")
                    {
                        PublicKey = autoQuestionId,
                        IsInteger = true,
                    });

            var storageStub = CreateQuestionnaireDenormalizerStorageStub(questionnaire);

            denormalizer = CreateQuestionnaireDenormalizer(questionFactoryMock, storageStub);

            var evnt = CreateQuestionChangedEvent(autoQuestionId, type: QuestionType.AutoPropagate, maxValue: 10, triggers: new List<Guid> { triggeredGroupId });

            questionChangedEvent = CreatePublishedEvent(questionnaire.PublicKey, evnt);
        };

        Because of = () => denormalizer.Handle(questionChangedEvent);

        It should_not_add_Autopropagate_question_in_questionnaire = () =>
            (questionnaire.Find<IQuestion>(autoQuestionId) as AutoPropagateQuestion).ShouldBeNull();

        It should_add_Numeric_question_in_questionnaire = () =>
            (questionnaire.Find<IQuestion>(autoQuestionId) as NumericQuestion).ShouldNotBeNull();

        It should_add_Numeric_question_with_integer_flag_set_in_true = () =>
        {
            var numericQuestion = questionnaire.Find<IQuestion>(autoQuestionId) as NumericQuestion;
            numericQuestion.IsInteger.ShouldBeTrue();
        };

        private static QuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<QuestionChanged> questionChangedEvent;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionnaireId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static Guid triggeredGroupId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        private static Guid autoQuestionId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    }
}
