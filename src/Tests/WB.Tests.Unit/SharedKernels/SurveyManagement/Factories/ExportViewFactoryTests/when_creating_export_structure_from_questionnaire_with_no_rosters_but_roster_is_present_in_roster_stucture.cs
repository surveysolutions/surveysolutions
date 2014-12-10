using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Supervisor.Factories;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    internal class when_creating_export_structure_from_questionnaire_with_no_rosters_but_roster_is_present_in_roster_stucture : ExportViewFactoryTestsContext
    {
        Establish context = () =>
        {
            misteriousRosterGroupId = Guid.Parse("00F000AAA111EE2DD2EE111AAA000FFF");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter();

            var questionnaireRosterStructureFactory =
                Mock.Of<IQuestionnaireRosterStructureFactory>(
                    _ =>
                        _.CreateQuestionnaireRosterStructure(questionnaire, 1) ==
                            new QuestionnaireRosterStructure()
                            {
                                RosterScopes =
                                    new Dictionary<ValueVector<Guid>, RosterScopeDescription>()
                                    {
                                        {
                                            new ValueVector<Guid> { misteriousRosterGroupId },
                                            new RosterScopeDescription(new ValueVector<Guid>() { misteriousRosterGroupId }, string.Empty,
                                                RosterScopeType.Fixed, null)
                                        }
                                    }
                            });

            exportViewFactory = CreateExportViewFactory(questionnaireRosterStructureFactory);
        };

        Because of = () =>
            invalidOperationException = Catch.Exception(()=>exportViewFactory.CreateQuestionnaireExportStructure(questionnaire, 1)) as InvalidOperationException;

        It should_InvalidOperationException_be_thrown = () =>
            invalidOperationException.ShouldNotBeNull();
        
        private static ExportViewFactory exportViewFactory;
        private static InvalidOperationException invalidOperationException;
        private static QuestionnaireDocument questionnaire;
        private static Guid misteriousRosterGroupId;
    }
}
