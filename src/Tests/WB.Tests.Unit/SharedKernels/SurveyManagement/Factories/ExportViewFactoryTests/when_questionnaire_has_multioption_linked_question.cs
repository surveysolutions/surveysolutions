﻿using System;
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
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    internal class when_questionnaire_has_multioption_linked_question : ExportViewFactoryTestsContext
    {
        Establish context = () =>
        {
            multyOptionLinkedQuestionId = Guid.Parse("d7127d06-5668-4fa3-b255-8a2a0aaaa020");
            linkedSourceQuestionId = Guid.NewGuid();

            questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.Roster(rosterId: Guid.NewGuid(), 
                    variable: "row", 
                    fixedTitles: new[] {"1", "2"},
                    children: new[]
                    {
                        Create.Entity.TextQuestion(questionId: linkedSourceQuestionId, variable: "varTxt")
                    }),
                Create.Entity.MultyOptionsQuestion(id: multyOptionLinkedQuestionId,
                    variable: "mult",
                    linkedToQuestionId: linkedSourceQuestionId));

            var questionnaireMockStorage = new Mock<IQuestionnaireStorage>();
            questionnaireMockStorage.Setup(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>())).Returns(Create.Entity.PlainQuestionnaire(questionnaire, 1, null));
            questionnaireMockStorage.Setup(x => x.GetQuestionnaireDocument(Moq.It.IsAny<QuestionnaireIdentity>())).Returns(questionnaire);
            exportViewFactory = CreateExportViewFactory(questionnaireMockStorage.Object);
        };

        Because of = () => questionnaaireExportStructure = exportViewFactory.CreateQuestionnaireExportStructure(questionnaire.PublicKey, 1);

        It should_fill_multioption_question_header_title = () =>
        {
            HeaderStructureForLevel headerStructureForLevel = questionnaaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()];
            ExportedQuestionHeaderItem exportedQuestionHeaderItem = headerStructureForLevel.HeaderItems[multyOptionLinkedQuestionId] as ExportedQuestionHeaderItem;

            exportedQuestionHeaderItem.ColumnNames.Length.ShouldEqual(2);
            exportedQuestionHeaderItem.ColumnNames.SequenceEqual(new[] { "mult__0", "mult__1" }).ShouldBeTrue();
            exportedQuestionHeaderItem.QuestionSubType.ShouldEqual(QuestionSubtype.MultyOption_Linked);
            exportedQuestionHeaderItem.QuestionType.ShouldEqual(QuestionType.MultyOption);
        };

        static ExportViewFactory exportViewFactory;
        static QuestionnaireExportStructure questionnaaireExportStructure;
        static QuestionnaireDocument questionnaire;
        static Guid multyOptionLinkedQuestionId;
        static Guid linkedSourceQuestionId;
    }
}