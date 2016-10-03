using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    internal class when_questionnaire_has_yesno_question : ExportViewFactoryTestsContext
    {
        Establish context = () =>
        {
            questionId = Guid.Parse("d7127d06-5668-4fa3-b255-8a2a0aaaa020");
            questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultyOptionsQuestion(id: questionId,
                    variable: "mult",
                    yesNoView: true,
                    options: new List<Answer> {
                        Create.Entity.Answer("foo", 28), Create.Entity.Answer("bar", 42)
                    }));

            var questionnaireMockStorage = new Mock<IQuestionnaireStorage>();
            questionnaireMockStorage.Setup(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>())).Returns(new PlainQuestionnaire(questionnaire, 1, null));
            questionnaireMockStorage.Setup(x => x.GetQuestionnaireDocument(Moq.It.IsAny<QuestionnaireIdentity>())).Returns(questionnaire);
            exportViewFactory = CreateExportViewFactory(questionnaireMockStorage.Object);
        };

        Because of = () => questionnaaireExportStructure = exportViewFactory.CreateQuestionnaireExportStructure(questionnaire.PublicKey, 1);

        It should_fill_yesno_question_subtype = () =>
        {
            HeaderStructureForLevel headerStructureForLevel = questionnaaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()];
            ExportedHeaderItem exportedHeaderItem = headerStructureForLevel.HeaderItems[questionId];

            exportedHeaderItem.ColumnValues.Length.ShouldEqual(2);
            exportedHeaderItem.QuestionSubType.ShouldEqual(QuestionSubtype.MultyOption_YesNo);
        };

        static ExportViewFactory exportViewFactory;
        static QuestionnaireExportStructure questionnaaireExportStructure;
        static QuestionnaireDocument questionnaire;
        static Guid questionId;
    }
}