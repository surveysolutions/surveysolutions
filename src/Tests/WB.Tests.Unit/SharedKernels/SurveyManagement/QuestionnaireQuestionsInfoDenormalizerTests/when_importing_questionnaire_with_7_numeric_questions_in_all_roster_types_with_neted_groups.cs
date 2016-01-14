extern alias datacollection;
using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.EventHandler.WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using TemplateImported = datacollection::Main.Core.Events.Questionnaire.TemplateImported;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionnaireQuestionsInfoDenormalizerTests
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
                .Setup(x => x.Store(Moq.It.IsAny<QuestionnaireQuestionsInfo>(), "33332222111100000000111122223333$1"))
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
          questionsInfoId.ShouldEqual(new QuestionnaireIdentity(questionnaireId, evnt.EventSequence).ToString());

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

        static QuestionnaireQuestionsInfo questionsInfo;
        static Guid questionnaireId = Guid.Parse("33332222111100000000111122223333");
        static Guid textAId = Guid.Parse("22222222222222222222222222222222");
        static Guid textBId = Guid.Parse("33333333333333333333333333333333");
        static Guid textCId = Guid.Parse("44444444444444444444444444444444");
        static Guid textDId = Guid.Parse("55555555555555555555555555555555");
        static Guid textEId = Guid.Parse("66666666666666666666666666666666");
        static Guid textFId = Guid.Parse("77777777777777777777777777777777");
        static Guid textGId = Guid.Parse("11111111111111111111111111111111");
        static Guid textHId = Guid.Parse("88888888888888888888888888888888");
        static Guid textIId = Guid.Parse("99999999999999999999999999999999");
        static string textAVar = "textA";
        static string textBVar = "textB";
        static string textCVar = "textC";
        static string textDVar = "textD";
        static string textEVar = "textE";
        static string textFVar = "textF";
        static string textGVar = "textG";
        static string textHVar = "textH";
        static string textIVar = "textI";
        static QuestionnaireDocument importeDocument;
        static string questionsInfoId;
        static QuestionnaireQuestionsInfoDenormalizer denormalizer;
        static IPublishedEvent<TemplateImported> evnt;
        static Mock<IReadSideKeyValueStorage<QuestionnaireQuestionsInfo>> questionnaireQuestionsInfoWriter;
    }
}