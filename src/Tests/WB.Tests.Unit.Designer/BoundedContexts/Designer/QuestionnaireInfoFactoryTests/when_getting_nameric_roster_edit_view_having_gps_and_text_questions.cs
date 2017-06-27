using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.Infrastructure.PlainStorage;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoFactoryTests
{
    internal class when_getting_nameric_roster_edit_view_having_gps_and_text_questions : QuestionnaireInfoFactoryTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionDetailsReaderMock = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();
            questionnaireView = Create.QuestionnaireDocumentWithOneChapter(children: new List<IComposite>
            {
                Create.NumericIntegerQuestion(q1Id, variable:"num_question", title: "num_question"),
                Create.Roster(rosterId: g2Id, title: "roster", variable:  "roster", rosterType: RosterSizeSourceType.Question, rosterSizeQuestionId: q1Id, children: new IComposite[]
                {
                    
                    Create.TextQuestion(q2Id, variable:"text_question"),
                    Create.GpsCoordinateQuestion(q3Id, "gps_q")
                }),
                
            });
            questionDetailsReaderMock
                .Setup(x => x.GetById(questionnaireId))
                .Returns(questionnaireView);

            factory = CreateQuestionnaireInfoFactory(questionDetailsReaderMock.Object);
            BecauseOf();
        }

        private void BecauseOf() =>
            result = factory.GetRosterEditView(questionnaireId, g2Id);
        
        [NUnit.Framework.Test] public void should_return_grouped_list_of_title_questions_size_2 () =>
            result.NumericIntegerTitles.Count.ShouldEqual(2);
       
        private static QuestionnaireInfoFactory factory;
        private static NewEditRosterView result;
        private static QuestionnaireDocument questionnaireView;
        private static Mock<IPlainKeyValueStorage<QuestionnaireDocument>> questionDetailsReaderMock;
        private static string questionnaireId = "11111111111111111111111111111111";
    }
}