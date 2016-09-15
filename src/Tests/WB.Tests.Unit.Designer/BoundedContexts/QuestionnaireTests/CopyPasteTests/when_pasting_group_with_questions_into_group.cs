using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
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
                    Create.StaticText(staticTextId:staticTextId, hideIfDisabled:true)
                }
                ));

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

        It should_clone_group = () => 
            questionnaire.QuestionnaireDocument.Find<IGroup>(targetId).ShouldNotBeNull();

        It should_raise_GroupCloned_event_with_correct_hideIfDisabled_flag_for_group = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(targetId).HideIfDisabled.ShouldEqual(true);
        
        It should_raise_QuestionCloned_event_with_correct_hideIfDisabled_flag_for_numeric_question = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(targetId).Children.OfType<INumericQuestion>().Single().HideIfDisabled.ShouldEqual(true);
        
        It should_raise_QuestionCloned_event_with_correct_hideIfDisabled_flag_for_textList_question = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(targetId).Children.OfType<ITextListQuestion>().Single().HideIfDisabled.ShouldEqual(true);
        
        It should_raise_QuestionCloned_event_with_correct_hideIfDisabled_flag_for_multimedia_question = () =>
        {
            var multimediaCloneEvent = questionnaire.QuestionnaireDocument.Find<IGroup>(targetId).Children.OfType<IMultimediaQuestion>().Single();
            multimediaCloneEvent.HideIfDisabled.ShouldEqual(true);
        };

        It should_raise_QuestionCloned_event_with_correct_hideIfDisabled_flag_for_qrBarcode_question = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(targetId).Children.OfType<IQRBarcodeQuestion>().Single().HideIfDisabled.ShouldEqual(true);
        
        It should_raise_QuestionCloned_event_with_correct_hideIfDisabled_flag_for_text_question = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(targetId).Children.OfType<TextQuestion>().Single().HideIfDisabled.ShouldEqual(true);
        
        It should_raise_QuestionCloned_event_with_correct_hideIfDisabled_flag_for_dateTime_question = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(targetId).Children.OfType<DateTimeQuestion>().Single().HideIfDisabled.ShouldEqual(true);
        
        It should_raise_QuestionCloned_event_with_correct_hideIfDisabled_flag_for_gps_question = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(targetId).Children.OfType<GpsCoordinateQuestion>().Single().HideIfDisabled.ShouldEqual(true);
        
        It should_raise_QuestionCloned_event_with_correct_hideIfDisabled_flag_for_singleOption_question = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(targetId).Children.OfType<SingleQuestion>().Single().HideIfDisabled.ShouldEqual(true);
        
        It should_raise_QuestionCloned_event_with_correct_hideIfDisabled_flag_for_multipleOptions_question = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(targetId).Children.OfType<IMultyOptionsQuestion>().Single().HideIfDisabled.ShouldEqual(true);

        It should_raise_StaticTextCloned_event_with_correct_hideIfDisabled_flag_for_static_Text = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(targetId).Children.OfType<IStaticText>().Single().HideIfDisabled.ShouldEqual(true);

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
        static Guid staticTextId = Guid.NewGuid();

        static Guid responsibleId;
        static Guid targetId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

        private static QuestionnaireDocument doc;
        private static PasteInto command;
    }
}

