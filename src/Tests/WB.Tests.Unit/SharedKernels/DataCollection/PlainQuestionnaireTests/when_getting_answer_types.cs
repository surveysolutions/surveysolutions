using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainQuestionnaireTests
{
    internal class when_getting_answer_types : PlainQuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.SingleQuestion(id: singleOptionQuestionId),
                Create.Entity.SingleQuestion(id: singleOptionLinkedOnQuestionId, linkedToQuestionId:Guid.NewGuid()),
                Create.Entity.SingleQuestion(id: singleOptionLinkedOnRosterId, linkedToRosterId:Guid.NewGuid()),
                Create.Entity.MultyOptionsQuestion(id: multiOptionQuestionId),
                Create.Entity.YesNoQuestion(questionId:multiOptionYesNoQuestionId),
                Create.Entity.MultyOptionsQuestion(id: multiOptionLinkedOnQuestionId, linkedToQuestionId:Guid.NewGuid()),
                Create.Entity.MultyOptionsQuestion(id: multiOptionLinkedOnRosterId, linkedToRosterId:Guid.NewGuid()),
                Create.Entity.NumericIntegerQuestion(id: numericIntQuestionId),
                Create.Entity.NumericRealQuestion(id: numericQuestionId),
                Create.Entity.DateTimeQuestion(questionId: dateTitleQuestionId),
                Create.Entity.GpsCoordinateQuestion(questionId: gpsQuestionId),
                Create.Entity.TextQuestion(questionId: textQuestionId),
                Create.Entity.TextListQuestion(questionId: textListQuestionId),
                Create.Entity.QRBarcodeQuestion(questionId: qrQuestionId),
                Create.Entity.MultimediaQuestion(questionId: multimediaQuestionId)
            });
        };

        Because of = () =>
            plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument, 0);

        It should_return_optioncode_answer_type_for_single_option_question = () =>
            plainQuestionnaire.GetAnswerType(singleOptionQuestionId).ShouldEqual(AnswerType.OptionCode);

        It should_return_RosterVector_answer_type_for_single_option_linked_on_question = () =>
           plainQuestionnaire.GetAnswerType(singleOptionLinkedOnQuestionId).ShouldEqual(AnswerType.RosterVector);

        It should_return_RosterVector_answer_type_for_single_option_linked_on_roster = () =>
           plainQuestionnaire.GetAnswerType(singleOptionLinkedOnRosterId).ShouldEqual(AnswerType.RosterVector);

        It should_return_OptionCodeArray_answer_type_for_multi_option_question = () =>
            plainQuestionnaire.GetAnswerType(multiOptionQuestionId).ShouldEqual(AnswerType.OptionCodeArray);

        It should_return_YesNoArray_answer_type_for_multi_option_yesno_question = () =>
           plainQuestionnaire.GetAnswerType(multiOptionYesNoQuestionId).ShouldEqual(AnswerType.YesNoArray);

        It should_return_RosterVectorArray_answer_type_for_multioption_linked_on_question = () =>
            plainQuestionnaire.GetAnswerType(multiOptionLinkedOnQuestionId).ShouldEqual(AnswerType.RosterVectorArray);

        It should_return_RosterVectorArray_answer_type_for_multioption_linked_on_roster = () =>
            plainQuestionnaire.GetAnswerType(multiOptionLinkedOnRosterId).ShouldEqual(AnswerType.RosterVectorArray);

        It should_return_Decimal_answer_type_for_numeric_real_question = () =>
            plainQuestionnaire.GetAnswerType(numericQuestionId).ShouldEqual(AnswerType.Decimal);

        It should_return_Integer_answer_type_for_numeric_int_question = () =>
            plainQuestionnaire.GetAnswerType(numericIntQuestionId).ShouldEqual(AnswerType.Integer);

        It should_return_DateTime_answer_type_for_datetime_question = () =>
           plainQuestionnaire.GetAnswerType(dateTitleQuestionId).ShouldEqual(AnswerType.DateTime);

        It should_return_GpsData_answer_type_for_gps_question = () =>
            plainQuestionnaire.GetAnswerType(gpsQuestionId).ShouldEqual(AnswerType.GpsData);

        It should_return_String_answer_type_for_tex_question = () =>
           plainQuestionnaire.GetAnswerType(textQuestionId).ShouldEqual(AnswerType.String);

        It should_return_DecimalAndStringArray_answer_type_for_textlist_question = () =>
            plainQuestionnaire.GetAnswerType(textListQuestionId).ShouldEqual(AnswerType.DecimalAndStringArray);

        It should_return_String_answer_type_for_qr_question = () =>
           plainQuestionnaire.GetAnswerType(qrQuestionId).ShouldEqual(AnswerType.String);

        It should_return_FileName_answer_type_for_multimedia_question = () =>
            plainQuestionnaire.GetAnswerType(multimediaQuestionId).ShouldEqual(AnswerType.FileName);

        private static PlainQuestionnaire plainQuestionnaire;
        private static readonly Guid singleOptionQuestionId = Guid.NewGuid();
        private static readonly Guid singleOptionLinkedOnQuestionId = Guid.NewGuid();
        private static readonly Guid singleOptionLinkedOnRosterId = Guid.NewGuid();
        private static readonly Guid multiOptionQuestionId = Guid.NewGuid();
        private static readonly Guid multiOptionYesNoQuestionId = Guid.NewGuid();
        private static readonly Guid multiOptionLinkedOnQuestionId = Guid.NewGuid();
        private static readonly Guid multiOptionLinkedOnRosterId = Guid.NewGuid();
        private static readonly Guid numericQuestionId = Guid.NewGuid();
        private static readonly Guid numericIntQuestionId = Guid.NewGuid();
        private static readonly Guid dateTitleQuestionId = Guid.NewGuid();
        private static readonly Guid gpsQuestionId = Guid.NewGuid();
        private static readonly Guid textQuestionId = Guid.NewGuid();
        private static readonly Guid textListQuestionId = Guid.NewGuid();
        private static readonly Guid qrQuestionId = Guid.NewGuid();
        private static readonly Guid multimediaQuestionId = Guid.NewGuid();
        private static QuestionnaireDocument questionnaireDocument;
    }
}