extern alias datacollection;

using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

using TemplateImported = datacollection::Main.Core.Events.Questionnaire.TemplateImported;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.QuestionnaireQuestionsInfoDenormalizerTests
{
    internal class when_importing_questionnaire_with_7_numeric_questions_in_all_roster_types_with_neted_groups : QuestionnaireQuestionsInfoDenormalizerTestContext
    {
        Establish context = () =>
        {
            importeDocument = CreateQuestionnaireDocumentWith7NumericQuestionsInDifferentRosters(questionnaireId,
                textAId, textBId, textCId, textDId, textEId, textFId, textGId, textHId, textIId,
                textAVar, textBVar, textCVar, textDVar, textEVar, textFVar, textGVar, textHVar, textIVar);
            questionnaireQuestionsInfoWriter = new Mock<IReadSideKeyValueStorage<QuestionnaireQuestionsInfo>>();

            questionnaireQuestionsInfoWriter
                .Setup(x => x.Store(Moq.It.IsAny<QuestionnaireQuestionsInfo>(), "33332222111100000000111122223333-1"))
                .Callback((QuestionnaireQuestionsInfo info, string id) =>
                {
                    questionsInfo = info;
                    questionsInfoId = id;
                });

            denormalizer = CreateQuestionnaireQuestionsInfoDenormalizer(questionnaireQuestionsInfoWriter.Object);
            evnt = CreateTemplateImportedEvent(importeDocument);
        };

        Because of = () =>
            denormalizer.Handle(evnt);

        It should_create_one_view_with_mapping_of_question_id_on_variable_name = () =>
            questionsInfo.ShouldNotBeNull();

        It should_create_one_view_with_id_equal_to_combination_of_questionnaireId_and_event_sequence = () =>
          questionsInfoId.ShouldEqual(RepositoryKeysHelper.GetVersionedKey(questionnaireId, evnt.EventSequence));

        It should_contains_all_numeric_keys_and_corresponding_variables_in_stored_view = () =>
            questionsInfo.QuestionIdToVariableMap.ShouldContain(
                new KeyValuePair<Guid, string>(textAId, textAVar),
                new KeyValuePair<Guid, string>(textBId, textBVar),
                new KeyValuePair<Guid, string>(textCId, textCVar),
                new KeyValuePair<Guid, string>(textDId, textDVar),
                new KeyValuePair<Guid, string>(textEId, textEVar),
                new KeyValuePair<Guid, string>(textFId, textFVar),
                new KeyValuePair<Guid, string>(textGId, textGVar),
                new KeyValuePair<Guid, string>(textHId, textHVar),
                new KeyValuePair<Guid, string>(textIId, textIVar)
        );
        
        private static QuestionnaireQuestionsInfo questionsInfo;
        private static Guid questionnaireId = Guid.Parse("33332222111100000000111122223333");
        private static Guid textAId = Guid.Parse("22222222222222222222222222222222");
        private static Guid textBId = Guid.Parse("33333333333333333333333333333333");
        private static Guid textCId = Guid.Parse("44444444444444444444444444444444");
        private static Guid textDId = Guid.Parse("55555555555555555555555555555555");
        private static Guid textEId = Guid.Parse("66666666666666666666666666666666");
        private static Guid textFId = Guid.Parse("77777777777777777777777777777777");
        private static Guid textGId = Guid.Parse("11111111111111111111111111111111");
        private static Guid textHId = Guid.Parse("88888888888888888888888888888888");
        private static Guid textIId = Guid.Parse("99999999999999999999999999999999");
        private static string textAVar = "textA";
        private static string textBVar = "textB";
        private static string textCVar = "textC";
        private static string textDVar = "textD";
        private static string textEVar = "textE";
        private static string textFVar = "textF";
        private static string textGVar = "textG";
        private static string textHVar = "textH";
        private static string textIVar = "textI";
        private static QuestionnaireDocument importeDocument;
        private static string questionsInfoId;
        private static QuestionnaireQuestionsInfoDenormalizer denormalizer;
        private static IPublishedEvent<TemplateImported> evnt;
        private static Mock<IReadSideKeyValueStorage<QuestionnaireQuestionsInfo>> questionnaireQuestionsInfoWriter;
    }
}