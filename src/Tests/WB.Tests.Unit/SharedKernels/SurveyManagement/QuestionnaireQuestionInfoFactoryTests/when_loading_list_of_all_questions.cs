using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionnaireQuestionInfoFactoryTests
{
    internal class when_loading_list_of_all_questions : QuestionnaireQuestionInfoFactoryTestContext
    {
        private Establish context = () =>
        {
            questionnaireDocumentReaderMock = new Mock<IReadSideKeyValueStorage<QuestionnaireDocumentVersioned>>();

            var questionnaire = new QuestionnaireDocument()
            {
                Children = new List<IComposite>()
                {
                    CreateQuestion(QuestionType.TextList, questionId: listQuestionId),
                    CreateQuestion(QuestionType.Text, questionId: textQuestionId),
                    CreateQuestion(QuestionType.SingleOption, questionId: singleQuestionId),
                    CreateQuestion(QuestionType.QRBarcode, questionId: qrQuestionId),
                    CreateQuestion(QuestionType.Numeric, questionId: numericQuestionId),
                    CreateQuestion(QuestionType.MultyOption, questionId: multiQuestionId),
                    CreateQuestion(QuestionType.GpsCoordinates, questionId: geoQuestionId),
                    CreateQuestion(QuestionType.DateTime, questionId: dateQuestionId)
                }
            };

            var questionnaireVersioned = Mock.Of<QuestionnaireDocumentVersioned>(x => x.Questionnaire == questionnaire && x.Version == version);

            questionnaireDocumentReaderMock.Setup(x => x.GetById(questionnaireId.FormatGuid() + "$" + version)).Returns(questionnaireVersioned);

            factory = CreateQuestionnaireQuestionInfoFactory(questionnaireStore: questionnaireDocumentReaderMock.Object);

            input = CreateQuestionnaireQuestionInfoInputModel(questionnaireId: questionnaireId, version: version, questionType: null);
        };

        Because of = () => view = factory.Load(input);

        It should_return_view_with_one_item_in_Variables_collection = () =>
            view.Variables.Count().ShouldEqual(8);

        It should_return_view_with_geoQuestionId_as_id_only = () =>
            view.Variables.Select(x => x.Id).ShouldContainOnly(new[] { geoQuestionId, textQuestionId, listQuestionId, singleQuestionId, qrQuestionId, numericQuestionId, multiQuestionId, dateQuestionId });

        It should_set__GpsCoordinates__as_question_type_into_record_with_id_equals_geoQuestionId = () =>
            view.Variables.Single(x => x.Id == geoQuestionId).Type.ShouldEqual(QuestionType.GpsCoordinates);

        It should_set__GpsCoordinates__as_variable_into_record_with_id_equals_geoQuestionId = () =>
            view.Variables.Single(x => x.Id == geoQuestionId).Variable.ShouldEqual("GpsCoordinates");

        It should_set__Text__as_question_type_into_record_with_id_equals_textQuestionId = () =>
            view.Variables.Single(x => x.Id == textQuestionId).Type.ShouldEqual(QuestionType.Text);

        It should_set__Text__as_variable_into_record_with_id_equals_textQuestionId = () =>
            view.Variables.Single(x => x.Id == textQuestionId).Variable.ShouldEqual("Text");

        It should_set__SingleOption__as_question_type_into_record_with_id_equals_textQuestionId = () =>
            view.Variables.Single(x => x.Id == singleQuestionId).Type.ShouldEqual(QuestionType.SingleOption);

        It should_set__SingleOption__as_variable_into_record_with_id_equals_textQuestionId = () =>
            view.Variables.Single(x => x.Id == singleQuestionId).Variable.ShouldEqual("SingleOption");

        It should_set__QRBarcode__as_question_type_into_record_with_id_equals_qrQuestionId = () =>
            view.Variables.Single(x => x.Id == qrQuestionId).Type.ShouldEqual(QuestionType.QRBarcode);

        It should_set__QRBarcode__as_variable_into_record_with_id_equals_qrQuestionId = () =>
            view.Variables.Single(x => x.Id == qrQuestionId).Variable.ShouldEqual("QRBarcode");

        It should_set__Numeric__as_question_type_into_record_with_id_equals_numericQuestionId = () =>
            view.Variables.Single(x => x.Id == numericQuestionId).Type.ShouldEqual(QuestionType.Numeric);

        It should_set__Numeric__as_variable_into_record_with_id_equals_numericQuestionId = () =>
            view.Variables.Single(x => x.Id == numericQuestionId).Variable.ShouldEqual("Numeric");

        It should_set__MultyOption__as_question_type_into_record_with_id_equals_multiQuestionId = () =>
            view.Variables.Single(x => x.Id == multiQuestionId).Type.ShouldEqual(QuestionType.MultyOption);

        It should_set__MultyOption__as_variable_into_record_with_id_equals_multiQuestionId = () =>
            view.Variables.Single(x => x.Id == multiQuestionId).Variable.ShouldEqual("MultyOption");

        It should_set__DateTime__as_question_type_into_record_with_id_equals_multiQuestionId = () =>
            view.Variables.Single(x => x.Id == dateQuestionId).Type.ShouldEqual(QuestionType.DateTime);

        It should_set__DateTime__as_variable_into_record_with_id_equals_multiQuestionId = () =>
            view.Variables.Single(x => x.Id == dateQuestionId).Variable.ShouldEqual("DateTime");

        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static Guid geoQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static Guid textQuestionId = Guid.Parse("33333333333333333333333333333333");
        private static Guid listQuestionId = Guid.Parse("44444444444444444444444444444444");
        private static Guid singleQuestionId = Guid.Parse("55555555555555555555555555555555");
        private static Guid qrQuestionId = Guid.Parse("66666666666666666666666666666666");
        private static Guid numericQuestionId = Guid.Parse("77777777777777777777777777777777");
        private static Guid multiQuestionId = Guid.Parse("88888888888888888888888888888888");
        private static Guid dateQuestionId = Guid.Parse("99999999999999999999999999999999");
        private const long version = 8;
        private static QuestionnaireQuestionInfoFactory factory;
        private static QuestionnaireQuestionInfoInputModel input;
        private static QuestionnaireQuestionInfoView view;
        private static Mock<IReadSideKeyValueStorage<QuestionnaireDocumentVersioned>> questionnaireDocumentReaderMock;
    }
}