using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.CopyPasteTests
{
    internal class when_pasting_group_with_questions_into_group : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            groupToPasteInId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            questionnaire = CreateQuestionnaireWithOneGroup(questionnaireId : questionnaireId, groupId: groupToPasteInId, responsibleId: responsibleId);

            
            doc = Create.QuestionnaireDocument(
                Guid.Parse("31111111111111111111111111111113"),
                Create.Chapter(chapterId: chapterId, hideIfDisabled: true, children: new List<IComposite>
                {
                    Create.NumericIntegerQuestion(id: numericQuestionId, hideIfDisabled: true),
                    Create.TextListQuestion(questionId: textListQuestionId, hideIfDisabled: true),
                    Create.MultimediaQuestion(questionId: multimediaQuestionId, hideIfDisabled: true),
                    Create.QRBarcodeQuestion(questionId: qrBarcodeQuestionId, hideIfDisabled: true),
                    Create.TextQuestion(questionId: textQuestionId, hideIfDisabled: true),
                    Create.DateTimeQuestion(questionId: dateTimeQuestionId, hideIfDisabled: true),
                    Create.GpsCoordinateQuestion(questionId: gpsQuestionId, hideIfDisabled: true),
                    Create.SingleOptionQuestion(questionId: singleOptionQuestionId, hideIfDisabled: true),
                    Create.MultipleOptionsQuestion(questionId: multipleOptionsQuestionId, hideIfDisabled: true),
                    
                }));
            eventContext = new EventContext();

            command = new PasteInto(
                questionnaireId: questionnaireId,
                entityId: targetId,
                sourceItemId: chapterId,
                responsibleId: responsibleId,
                sourceQuestionnaireId: questionnaireId,
                parentId: groupToPasteInId);

            command.SourceDocument = doc;
        };

        Because of = () => 
            questionnaire.PasteInto(command);

        It should_clone_group =
            () => eventContext.ShouldContainEvent<GroupCloned>();

        It should_raise_GroupCloned_event_with_correct_hideIfDisabled_flag_for_group = () =>
            eventContext.GetEvents<GroupCloned>().Single(e => e.SourceGroupId == chapterId).HideIfDisabled.ShouldEqual(true);
        
        It should_raise_QuestionCloned_event_with_correct_hideIfDisabled_flag_for_numeric_question = () =>
            eventContext.GetEvents<NumericQuestionCloned>().Single(e => e.SourceQuestionId == numericQuestionId).HideIfDisabled.ShouldEqual(true);
        
        It should_raise_QuestionCloned_event_with_correct_hideIfDisabled_flag_for_textList_question = () =>
            eventContext.GetEvents<TextListQuestionCloned>().Single(e => e.SourceQuestionId == textListQuestionId).HideIfDisabled.ShouldEqual(true);
        
        It should_raise_QuestionCloned_event_with_correct_hideIfDisabled_flag_for_multimedia_question = () =>
        {
            var multimediaCloneEvent = eventContext.GetEvents<QuestionCloned>().Single(e => e.SourceQuestionId == multimediaQuestionId);
            multimediaCloneEvent.HideIfDisabled.ShouldEqual(true);

            eventContext.GetEvents<MultimediaQuestionUpdated>().Single(e => e.QuestionId == multimediaCloneEvent.PublicKey).HideIfDisabled.ShouldEqual(true);
        };

        It should_raise_QuestionCloned_event_with_correct_hideIfDisabled_flag_for_qrBarcode_question = () =>
            eventContext.GetEvents<QRBarcodeQuestionCloned>().Single(e => e.SourceQuestionId == qrBarcodeQuestionId).HideIfDisabled.ShouldEqual(true);
        
        It should_raise_QuestionCloned_event_with_correct_hideIfDisabled_flag_for_text_question = () =>
            eventContext.GetEvents<QuestionCloned>().Single(e => e.SourceQuestionId == textQuestionId).HideIfDisabled.ShouldEqual(true);
        
        It should_raise_QuestionCloned_event_with_correct_hideIfDisabled_flag_for_dateTime_question = () =>
            eventContext.GetEvents<QuestionCloned>().Single(e => e.SourceQuestionId == dateTimeQuestionId).HideIfDisabled.ShouldEqual(true);
        
        It should_raise_QuestionCloned_event_with_correct_hideIfDisabled_flag_for_gps_question = () =>
            eventContext.GetEvents<QuestionCloned>().Single(e => e.SourceQuestionId == gpsQuestionId).HideIfDisabled.ShouldEqual(true);
        
        It should_raise_QuestionCloned_event_with_correct_hideIfDisabled_flag_for_singleOption_question = () =>
            eventContext.GetEvents<QuestionCloned>().Single(e => e.SourceQuestionId == singleOptionQuestionId).HideIfDisabled.ShouldEqual(true);
        
        It should_raise_QuestionCloned_event_with_correct_hideIfDisabled_flag_for_multipleOptions_question = () =>
            eventContext.GetEvents<QuestionCloned>().Single(e => e.SourceQuestionId == multipleOptionsQuestionId).HideIfDisabled.ShouldEqual(true);
        
        static Questionnaire questionnaire;
        static Guid groupToPasteInId;

        static Guid chapterId = Guid.Parse("CCDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        static Guid questionnaireId = Guid.Parse("CCDDDDDDDDDDDDDDDDDDDDDDDDDDDDDA");

        static Guid numericQuestionId = Guid.NewGuid();
        static Guid textListQuestionId = Guid.NewGuid();
        static Guid multimediaQuestionId = Guid.NewGuid();
        static Guid qrBarcodeQuestionId = Guid.NewGuid();
        static Guid textQuestionId = Guid.NewGuid();
        static Guid dateTimeQuestionId = Guid.NewGuid();
        static Guid gpsQuestionId = Guid.NewGuid();
        static Guid singleOptionQuestionId = Guid.NewGuid();
        static Guid multipleOptionsQuestionId = Guid.NewGuid();

        static EventContext eventContext;
        static Guid responsibleId;
        static Guid targetId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

        private static QuestionnaireDocument doc;
        private static PasteInto command;
    }
}

