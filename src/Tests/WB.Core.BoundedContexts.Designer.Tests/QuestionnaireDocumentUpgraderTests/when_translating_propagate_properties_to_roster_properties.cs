using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireDocumentUpgraderTests
{
    [Subject(typeof(QuestionnaireDocumentUpgrader))]
    public class QuestionnaireDocumentUpgraderContext
    {
        public static QuestionnaireDocument CreateQuestionnaire()
        {
            return new QuestionnaireDocument();
        }

        internal static QuestionnaireDocumentUpgrader CreateUpgrader(IQuestionFactory questionFactory)
        {
            return new QuestionnaireDocumentUpgrader(questionFactory);
        }

        public static QuestionnaireDocument CreateQuestionnaireWith2PropagateQuestionsAndGroups(Guid firstPropagateQuestionId, Guid  secondPropagateQuestionId,
            Guid firstPropagateGroupId, Guid secondPropagateGroupId)
        {
            var questionnaire = new QuestionnaireDocument();
            var chapter = questionnaire.AddChapter(Guid.NewGuid());
            chapter.AddGroup(firstPropagateGroupId, Propagate.AutoPropagated);
            chapter.AddGroup(secondPropagateGroupId, Propagate.AutoPropagated);

            chapter.AddQuestion(firstPropagateQuestionId, QuestionType.AutoPropagate, new List<Guid>() { firstPropagateGroupId }, 5, "firstPropagateQuestion", new List<IAnswer>(), Order.AsIs, false, false);
            chapter.AddQuestion(secondPropagateQuestionId, QuestionType.AutoPropagate, new List<Guid>() { secondPropagateGroupId }, 5, "secondPropagateQuestion", new List<IAnswer>(), Order.AsIs, false, false);

            questionnaire.ConnectChildrenWithParent();
            return questionnaire;
        }
    }

    public class when_translating_propagate_properties_to_roster_properties : QuestionnaireDocumentUpgraderContext
    {
        Establish context = () =>
        {
            questionFactoryMock = new Mock<IQuestionFactory>();
            upgrader = CreateUpgrader(questionFactoryMock.Object);
            questionnaireDocument = CreateQuestionnaire();
        };

        Because of = () => upgradedDocument = upgrader.TranslatePropagatePropertiesToRosterProperties(questionnaireDocument);

        It should_return_cloned_copy_of_document = () =>
            upgradedDocument.ShouldNotBeTheSameAs(questionnaireDocument);

        private static  Mock<IQuestionFactory> questionFactoryMock;
        private static QuestionnaireDocumentUpgrader upgrader;
        private static QuestionnaireDocument questionnaireDocument;
        private static QuestionnaireDocument upgradedDocument;
    }

    public class when_translating_propagate_properties_to_roster_properties_and_questionnaire_has_2_propagate_questions : QuestionnaireDocumentUpgraderContext
    {
        Establish context = () =>
        {
            questionFactoryMock = new Mock<IQuestionFactory>();
            //questionFactoryMock.Setup()
            upgrader = CreateUpgrader(questionFactoryMock.Object);
            questionnaireDocument = CreateQuestionnaireWith2PropagateQuestionsAndGroups(
                firstPropagateQuestionId,
                secondPropagateQuestionId,
                firstPropagateGroupId,
                secondPropagateGroupId);
        };

        Because of = () => upgradedDocument = upgrader.TranslatePropagatePropertiesToRosterProperties(questionnaireDocument);

        //It should_return_cloned_copy_of_document = () =>
        //    upgradedDocument.Find<AbstractQuestion>(q => q.QuestionType == QuestionType.AutoPropagate).Count().ShouldEqual(0);

        private static Mock<IQuestionFactory> questionFactoryMock;
        private static QuestionnaireDocumentUpgrader upgrader;
        private static QuestionnaireDocument questionnaireDocument;
        private static QuestionnaireDocument upgradedDocument;
        private static Guid firstPropagateQuestionId = new Guid("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid secondPropagateQuestionId = new Guid("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid firstPropagateGroupId = new Guid("11111111111111111111111111111111");
        private static Guid secondPropagateGroupId = new Guid("22222222222222222222222222222222");
    }

    public class when_translating_propagate_properties_to_roster_properties_and_propagate_question_has_3_triggers : QuestionnaireDocumentUpgraderContext
    {
        Establish context = () =>
        {
        };
    }
}
