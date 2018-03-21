using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using NHibernate.Util;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    internal class when_creating_export_structure_from_questionnaire_containing_gps_question: ExportViewFactoryTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            gpsQuestionId = Guid.Parse("BBF000AAA111EE2DD2EE111AAA000AAA");

            questionnaireDocument =
                Create.Entity.QuestionnaireDocument(children: Create.Entity.GpsCoordinateQuestion(questionId: gpsQuestionId, variable:"gps", label:"gps label"));

            var questionnaireMockStorage = new Mock<IQuestionnaireStorage>();
            questionnaireMockStorage.Setup(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>())).Returns(Create.Entity.PlainQuestionnaire(questionnaireDocument, 1, null));
            questionnaireMockStorage.Setup(x => x.GetQuestionnaireDocument(Moq.It.IsAny<QuestionnaireIdentity>())).Returns(questionnaireDocument);
            exportViewFactory = CreateExportViewFactory(questionnaireMockStorage.Object);
            BecauseOf();
        }

        public void BecauseOf() =>
            gpsExportedQuestionHeaderItem = exportViewFactory.CreateQuestionnaireExportStructure(questionnaireDocument.PublicKey, 1).HeaderToLevelMap[new ValueVector<Guid>()].HeaderItems[gpsQuestionId] as ExportedQuestionHeaderItem;

        [NUnit.Framework.Test] public void should_create_header_with_5_columns_wich_corresponds_to_gps_properties () =>
            gpsExportedQuestionHeaderItem.ColumnHeaders.Select(x=> x.Name).Should().BeEquivalentTo(new[] { "gps__Latitude", "gps__Longitude", "gps__Accuracy", "gps__Altitude" , "gps__Timestamp"});

        [NUnit.Framework.Test] public void should_create_header_with_5_columns_with_valid_labels () =>
            gpsExportedQuestionHeaderItem.ColumnHeaders.Select(x=> x.Title).Should().BeEquivalentTo(new[] { "gps label: Latitude", "gps label: Longitude", "gps label: Accuracy", "gps label: Altitude", "gps label: Timestamp" });

        private static ExportViewFactory exportViewFactory;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid gpsQuestionId;
        private static ExportedQuestionHeaderItem gpsExportedQuestionHeaderItem;
    }
}
