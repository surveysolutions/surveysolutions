using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using it = Moq.It;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionnaireQuestionInfoFactoryTests
{
    internal class when_loading_list_of_geo_questions : QuestionnaireQuestionInfoFactoryTestContext
    {
        private Establish context = () =>
        {
            var questionnaire = new QuestionnaireDocument()
            {
                Children = new List<IComposite>()
                {
                    CreateQuestion(QuestionType.TextList),
                    CreateQuestion(QuestionType.Text),
                    CreateQuestion(QuestionType.SingleOption),
                    CreateQuestion(QuestionType.QRBarcode),
                    CreateQuestion(QuestionType.Numeric),
                    CreateQuestion(QuestionType.MultyOption),
                    CreateQuestion(QuestionType.GpsCoordinates, questionId: geoQuestionId),
                    CreateQuestion(QuestionType.DateTime)
                }.ToReadOnlyCollection()
            };

            factory = CreateQuestionnaireQuestionInfoFactory(questionnaire);

            input = CreateQuestionnaireQuestionInfoInputModel(questionnaireId: questionnaireId, version: version,
                questionType: QuestionType.GpsCoordinates);
        };

        Because of = () => view = factory.Load(input);

        It should_return_view_with_one_item_in_Variables_collection = () =>
            view.Variables.Count().ShouldEqual(1);

        It should_return_view_with_geoQuestionId_as_id_only = () =>
            view.Variables.Select(x => x.Id).ShouldContainOnly(new[] { geoQuestionId });

        It should_set__GpsCoordinates__as_question_type_into_record_with_id_equals_geoQuestionId = () =>
            view.Variables.Single(x => x.Id == geoQuestionId).Type.ShouldEqual(QuestionType.GpsCoordinates);

        It should_set__GpsCoordinates__as_variable_into_record_with_id_equals_geoQuestionId = () =>
            view.Variables.Single(x => x.Id == geoQuestionId).Variable.ShouldEqual("GpsCoordinates");

        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static Guid geoQuestionId = Guid.Parse("22222222222222222222222222222222");
        private const long version = 8;
        private static QuestionnaireQuestionInfoFactory factory;
        private static QuestionnaireQuestionInfoInputModel input;
        private static QuestionnaireQuestionInfoView view;
    }
}
