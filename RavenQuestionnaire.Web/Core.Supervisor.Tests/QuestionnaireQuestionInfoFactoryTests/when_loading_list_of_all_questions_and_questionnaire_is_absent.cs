using System;
using System.Linq;
using Core.Supervisor.Views.Reposts.Factories;
using Core.Supervisor.Views.Reposts.InputModels;
using Core.Supervisor.Views.Reposts.Views;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using It = Machine.Specifications.It;

namespace Core.Supervisor.Tests.QuestionnaireQuestionInfoFactoryTests
{
    internal class when_loading_list_of_all_questions_and_questionnaire_is_absent : QuestionnaireQuestionInfoFactoryTestContext
    {
        private Establish context = () =>
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);

            factory = CreateQuestionnaireQuestionInfoFactory();

            input = CreateQuestionnaireQuestionInfoInputModel(questionnaireId: questionnaireId, version: version, questionType:null);
        };

        Because of = () => view = factory.Load(input);

        It should_return_not_null_view = () =>
            view.ShouldNotBeNull();

        It should_return_view_without_any_item_in_Variables_collection = () =>
            view.Variables.Count().ShouldEqual(0);

        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private const long version = 8;
        private static QuestionnaireQuestionInfoFactory factory;
        private static QuestionnaireQuestionInfoInputModel input;
        private static QuestionnaireQuestionInfoView view;
    }
}