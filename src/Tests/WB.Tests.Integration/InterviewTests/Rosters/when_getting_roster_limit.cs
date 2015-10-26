using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Integration.InterviewTests.Rosters
{
    public class when_getting_roster_limit
    {
        Establish context = () =>
        {
            var questionnaire = Create.QuestionnaireDocument(Guid.NewGuid(),
                Create.Chapter(children: new List<IComposite>
                {
                    new TextListQuestion("Question TL") { PublicKey = textListId, QuestionType = QuestionType.TextList, StataExportCaption = "test" },
                    Create.Roster(rosterSizeQuestionId: textListId)
                })
            );

            plaineQuestionnaire = new PlainQuestionnaire(questionnaire, 1);
            questionnaireModel = new QuestionnaireModelBuilder().BuildQuestionnaireModel(questionnaire);
        };

        Because of = () =>
        {
            plaineQuestionnaireRosterLimit = plaineQuestionnaire.GetMaxRosterRowCount();
            questionnaireModelRosterLimit = questionnaireModel.GetTextListQuestion(textListId).MaxAnswerCount.Value;
        };

        It should_return_the_same_limit_for_roster_in_plane_questionnaire = () =>
            plaineQuestionnaireRosterLimit.ShouldEqual(Constants.MaxRosterRowCount);

        It should_return_the_same_limit_for_rosters_in_questionnaire_model_builder = () =>
           questionnaireModelRosterLimit.ShouldEqual(Constants.MaxRosterRowCount);

        It should_return_the_same_limit_for_integer_question = () =>
            IntegerQuestionViewModel.RosterUpperBoundDefaultValue.ShouldEqual(Constants.MaxRosterRowCount);

        private static int plaineQuestionnaireRosterLimit;
        private static int questionnaireModelRosterLimit;

        private static QuestionnaireModel questionnaireModel;
        private static PlainQuestionnaire plaineQuestionnaire;
        private static readonly Guid textListId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}
