using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    internal class when_creating_export_structure_from_questionnaire_with_no_rosters_but_roster_is_present_in_roster_stucture : ExportViewFactoryTestsContext
    {
        private Establish context = () =>
        {
            misteriousRosterGroupId = Guid.Parse("00F000AAA111EE2DD2EE111AAA000FFF");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter();

            var rosterScopes =
                new Dictionary<ValueVector<Guid>, RosterScopeDescription>()
                {
                    {
                        new ValueVector<Guid> {misteriousRosterGroupId},
                        new RosterScopeDescription(new ValueVector<Guid>() {misteriousRosterGroupId}, string.Empty,
                            RosterScopeType.Fixed, null)
                    }
                };


            var questionnaireMock = new Mock<IQuestionnaire>();
            questionnaireMock.Setup(x => x.GetRosterScopes()).Returns(rosterScopes);

            var questionnaireMockStorage = new Mock<IQuestionnaireStorage>();
            questionnaireMockStorage.Setup(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>())).Returns(questionnaireMock.Object);
            questionnaireMockStorage.Setup(x => x.GetQuestionnaireDocument(Moq.It.IsAny<QuestionnaireIdentity>())).Returns(questionnaire);
            exportViewFactory = CreateExportViewFactory(questionnaireMockStorage.Object);
        };

        Because of = () =>
            invalidOperationException = Catch.Exception(() => exportViewFactory.CreateQuestionnaireExportStructure(new QuestionnaireIdentity(questionnaireId, 1))) as InvalidOperationException;

        It should_InvalidOperationException_be_thrown = () =>
            invalidOperationException.ShouldNotBeNull();
        
        private static ExportViewFactory exportViewFactory;
        private static InvalidOperationException invalidOperationException;
        private static QuestionnaireDocument questionnaire;
        private static Guid misteriousRosterGroupId;
        private static Guid questionnaireId = Guid.Parse("FFF000AAA111EE2DD2EE111AAA000AAA");
    }
}
