using System;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainQuestionnaireTests
{
    internal class when_getting_answer_types : PlainQuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
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
            BecauseOf();
        }

        public void BecauseOf() =>
            plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument, 0);

        [NUnit.Framework.Test] public void should_return_optioncode_answer_type_for_single_option_question () =>
            plainQuestionnaire.GetAnswerType(singleOptionQuestionId).Should().Be(AnswerType.OptionCode);

        [NUnit.Framework.Test] public void should_return_RosterVector_answer_type_for_single_option_linked_on_question () =>
           plainQuestionnaire.GetAnswerType(singleOptionLinkedOnQuestionId).Should().Be(AnswerType.RosterVector);

        [NUnit.Framework.Test] public void should_return_RosterVector_answer_type_for_single_option_linked_on_roster () =>
           plainQuestionnaire.GetAnswerType(singleOptionLinkedOnRosterId).Should().Be(AnswerType.RosterVector);

        [NUnit.Framework.Test] public void should_return_OptionCodeArray_answer_type_for_multi_option_question () =>
            plainQuestionnaire.GetAnswerType(multiOptionQuestionId).Should().Be(AnswerType.OptionCodeArray);

        [NUnit.Framework.Test] public void should_return_YesNoArray_answer_type_for_multi_option_yesno_question () =>
           plainQuestionnaire.GetAnswerType(multiOptionYesNoQuestionId).Should().Be(AnswerType.YesNoArray);

        [NUnit.Framework.Test] public void should_return_RosterVectorArray_answer_type_for_multioption_linked_on_question () =>
            plainQuestionnaire.GetAnswerType(multiOptionLinkedOnQuestionId).Should().Be(AnswerType.RosterVectorArray);

        [NUnit.Framework.Test] public void should_return_RosterVectorArray_answer_type_for_multioption_linked_on_roster () =>
            plainQuestionnaire.GetAnswerType(multiOptionLinkedOnRosterId).Should().Be(AnswerType.RosterVectorArray);

        [NUnit.Framework.Test] public void should_return_Decimal_answer_type_for_numeric_real_question () =>
            plainQuestionnaire.GetAnswerType(numericQuestionId).Should().Be(AnswerType.Decimal);

        [NUnit.Framework.Test] public void should_return_Integer_answer_type_for_numeric_int_question () =>
            plainQuestionnaire.GetAnswerType(numericIntQuestionId).Should().Be(AnswerType.Integer);

        [NUnit.Framework.Test] public void should_return_DateTime_answer_type_for_datetime_question () =>
           plainQuestionnaire.GetAnswerType(dateTitleQuestionId).Should().Be(AnswerType.DateTime);

        [NUnit.Framework.Test] public void should_return_GpsData_answer_type_for_gps_question () =>
            plainQuestionnaire.GetAnswerType(gpsQuestionId).Should().Be(AnswerType.GpsData);

        [NUnit.Framework.Test] public void should_return_String_answer_type_for_tex_question () =>
           plainQuestionnaire.GetAnswerType(textQuestionId).Should().Be(AnswerType.String);

        [NUnit.Framework.Test] public void should_return_DecimalAndStringArray_answer_type_for_textlist_question () =>
            plainQuestionnaire.GetAnswerType(textListQuestionId).Should().Be(AnswerType.DecimalAndStringArray);

        [NUnit.Framework.Test] public void should_return_String_answer_type_for_qr_question () =>
           plainQuestionnaire.GetAnswerType(qrQuestionId).Should().Be(AnswerType.String);

        [NUnit.Framework.Test] public void should_return_FileName_answer_type_for_multimedia_question () =>
            plainQuestionnaire.GetAnswerType(multimediaQuestionId).Should().Be(AnswerType.FileName);

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
