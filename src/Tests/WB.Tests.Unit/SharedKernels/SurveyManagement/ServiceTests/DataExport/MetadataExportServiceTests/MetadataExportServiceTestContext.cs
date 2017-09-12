using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Ddi;
using WB.Core.BoundedContexts.Headquarters.DataExport.Ddi.Impl;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.MetadataExportServiceTests
{
    [Subject(typeof (DdiMetadataFactory))]
    internal class MetadataExportServiceTestContext
    {
        protected static DdiMetadataFactory CreateMetadataExportService(
            QuestionnaireDocument questionnaireDocument,
            IMetaDescriptionFactory metaDescriptionFactory = null,
            IQuestionnaireLabelFactory questionnaireLabelFactory=null)
        {
            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>())).Returns<string, string>(Path.Combine);

            return new DdiMetadataFactory(
                fileSystemAccessor.Object,
                Mock.Of<ILogger>(),
                metaDescriptionFactory ?? Mock.Of<IMetaDescriptionFactory>(),
                questionnaireLabelFactory ?? new QuestionnaireLabelFactory(),
                Mock.Of<IQuestionnaireStorage>(
                    _ =>
                        _.GetQuestionnaireDocument(Moq.It.IsAny<QuestionnaireIdentity>()) == questionnaireDocument),
                Mock.Of<IQuestionnaireExportStructureStorage>(
                    _ =>
                        _.GetQuestionnaireExportStructure(Moq.It.IsAny<QuestionnaireIdentity>()) ==
                        new QuestionnaireExportStructure()));
        }

        protected static HeaderStructureForLevel CreateHeaderStructureForLevel(string levelName = "table name", string[] referenceNames = null, ValueVector<Guid> levelScopeVector = null)
        {
            return new HeaderStructureForLevel()
            {
                LevelScopeVector = levelScopeVector ?? new ValueVector<Guid>(),
                LevelName = levelName,
                LevelIdColumnName = "Id",
                IsTextListScope = referenceNames != null,
                ReferencedNames = referenceNames,
                HeaderItems =
                    new Dictionary<Guid, IExportedHeaderItem>
                    {
                        { Guid.NewGuid(), CreateExportedHeaderItem() },
                        { Guid.NewGuid(), CreateExportedHeaderItem(QuestionType.Numeric, new[] { "a" }) }
                    }
            };
        }

        protected static ExportedHeaderItem CreateExportedHeaderItem(QuestionType type = QuestionType.Text,
            string[] columnNames = null, Guid? questionId = null, params LabelItem[] labels)
        {
            return new ExportedHeaderItem()
            {
                ColumnNames = columnNames ?? new[] {"1"},
                Titles = columnNames ?? new[] {"1"},
                QuestionType = type,
                VariableName = Guid.NewGuid().ToString(),
                PublicKey = questionId ?? Guid.NewGuid(),
                Labels = labels.ToDictionary(x => Guid.NewGuid(), x => x)
            };
        }

        protected static LabelItem CreateLabelItem(string caption="caption", string title="title")
        {
            return new LabelItem() {Caption = caption, Title = title};
        }

        protected static QuestionnaireExportStructure CreateQuestionnaireExportStructure(params HeaderStructureForLevel[] levels)
        {
            var header = new Dictionary<ValueVector<Guid>, HeaderStructureForLevel>();
            if (levels != null && levels.Length > 0)
            {
                header = levels.ToDictionary((i) => i.LevelScopeVector, (i) => i);
            }
            return new QuestionnaireExportStructure() { HeaderToLevelMap = header };
        }
    }
}
