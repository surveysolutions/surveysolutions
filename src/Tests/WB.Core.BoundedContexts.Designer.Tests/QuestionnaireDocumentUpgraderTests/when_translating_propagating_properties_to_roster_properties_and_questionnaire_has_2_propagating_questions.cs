using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireDocumentUpgraderTests
{
    internal class when_translating_propagating_properties_to_roster_properties_and_questionnaire_has_2_propagating_questions : QuestionnaireDocumentUpgraderTestsContext
    {
        Establish context = () =>
        {
            var firstAutopropagatingQuestionId = Guid.Parse("1111111111111111AAAAAAAAAAAAAAAA");
            var secondAutopropagatingQuestionId = Guid.Parse("2222222222222222AAAAAAAAAAAAAAAA");

            initialDocument = CreateQuestionnaireDocument(
                CreateGroup(children: new[]
                {
                    CreateAutoPropagatingQuestion(questionId: firstAutopropagatingQuestionId),
                    CreateAutoPropagatingQuestion(questionId: secondAutopropagatingQuestionId),
                }));

            firstQuestionFromFactory = Mock.Of<IQuestion>();
            secondQuestionFromFactory = Mock.Of<IQuestion>();

            Func<Guid, Expression<Func<QuestionData, bool>>> isNumericIntegerWithId = id => data
                => data.PublicKey == id
                && data.QuestionType == QuestionType.Numeric
                && data.IsInteger == true;

            var questionFactory = Mock.Of<IQuestionFactory>(factory
                => factory.CreateQuestion(it.Is(isNumericIntegerWithId(firstAutopropagatingQuestionId))) == firstQuestionFromFactory
                && factory.CreateQuestion(it.Is(isNumericIntegerWithId(secondAutopropagatingQuestionId))) == secondQuestionFromFactory);
            
            upgrader = CreateQuestionnaireDocumentUpgrader(questionFactory: questionFactory);
        };

        Because of = () =>
            resultDocument = upgrader.TranslatePropagatePropertiesToRosterProperties(initialDocument);

        It should_return_document_with_2_questions = () =>
            resultDocument.GetAllQuestions().Count().ShouldEqual(2);

        It should_return_document_with_question_returned_by_question_factory_using_first_autopropagating_question_id_and_numeric_integer_type = () =>
            resultDocument.GetAllQuestions().ShouldContain(firstQuestionFromFactory);

        It should_return_document_with_question_returned_by_question_factory_using_second_autopropagating_question_id_and_numeric_integer_type = () =>
            resultDocument.GetAllQuestions().ShouldContain(secondQuestionFromFactory);

        private static QuestionnaireDocument resultDocument;
        private static QuestionnaireDocumentUpgrader upgrader;
        private static QuestionnaireDocument initialDocument;
        private static IQuestion firstQuestionFromFactory;
        private static IQuestion secondQuestionFromFactory;
    }
}
