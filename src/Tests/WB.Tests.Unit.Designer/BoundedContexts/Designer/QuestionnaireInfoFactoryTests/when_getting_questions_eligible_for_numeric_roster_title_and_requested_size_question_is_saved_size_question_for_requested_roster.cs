using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoFactoryTests
{
    internal class when_getting_questions_eligible_for_numeric_roster_title_and_requested_size_question_is_saved_size_question_for_requested_roster : QuestionnaireInfoFactoryTestContext
    {
        Establish context = () =>
        {
            questionnaireView = Create.QuestionnaireDocument(Guid.NewGuid(),
                Create.Roster(roster1Id, rosterSizeQuestionId: rosterSizeQuestionId, rosterType: RosterSizeSourceType.Question, children: new List<IComposite>()
                {
                    Create.TextQuestion(rosterTitleQuestionId),
                }),
                Create.Roster(roster2Id, rosterSizeQuestionId: rosterSizeQuestionId, rosterType: RosterSizeSourceType.Question),
                Create.NumericIntegerQuestion(rosterSizeQuestionId)
            ); 

            questionDetailsReaderMock
                .Setup(x => x.GetById(questionnaireId))
                .Returns(questionnaireView);

            factory = CreateQuestionnaireInfoFactory(questionDetailsReaderMock.Object);
        };

        Because of = () =>
            result = factory.GetQuestionsEligibleForNumericRosterTitle(questionnaireId, roster2Id, rosterSizeQuestionId);

        It should_return_2_elements_to_show_in_dropdown = () =>
            result.Count.ShouldEqual(2);

        It should_return_roster_title_questions_as_the_second_element = () => 
            result.ElementAt(1).Id.ShouldEqual(rosterTitleQuestionId.FormatGuid());

        private static QuestionnaireInfoFactory factory;
        private static List<DropdownEntityView> result;
        private static QuestionnaireDocument questionnaireView;
        private static readonly Mock<IPlainKeyValueStorage<QuestionnaireDocument>> questionDetailsReaderMock = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();
        private static string questionnaireId = "11111111111111111111111111111111";
        private static readonly Guid roster1Id = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid roster2Id = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid rosterSizeQuestionId = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid rosterTitleQuestionId = Guid.Parse("44444444444444444444444444444444");
    }
}