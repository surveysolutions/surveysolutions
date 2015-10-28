using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Services.QuestionnaireModelBuilderTests
{
    internal class when_building_model_and_questionnaire_document_has_list_roster_without_max_value : QuestionnaireModelBuilderTestContext
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocument(children: new List<IComposite>
            {
                Create.Chapter(children: new List<IComposite>
                {
                    Create.TextListQuestion(questionId: textListId, variable: "test"),
                    Create.Roster(rosterSizeQuestionId: textListId)
                })
            });
           
            modelBuilder = CreateQuestionnaireModelBuilder();
        };

        Because of = () =>
            questionnaireModel = modelBuilder.BuildQuestionnaireModel(questionnaire);

        It should_set_40_as_MaxAnswerCount_for_list_question = () =>
            questionnaireModel.GetTextListQuestion(textListId).MaxAnswerCount.ShouldEqual(40);

        private static QuestionnaireModel questionnaireModel;
        private static QuestionnaireModelBuilder modelBuilder;
        private static readonly Guid textListId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static QuestionnaireDocument questionnaire;
    }
}